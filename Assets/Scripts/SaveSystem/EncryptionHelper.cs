using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class EncryptionHelper
{
    // These must be 16, 24, or 32 bytes long. Keep them secret!
    private static readonly string Salt = "MySuperSecretKey1234567890123456";
    private static readonly string IV = "RandomVector1234";

    public static string Encrypt(string plainText)
    {
        byte[] key = Encoding.UTF8.GetBytes(Salt);
        byte[] iv = Encoding.UTF8.GetBytes(IV);

        using (Aes aes = Aes.Create())
        {
            using (ICryptoTransform encryptor = aes.CreateEncryptor(key, iv))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
        }
    }

    public static string Decrypt(string cipherText)
    {
        byte[] key = Encoding.UTF8.GetBytes(Salt);
        byte[] iv = Encoding.UTF8.GetBytes(IV);
        byte[] buffer = Convert.FromBase64String(cipherText);

        using (Aes aes = Aes.Create())
        {
            using (ICryptoTransform decryptor = aes.CreateDecryptor(key, iv))
            {
                using (MemoryStream ms = new MemoryStream(buffer))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}