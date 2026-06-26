namespace LinkUpPro.Core.Application.DTOs.Email
{
    public class EmailRequestDTO
    {
        public string? To { get; set; }
        public required string Subject { get; set; }
        public required string HtmlBody { get; set; }
        public List<string>? ToRange { get; set; } = [];
    }
}
