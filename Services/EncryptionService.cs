using System.Security.Cryptography;
using System.Text;

namespace BlockShare.Services
{
    public class EncryptionService
    {
        private readonly byte[] _key;

        public EncryptionService(IConfiguration configuration)
        {
            _key = Encoding.UTF8.GetBytes(configuration["Encryption:MasterKey"]);
        }

        public string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();
            var iv = aes.IV;

            using var encryptor = aes.CreateEncryptor(aes.Key, iv);
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            // Повертаємо base64(iv + encrypted)
            var combined = iv.Concat(encryptedBytes).ToArray();
            return Convert.ToBase64String(combined);
        }

        public string Decrypt(string encryptedBase64)
        {
            var combined = Convert.FromBase64String(encryptedBase64);
            var iv = combined.Take(16).ToArray();
            var cipher = combined.Skip(16).ToArray();

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            var decryptedBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}
