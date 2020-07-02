using System.Security.Cryptography;
using System.Text;

namespace Wexflow.CommandLineParserClient.Common.Extensions
{
    public static class StringExtensions
    {
        public static string EncodeWithMd5(this string password)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.ASCII.GetBytes(password);
                var hashedBytes = md5.ComputeHash(inputBytes);

                var builder = new StringBuilder();
                foreach (var hashedByte in hashedBytes)
                {
                    builder.Append(hashedByte.ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}
