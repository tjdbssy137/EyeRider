using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
public static class Aes256Util
{
    /// <summary>
    /// 문자열을 AES-256으로 암호화하고 Base64 문자열로 반환합니다.
    /// </summary>
    /// <param name="plainText">평문 문자열</param>
    /// <param name="key">AES 키 (256비트)</param>
    /// <param name="iv">IV (128비트)</param>
    /// <returns>Base64 인코딩된 암호문 문자열</returns>
    public static string EncryptString(string plainText, string key, string iv)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentNullException(nameof(plainText));
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(iv))
            throw new ArgumentNullException("Key와 IV는 null이 될 수 없습니다.");

        byte[] keyBytes = GetAesKeyFromString(key, 32); // 32바이트 (256비트)
        byte[] ivBytes = GetAesKeyFromString(iv, 16);   // 16바이트 (128비트)

        using (Aes aes = Aes.Create())
        {
            aes.KeySize = 256;
            aes.Key = keyBytes;
            aes.IV = ivBytes;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (MemoryStream ms = new MemoryStream())
            using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                cs.Write(plainBytes, 0, plainBytes.Length);
                cs.FlushFinalBlock();
                return Convert.ToBase64String(ms.ToArray()); // 암호문을 Base64 문자열로 변환
            }
        }
    }

    /// <summary>
    /// Base64로 암호화된 문자열을 AES-256 복호화
    /// </summary>
    /// <param name="cipherText">Base64 인코딩된 암호문</param>
    /// <param name="key">AES 키 (256비트)</param>
    /// <param name="iv">IV (128비트)</param>
    /// <returns>복호화된 평문 문자열</returns>
    public static string DecryptString(string cipherText, string key, string iv)
    {
        if (string.IsNullOrEmpty(cipherText))
            throw new ArgumentNullException(nameof(cipherText));
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(iv))
            throw new ArgumentNullException("Key와 IV는 null이 될 수 없습니다.");

        byte[] keyBytes = GetAesKeyFromString(key, 32); // 32바이트 (256비트)
        byte[] ivBytes = GetAesKeyFromString(iv, 16);   // 16바이트 (128비트)
        byte[] cipherBytes = Convert.FromBase64String(cipherText); // Base64 → byte[]

        using (Aes aes = Aes.Create())
        {
            aes.KeySize = 256;
            aes.Key = keyBytes;
            aes.IV = ivBytes;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (MemoryStream ms = new MemoryStream(cipherBytes))
            using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (StreamReader sr = new StreamReader(cs, Encoding.UTF8))
            {
                return sr.ReadToEnd(); // 복호화된 문자열 반환
            }
        }
    }

    /// <summary>
    /// 문자열을 AES 키 또는 IV에 사용할 수 있도록 지정된 길이의 바이트 배열로 변환
    /// </summary>
    private static byte[] GetAesKeyFromString(string input, int length)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(input);
        Array.Resize(ref keyBytes, length); // 지정된 길이로 맞춤
        return keyBytes;
    }
}