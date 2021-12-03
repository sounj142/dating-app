using System.Security.Cryptography;
using System.Text;

namespace API.Extensions
{
    public static class HMACSHA512Extensions
    {
        public static byte[] ComputePasswordHash(this HMACSHA512 hmac, string password)
        {
            return hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }
    }
}
