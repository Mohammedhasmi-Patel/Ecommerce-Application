namespace Ecommerce.DTO.User
{
    public class LoginResponse
    {

        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public string Role { get; set; }

        public string Token { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}
