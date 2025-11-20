
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Url_Shorten_Service.Data;
using Url_Shorten_Service.DTOs;
using Url_Shorten_Service.Models;
using Microsoft.AspNetCore.Mvc;
using Shareds_Events;


namespace Url_Shorten_Service.Services
{
    public class SendUrlService
    {
        private readonly ShortenDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        public SendUrlService(ShortenDbContext context, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
        }
        public async Task<UrlShorten?> GetShortUrlByCode(string code)
        {
            return await _context.UrlShortenes.FirstOrDefaultAsync(u => u.ShortenCode == code);
        }


        public async Task<UrlShorten> SendShortUrl(ShortenDto dto, string baseUrl, string? email)
        {

            if (!string.IsNullOrWhiteSpace(dto.ShortenCode))
            {
                bool exists = await _context.UrlShortenes.AnyAsync(u => u.ShortenCode == dto.ShortenCode);
                if (exists)
                    throw new Exception("The ShortenCode already exists. Please choose a different code.");
            }

            string code;

            if (string.IsNullOrWhiteSpace(dto.ShortenCode))
            {
                code = Guid.NewGuid().ToString().Substring(0, 7);
            }else
            {
                code = dto.ShortenCode;
            }


            var shortUrl = new UrlShorten
            {
                Email = email,
                OriginalUrl = dto.OriginalUrl,
                ShortenCode = code,
                DateTime = DateTime.UtcNow
            };

            _context.UrlShortenes.Add(shortUrl);
            await _context.SaveChangesAsync();

            var link = $"{baseUrl}/api/shorten/{shortUrl.ShortenCode}";

            await _publishEndpoint.Publish(new UrlSendEvent
            {
                Email = email,
                OriginalUrl = shortUrl.OriginalUrl,
                ShortenUrl = link,
                ShortenCode = shortUrl.ShortenCode,
                SourceService = "UrlShorten",
                DateTime = shortUrl.DateTime
            });

            return shortUrl;
        }
    }
}

