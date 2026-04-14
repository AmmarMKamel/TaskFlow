using System.Security.Cryptography;
using System.Text;

namespace TaskFlow.src.Infrastructure.Services
{
    public class PasswordHasher
    {
        public (string hash, string salt) HashPassword(string password)
        {
            using var hmac = new HMACSHA512();

            var salt = Convert.ToBase64String(hmac.Key);
            var hash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));

            return (hash, salt);
        }

        public bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            var key = Convert.FromBase64String(storedSalt);

            using var hmac = new HMACSHA512(key);

            var computedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));

            return computedHash == storedHash;
        }
    }
}
