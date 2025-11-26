

namespace Shareds_Events
{
    public class LoginEvent
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string SourceService { get; set; } = "Login";
    }
}
