namespace LinkUpPro.Core.Application.ViewModels.User
{
    public class DeleteUserViewModel
    {      
        public required string Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? UserName { get; set; }
    }
}
