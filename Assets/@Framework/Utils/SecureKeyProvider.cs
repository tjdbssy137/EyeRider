using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

public static class SecureKeyProvider
{
    private const string FileName = "appkey.dat";
    private static string _cachedKey = null;

    public static string GetKey()
    {
        if (_cachedKey != null)
            return _cachedKey;

        string path = Path.Combine(Application.persistentDataPath, FileName);

        if (File.Exists(path))
        {
            _cachedKey = File.ReadAllText(path);
        }
        else
        {
            _cachedKey = GenerateRandomKey(32);
            File.WriteAllText(path, _cachedKey);
        }

        return _cachedKey;
    }

    private static string GenerateRandomKey(int length)
    {
        byte[] bytes = new byte[length];
        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(bytes);
        }
        return Convert.ToBase64String(bytes);
    }
}
