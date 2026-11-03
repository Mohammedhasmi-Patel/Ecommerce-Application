namespace Ecommerce.DTO.User
{
    public class CreateUserRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public IFormFile? ProfileImage { get; set; }
    }
}
