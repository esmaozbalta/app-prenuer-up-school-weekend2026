using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Archi.Api.Contracts.Auth;
using Archi.Api.Contracts.Common;
using Archi.Api.Contracts.Profile;
using Archi.Api.Data;
using Archi.Api.Models;
using Archi.Api.Security;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql
            .EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null)
            .CommandTimeout(120)));

var jwtSettings = builder.Configuration.GetSection("Jwt");
var signingKey = jwtSettings["SigningKey"] ??
                 throw new InvalidOperationException("Jwt:SigningKey is missing.");
var issuer = jwtSettings["Issuer"] ??
             throw new InvalidOperationException("Jwt:Issuer is missing.");
var audience = jwtSettings["Audience"] ??
               throw new InvalidOperationException("Jwt:Audience is missing.");
var jwtKeyId = jwtSettings["KeyId"] ?? "archi-symmetric-v1";
var signingSecurityKey = CreateJwtSigningKey(signingKey, jwtKeyId);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Keep JWT claim names as-is (e.g. "sub") for minimal APIs and manual claim reads.
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
            IssuerSigningKey = signingSecurityKey
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", corsPolicy =>
    {
        corsPolicy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Archi API V1");
    c.RoutePrefix = string.Empty;
});
app.MapGet("/swagger", () => Results.Redirect("/"));

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/v1/auth/register", async (
    RegisterRequest request,
    AppDbContext dbContext,
    IConfiguration configuration,
    HttpContext httpContext) =>
{
    var clientKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    if (RegisterAttemptGuard.IsLocked(clientKey))
    {
        return Results.StatusCode(StatusCodes.Status423Locked);
    }

    if (string.IsNullOrWhiteSpace(request.Email) ||
        string.IsNullOrWhiteSpace(request.Username) ||
        string.IsNullOrWhiteSpace(request.Password))
    {
        RegisterAttemptGuard.RegisterFailure(clientKey);
        return Results.BadRequest(new ErrorResponse("Email, username and password are required."));
    }

    if (request.Password.Length < 8)
    {
        RegisterAttemptGuard.RegisterFailure(clientKey);
        return Results.BadRequest(new ErrorResponse("Password must be at least 8 characters."));
    }

    var normalizedUsername = request.Username.Trim().ToLowerInvariant();
    var normalizedEmail = request.Email.Trim().ToLowerInvariant();

    var userExists = await dbContext.Users.AnyAsync(user =>
        user.NormalizedUsername == normalizedUsername ||
        user.NormalizedEmail == normalizedEmail);

    if (userExists)
    {
        RegisterAttemptGuard.RegisterFailure(clientKey);
        return Results.Conflict(new ErrorResponse("Username or email already exists."));
    }

    var user = new User
    {
        Id = Guid.NewGuid(),
        Email = request.Email.Trim(),
        NormalizedEmail = normalizedEmail,
        Username = request.Username.Trim(),
        NormalizedUsername = normalizedUsername,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
        IsPrivate = false
    };

    dbContext.Users.Add(user);
    await dbContext.SaveChangesAsync();
    RegisterAttemptGuard.RegisterSuccess(clientKey);

    var token = CreateJwtToken(user, configuration);
    var response = new RegisterResponse(
        user.Id,
        user.Email,
        user.Username,
        user.IsPrivate,
        token);

    return Results.Created($"/api/v1/users/{user.Id}", response);
});

app.MapPost("/api/v1/auth/login", async (
    LoginRequest request,
    AppDbContext dbContext,
    IConfiguration configuration) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) ||
        string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new ErrorResponse("Email and password are required."));
    }

    var normalizedEmail = request.Email.Trim().ToLowerInvariant();
    var user = await dbContext.Users.FirstOrDefaultAsync(dbUser =>
        dbUser.NormalizedEmail == normalizedEmail);

    if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
    {
        return Results.Unauthorized();
    }

    var token = CreateJwtToken(user, configuration);
    var response = new LoginResponse(
        user.Id,
        user.Email,
        user.Username,
        user.IsPrivate,
        token);

    return Results.Ok(response);
});

app.MapGet("/api/v1/profile", async (ClaimsPrincipal principal, AppDbContext dbContext) =>
{
    var userId = TryGetUserId(principal);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    var currentUser = await dbContext.Users.FindAsync(userId.Value);
    if (currentUser is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(new UserProfileResponse(
        currentUser.Id,
        currentUser.Email,
        currentUser.Username,
        currentUser.IsPrivate));
}).RequireAuthorization();

app.MapMethods(
    "/api/v1/profile/privacy",
    new[] { "PATCH", "PUT", "POST" },
    async (UpdatePrivacyRequest request, ClaimsPrincipal principal, AppDbContext dbContext) =>
{
    var userId = TryGetUserId(principal);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    var currentUser = await dbContext.Users.FindAsync(userId.Value);
    if (currentUser is null)
    {
        return Results.NotFound();
    }

    currentUser.IsPrivate = request.IsPrivate;
    await dbContext.SaveChangesAsync();

    return Results.Ok(new UserProfileResponse(
        currentUser.Id,
        currentUser.Email,
        currentUser.Username,
        currentUser.IsPrivate));
}).RequireAuthorization();

app.MapGet("/api/v1/users/{userId:guid}/profile", async (
    Guid userId,
    ClaimsPrincipal principal,
    AppDbContext dbContext) =>
{
    var target = await dbContext.Users.FindAsync(userId);
    if (target is null)
    {
        return Results.NotFound();
    }

    if (!target.IsPrivate)
    {
        return Results.Ok(new UserProfileResponse(
            target.Id,
            target.Email,
            target.Username,
            target.IsPrivate));
    }

    var callerId = TryGetUserId(principal);
    if (callerId is { } ownerId && ownerId == target.Id)
    {
        return Results.Ok(new UserProfileResponse(
            target.Id,
            target.Email,
            target.Username,
            target.IsPrivate));
    }

    return Results.NotFound();
});

app.MapGet("/api/v1/health", () => Results.Ok(new { status = "ok" }));

app.Run();

static Guid? TryGetUserId(ClaimsPrincipal principal)
{
    var candidates = new[]
    {
        principal.FindFirstValue(JwtRegisteredClaimNames.Sub),
        principal.FindFirstValue(ClaimTypes.NameIdentifier),
    };

    foreach (var candidate in candidates)
    {
        if (Guid.TryParse(candidate, out var fromKnownClaim))
        {
            return fromKnownClaim;
        }
    }

    foreach (var claim in principal.Claims)
    {
        if (claim.Type is JwtRegisteredClaimNames.Sub or "sub" or ClaimTypes.NameIdentifier
            && Guid.TryParse(claim.Value, out var id))
        {
            return id;
        }
    }

    return null;
}

static string CreateJwtToken(User user, IConfiguration configuration)
{
    var jwtSection = configuration.GetSection("Jwt");
    var signingKey = jwtSection["SigningKey"]!;
    var keyId = jwtSection["KeyId"] ?? "archi-symmetric-v1";
    var issuer = jwtSection["Issuer"]!;
    var audience = jwtSection["Audience"]!;
    var expiryMinutes = int.Parse(jwtSection["ExpiryMinutes"] ?? "60");

    var credentials = new SigningCredentials(
        CreateJwtSigningKey(signingKey, keyId),
        SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim("username", user.Username)
    };

    var token = new JwtSecurityToken(
        issuer,
        audience,
        claims,
        expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
        signingCredentials: credentials);

    return new JwtSecurityTokenHandler().WriteToken(token);
}

static SymmetricSecurityKey CreateJwtSigningKey(string signingKey, string keyId) =>
    new(Encoding.UTF8.GetBytes(signingKey)) { KeyId = keyId };

public partial class Program;
