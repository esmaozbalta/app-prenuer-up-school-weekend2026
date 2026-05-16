using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Archi.Api.Contracts.Auth;
using Archi.Api.Contracts.Common;
using Archi.Api.Contracts.Profile;
using Archi.Api.Contracts.Users;
using Archi.Api.Data;
using Archi.Api.DependencyInjection;
using Archi.Api.Endpoints;
using Archi.Api.Models;
using Archi.Api.Security;
using Archi.Api.Services.Cache;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var databaseConnectionString = DatabaseConnection.Resolve(builder.Configuration);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        databaseConnectionString,
        npgsql => npgsql
            .EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null)
            .CommandTimeout(120)));

builder.Services.AddArchiCache(builder.Configuration);
builder.Services.AddArchiSearchServices(builder.Configuration);
builder.Services.AddArchiArchiveServices();
builder.Services.AddArchiFeedServices();
builder.Services.AddArchiSyncServices(builder.Configuration);

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

var parsedDatabase = new Npgsql.NpgsqlConnectionStringBuilder(databaseConnectionString);
app.Logger.LogInformation(
    "Database configured: Host={Host}, Port={Port}, Database={Database}, Username={Username}",
    parsedDatabase.Host,
    parsedDatabase.Port,
    parsedDatabase.Database,
    parsedDatabase.Username);

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

app.MapPost("/api/v1/auth/register", HandleRegisterAsync);
app.MapPost("/api/v1/users", HandleRegisterAsync);

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

app.MapGet("/api/v1/users/{userId:guid}", async (
    Guid userId,
    ClaimsPrincipal principal,
    AppDbContext dbContext) =>
{
    var target = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Id == userId);
    if (target is null)
    {
        return Results.NotFound();
    }

    var callerId = TryGetUserId(principal);
    if (!target.IsPrivate)
    {
        var includeOauth = callerId == target.Id;
        return Results.Ok(MapToUserResponse(target, includeOauth));
    }

    if (callerId == target.Id)
    {
        return Results.Ok(MapToUserResponse(target, includeOauthId: true));
    }

    return Results.NotFound();
});

app.MapGet("/api/v1/users", async (int? page, int? pageSize, AppDbContext dbContext) =>
{
    var resolvedPage = Math.Max(1, page ?? 1);
    var resolvedSize = Math.Clamp(pageSize ?? 20, 1, 100);

    var publicUsers = dbContext.Users.AsNoTracking().Where(user => !user.IsPrivate);
    var totalCount = await publicUsers.CountAsync();
    var items = await publicUsers
        .OrderByDescending(user => user.CreatedAt)
        .Skip((resolvedPage - 1) * resolvedSize)
        .Take(resolvedSize)
        .Select(user => new UserResponse(
            user.Id,
            user.Email,
            user.Username,
            user.IsPrivate,
            user.IsVaultMember,
            user.CreatedAt,
            null))
        .ToListAsync();

    return Results.Ok(new UserListResponse(items, totalCount));
});

app.MapPut("/api/v1/users/{userId:guid}", async (
    Guid userId,
    UpdateUserRequest request,
    ClaimsPrincipal principal,
    AppDbContext dbContext) =>
{
    var callerId = TryGetUserId(principal);
    if (callerId is null || callerId != userId)
    {
        return Results.Forbid();
    }

    var user = await dbContext.Users.FirstOrDefaultAsync(dbUser => dbUser.Id == userId);
    if (user is null)
    {
        return Results.NotFound();
    }

    if (request.Username is not null)
    {
        var trimmedUsername = request.Username.Trim();
        if (string.IsNullOrWhiteSpace(trimmedUsername))
        {
            return Results.BadRequest(new ErrorResponse("Username cannot be empty."));
        }

        var normalizedUsername = trimmedUsername.ToLowerInvariant();
        if (normalizedUsername != user.NormalizedUsername)
        {
            var usernameTaken = await dbContext.Users.AnyAsync(dbUser =>
                dbUser.NormalizedUsername == normalizedUsername && dbUser.Id != userId);
            if (usernameTaken)
            {
                return Results.Conflict(new ErrorResponse("Username already in use."));
            }

            user.Username = trimmedUsername;
            user.NormalizedUsername = normalizedUsername;
        }
    }

    if (request.Email is not null)
    {
        var trimmedEmail = request.Email.Trim();
        var normalizedEmail = trimmedEmail.ToLowerInvariant();
        if (normalizedEmail != user.NormalizedEmail)
        {
            if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            {
                return Results.BadRequest(new ErrorResponse("Current password is required to change email."));
            }

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                return Results.BadRequest(new ErrorResponse("Current password is incorrect."));
            }

            var emailTaken = await dbContext.Users.AnyAsync(dbUser =>
                dbUser.NormalizedEmail == normalizedEmail && dbUser.Id != userId);
            if (emailTaken)
            {
                return Results.Conflict(new ErrorResponse("Email already in use."));
            }

            user.Email = trimmedEmail;
            user.NormalizedEmail = normalizedEmail;
        }
    }

    if (request.NewPassword is not null)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
        {
            return Results.BadRequest(new ErrorResponse("Current password is required to set a new password."));
        }

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return Results.BadRequest(new ErrorResponse("Current password is incorrect."));
        }

        if (request.NewPassword.Length < 8)
        {
            return Results.BadRequest(new ErrorResponse("New password must be at least 8 characters."));
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
    }

    await dbContext.SaveChangesAsync();
    return Results.Ok(MapToUserResponse(user, includeOauthId: true));
}).RequireAuthorization();

app.MapDelete("/api/v1/users/{userId:guid}", async (
    Guid userId,
    [FromBody] DeleteUserRequest request,
    ClaimsPrincipal principal,
    AppDbContext dbContext) =>
{
    var callerId = TryGetUserId(principal);
    if (callerId is null || callerId != userId)
    {
        return Results.Forbid();
    }

    if (string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new ErrorResponse("Password is required."));
    }

    var user = await dbContext.Users.FirstOrDefaultAsync(dbUser => dbUser.Id == userId);
    if (user is null)
    {
        return Results.NotFound();
    }

    if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
    {
        return Results.BadRequest(new ErrorResponse("Password is incorrect."));
    }

    dbContext.Users.Remove(user);
    await dbContext.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

app.MapSearchEndpoints();
app.MapArchiveEndpoints();
app.MapFeedEndpoints();
app.MapVibeEndpoints();
app.MapSyncEndpoints();
app.MapShareCardEndpoints();

app.MapGet("/api/v1/health", async (ICacheService cacheService) =>
{
    var cacheHealthy = await cacheService.PingAsync();
    return Results.Ok(new
    {
        status = "ok",
        cache = new { provider = cacheService.ProviderName, healthy = cacheHealthy }
    });
});

app.Run();

static UserResponse MapToUserResponse(User user, bool includeOauthId) =>
    new(
        user.Id,
        user.Email,
        user.Username,
        user.IsPrivate,
        user.IsVaultMember,
        user.CreatedAt,
        includeOauthId ? user.OauthId : null);

static async Task<IResult> HandleRegisterAsync(
    RegisterRequest request,
    AppDbContext dbContext,
    IConfiguration configuration,
    HttpContext httpContext)
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

    var createdAt = DateTimeOffset.UtcNow;
    var user = new User
    {
        Id = Guid.NewGuid(),
        Email = request.Email.Trim(),
        NormalizedEmail = normalizedEmail,
        Username = request.Username.Trim(),
        NormalizedUsername = normalizedUsername,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
        IsPrivate = false,
        IsVaultMember = false,
        CreatedAt = createdAt
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
}

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
