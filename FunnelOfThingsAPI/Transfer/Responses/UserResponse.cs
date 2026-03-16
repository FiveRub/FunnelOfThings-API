namespace FunnelOfThingsAPI.Transfer.Responses
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; }
        public bool IsVerified { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }



    }
}
