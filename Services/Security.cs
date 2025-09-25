using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;


namespace KBN.Services
{
    public class Security
    {
        // Encrypt
        public static string Encrypt(string plainText, string key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes(key)); // derive a 256-bit key
                aes.GenerateIV(); // random IV for security

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    // prepend IV so we can use it for decryption
                    ms.Write(aes.IV, 0, aes.IV.Length);
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var writer = new StreamWriter(cs))
                    {
                        writer.Write(plainText);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        // Decrypt
        public static string Decrypt(string cipherText, string key)
        {
            byte[] fullCipher = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes(key));
                byte[] iv = new byte[aes.BlockSize / 8];
                Array.Copy(fullCipher, iv, iv.Length);
                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var reader = new StreamReader(cs))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }

}
