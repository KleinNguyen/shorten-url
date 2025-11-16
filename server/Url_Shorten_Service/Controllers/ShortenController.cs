
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Url_Shorten_Service.DTOs;
using Url_Shorten_Service.Services;


namespace Url_Shorten_Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class ShortenController : ControllerBase
    {
        private readonly SendUrlService _service;

        public ShortenController(SendUrlService service)
        {
            _service = service;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SendShortUrl(ShortenDto dto)
        {
            try
            {
                string? email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;


                if (string.IsNullOrWhiteSpace(dto.OriginalUrl) ||
                !Uri.TryCreate(dto.OriginalUrl, UriKind.Absolute, out Uri? validatedUri) ||
                (validatedUri.Scheme != Uri.UriSchemeHttp && validatedUri.Scheme != Uri.UriSchemeHttps))
                {
                    return BadRequest(new { Message = "Invalid URL. Please provide a valid HTTP or HTTPS link." });
                }


                if (!string.IsNullOrEmpty(dto.ShortenCode) && dto.ShortenCode.Length != 7)
                {
                    return BadRequest(new { Message = "Alias not available. Shorten code must be exactly 7 characters." });
                }

                var baseUrl = Environment.GetEnvironmentVariable("BASE_URL");
                if (string.IsNullOrEmpty(baseUrl))
                {
                    baseUrl = $"{Request.Scheme}://{Request.Host}";
                }


                var result = await _service.SendShortUrl(dto, baseUrl, email);

                return Ok(new
                {
                    Link = $"{baseUrl}/api/shorten/{result.ShortenCode}",
                    IsAuthenticated = !string.IsNullOrEmpty(email)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }


        [HttpGet("{code}")]
        [AllowAnonymous]
        public async Task<IActionResult> RedirectToOriginal(string code)
        {
            
            var shortUrl = await _service.GetShortUrlByCode(code);
            if (shortUrl == null) return NotFound();

            return Redirect(shortUrl.OriginalUrl);
        }

    }
}

