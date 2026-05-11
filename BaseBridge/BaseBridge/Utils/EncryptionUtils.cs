using System.Security.Cryptography;
using System.Text;

namespace BaseBridge.Utils;

public class EncryptionUtils
{
    private const string SecretKey = "ldKSOoP7ycMGcLOSswx11DFf6ApPqNTj";

    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) 
            return plainText;

        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(SecretKey.PadRight(32).Substring(0, 32));
        aes.GenerateIV();

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        var result = new byte[aes.IV.Length + encryptedBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

        return Convert.ToBase64String(result);
    }

    public static string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            return encryptedText;

        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(SecretKey.PadRight(32).Substring(0, 32));
        var allBytes = Convert.FromBase64String(encryptedText);

        var iv = new byte[16];
        var encryptedBytes = new byte[allBytes.Length - 16];
        Buffer.BlockCopy(allBytes, 0, iv, 0, 16);
        Buffer.BlockCopy(allBytes, 16, encryptedBytes, 0, encryptedBytes.Length);

        aes.IV = iv;
        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        var plainBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
        
        return Encoding.UTF8.GetString(plainBytes);
    }
}