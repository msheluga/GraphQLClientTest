using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GraphQLClientTest
{
    public static class CryptographyExtensions
    {
        /// <summary>Computes the MD5 hash of the given text.</summary>
        /// <param name="text">The text.</param>
        /// <returns>A string representation of the MD5 hash of the given text.</returns>
        public static string ComputeMd5Hash(this string text)
        {
            using var md5 = MD5.Create();
            byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(text));

            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                builder.Append(data[i].ToString("x2"));
            }

            return builder.ToString();
        }
    }
}
