using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Archi.Api.Contracts.Archive;
using Archi.Api.Data;
using Archi.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Archi.Api.Controllers;

[ApiController]
[Route("api/v1/archive")]
[Authorize] // 1. YENİLİK: Bu controller'a artık sadece geçerli bir Token'ı olanlar girebilir!
public sealed class ArchiveController(AppDbContext context) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SaveOrUpdateArchive([FromBody] ArchiveItemDto request, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized(new { message = "Geçersiz token veya kullanıcı kimliği bulunamadı." });
        }

        // --- İŞTE O SİHİRLİ KISIM ---
        var currentUser = await context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (currentUser == null)
        {
            // Kullanıcı Supabase'den ilk defa geliyorsa, onu hemen kendi yerel sistemimize kaydediyoruz!
            currentUser = new User { Id = userId, Email = "Bilinmiyor", Username = "Kullanıcı" };
            context.Users.Add(currentUser);
            await context.SaveChangesAsync(cancellationToken);
        }

        // ... Kodun geri kalanı mevcut halindeki gibi devam edecek
        var existingItem = await context.ArchiveItems
            .FirstOrDefaultAsync(a => a.ExternalId == request.ExternalId && a.UserId == currentUser.Id, cancellationToken);
        var statusEnum = request.Status switch
        {
            "Listem" => (ArchiveItemStatus)0,
            "Akış" => (ArchiveItemStatus)1,
            "Arşivim" => (ArchiveItemStatus)2,
            _ => (ArchiveItemStatus)0
        };

        if (existingItem != null)
        {
            existingItem.Status = statusEnum;
            
            if (!string.IsNullOrEmpty(request.ImageUrl))
            {
                int? parsedYear = int.TryParse(request.Year, out var y) ? y : null;

                existingItem.Metadata = new ArchiveMetadata 
                {
                    ImageUrl = request.ImageUrl,
                    Year = existingItem.Metadata?.Year ?? parsedYear
                };
            }
        }
        else
        {
            var newItem = new ArchiveItem
            {
                Id = Guid.NewGuid(),
                UserId = currentUser.Id, // YENİ KAYIT EKLENİRKEN ARTIK GERÇEK KULLANICI ID'Sİ YAZILIYOR
                ExternalId = request.ExternalId,
                Title = request.Title,
                Category = request.Category,
                Status = statusEnum,
                CreatedAt = DateTimeOffset.UtcNow,
                Metadata = new ArchiveMetadata 
                {
                    ImageUrl = request.ImageUrl,
                    Year = int.TryParse(request.Year, out var year) ? year : null 
                }
            };
            context.ArchiveItems.Add(newItem);
        }

        await context.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Arşive başarıyla eklendi." });
    }

    [HttpGet]
    public async Task<IActionResult> GetUserArchives(CancellationToken cancellationToken)
    {
        // İstek atan kişinin Token'ından kimliğini alıyoruz
        var userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized(new { message = "Geçersiz oturum." });
        }
        var currentUser = await context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (currentUser == null)
        {
            // Kullanıcı Supabase'den giriş yapmış ama yerel Users tablomuzda henüz yok. 
            // Hata vermek yerine onu hemen veritabanımıza ekliyoruz!
            currentUser = new User { Id = userId, Email = "Bilinmiyor", Username = "Kullanıcı" };
            context.Users.Add(currentUser);
            await context.SaveChangesAsync(cancellationToken);
        }
        // SADECE O KİŞİYE AİT olan kayıtları getiriyoruz! Başkasının kayıtları karışmıyor.
        var items = await context.ArchiveItems
            .Where(a => a.UserId == userId)
            .ToListAsync(cancellationToken);

        var response = items.Select(item => new ArchiveItemDto
        {
            ExternalId = item.ExternalId,
            Title = item.Title,
            Category = item.Category,
            Status = ((int)item.Status).ToString(),
            ImageUrl = item.Metadata.ImageUrl?.ToString() ?? "", 
            Year = item.Metadata.Year?.ToString() ?? ""          
        }).ToList(); 

        return Ok(response);
    }
}

public class ArchiveItemDto
{
    public string ExternalId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}