using System.Security.Cryptography;
using System.Text;

namespace GuardianEye.Helpers
{
    public static class SecureStorage
    {
        private static readonly byte[] Entropy = { 71, 117, 97, 114, 100, 105, 97, 110, 69, 121, 101 };

        public static string Encrypt(string plainText)
        {
            try
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] encrypted = ProtectedData.Protect(plainBytes, Entropy, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encrypted);
            }
            catch
            {
                return plainText;
            }
        }

        public static string Decrypt(string encryptedText)
        {
            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                byte[] decrypted = ProtectedData.Unprotect(encryptedBytes, Entropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(decrypted);
            }
            catch
            {
                return encryptedText;
            }
        }
    }
}