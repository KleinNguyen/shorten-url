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


                var baseUrl = Environment.GetEnvironmentVariable("BASE_URL");
                if (string.IsNullOrEmpty(baseUrl))
                {
                    baseUrl = $"{Request.Scheme}://{Request.Host}";
                }

                // Gọi service
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