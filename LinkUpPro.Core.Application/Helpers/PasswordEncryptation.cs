using System.Security.Cryptography;
using System.Text;

namespace LinkUpPro.Core.Application.Helpers
{
    public static class PasswordEncryptation
    {
        public static string ComputeSha256Hash(string password)
        {
            //Create a SHA256   
            using SHA256 sha256Hash = SHA256.Create();
            //ComputeHash
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

            //Convert byte array to a string
            StringBuilder sbuilder = new();

            foreach (var item in bytes)
            {
                sbuilder.Append(item.ToString("x2"));
            }

            return sbuilder.ToString();
        }
    }
}
