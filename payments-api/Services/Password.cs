using System.Data;
using System.Security.Cryptography;
using System.Text;
using PaymentsAPI.Utils;

namespace PaymentsAPI.Services
{
    public class Password
    {
        public static void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = System.BitConverter.ToString(hmac.Key);
                passwordHash = System.Convert.ToBase64String(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)));
            }
        }

        public static bool VerifyPasswordHash(string password, string passwordHash, string passwordSalt)
        {
            using (var hmac = new HMACSHA512(Converter.bitConverterStringToByteArray(passwordSalt)))
            {
                var computedHash = System.Convert.ToBase64String(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}