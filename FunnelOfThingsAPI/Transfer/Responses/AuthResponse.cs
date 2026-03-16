namespace FunnelOfThingsAPI.Transfer.Responses
{
    public class AuthResponse
    {
        public int UserId { get; set; }  
        public string Email { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string Token { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }
}