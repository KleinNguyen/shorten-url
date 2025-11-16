namespace Authentication_Service.DTOs
{
    public class TokenDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }

    }
}
