using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;

namespace FunnelOfThingsAPI.Services
{
    public class PasswordService
    {
        public string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(64);

            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(password + Convert.ToBase64String(salt)));

            return Convert.ToBase64String(hash) + ":" + Convert.ToBase64String(salt);
        }
        public bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split(':');
            if (parts.Length != 2)
                return false;
            var hash = Convert.FromBase64String(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var computedHash = SHA256.HashData(Encoding.UTF8.GetBytes(password + Convert.ToBase64String(salt)));
            return hash.SequenceEqual(computedHash);


        }
        
    }
}
