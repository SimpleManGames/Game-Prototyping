using System.Security.Cryptography;
using System.Text;

namespace Core.Helper
{
    public class Hash
    {
        public enum HashType
        {
            MD5,
            SHA1,
            SHA256,
            SHA512
        }

        public class HashHelper
        {
            public static string ReturnHash(string input, HashType type)
            {
                byte[] hashBytes = new byte[0];
                HashAlgorithm hashCode;

                switch (type)
                {
                    case HashType.MD5:
                        hashCode = MD5.Create();
                        hashBytes = hashCode.ComputeHash(Encoding.UTF8.GetBytes(input));
                        break;
                    case HashType.SHA1:
                        hashCode = SHA1.Create();
                        hashBytes = hashCode.ComputeHash(Encoding.UTF8.GetBytes(input));
                        break;
                    case HashType.SHA256:
                        hashCode = SHA256.Create();
                        hashBytes = hashCode.ComputeHash(Encoding.UTF8.GetBytes(input));
                        break;
                    case HashType.SHA512:
                        hashCode = SHA512.Create();
                        hashBytes = hashCode.ComputeHash(Encoding.UTF8.GetBytes(input));
                        break;
                }

                StringBuilder returnString = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    returnString.Append(b.ToString("X2"));
                }

                return returnString.ToString();
            }
        }
    }
}