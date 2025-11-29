using System.Text;
using System;
using UnityEngine;
using Unity.VisualScripting;

//얼리억세서
// 보통 사람들이 알지도못할떄 1월 부터
//  -> 코딩공부룰 게을리
//  => 지금부터는 AI를 잘쓰는사람이 일잘하는 사람이다.

public static class SecurePlayerPrefs
{
    private static string _iv = "qzrmkfwusaplteyngbjhcdovxi";
    private static string _key => SecureKeyProvider.GetKey();
    
    public static void SetString(string key, string value)
    {
        string newValue = Aes256Util.EncryptString(value, _key, _iv);
        string newKey = Aes256Util.EncryptString(key, _key, _iv);

        PlayerPrefs.SetString(newKey, newValue);
    }

    public static string GetString(string key, string defaultValue)
    {
        string newKey = Aes256Util.EncryptString(key, _key, _iv);
        var value = PlayerPrefs.GetString(newKey, defaultValue);

        if(value == defaultValue)
        {
            return defaultValue;
        }

        return Aes256Util.DecryptString(value, _key, _iv);
    }

    public static void SetInt(string key, int value)
    {
        string newValue = Aes256Util.EncryptString(value.ToString(), _key, _iv);
        string newKey = Aes256Util.EncryptString(key, _key, _iv);

        PlayerPrefs.SetString(newKey, newValue);
    }

    public static int GetInt(string key, int defaultValue)
    {
        string newKey = Aes256Util.EncryptString(key, _key, _iv);
        var value = PlayerPrefs.GetString(newKey, defaultValue.ToString());

        if (value == defaultValue.ToString())
        {
            return defaultValue;
        }
        var returnValue = Aes256Util.DecryptString(value, _key, _iv);

        return int.Parse(returnValue);
    }

    public static void SetFloat(string key, float value)
    {
        string newValue = Aes256Util.EncryptString(value.ToString(), _key, _iv);
        string newKey = Aes256Util.EncryptString(key, _key, _iv);

        PlayerPrefs.SetString(newKey, newValue);
    }

    public static float GetFloat(string key, float defaultValue)
    {
        string newKey = Aes256Util.EncryptString(key, _key, _iv);
        var value = PlayerPrefs.GetString(newKey, defaultValue.ToString());

        if (value == defaultValue.ToString())
        {
            return defaultValue;
        }
        var returnValue = Aes256Util.DecryptString(value, _key, _iv);

        return float.Parse(returnValue);
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }

    public static bool HasKey(string key)
    {
        string newKey = Aes256Util.EncryptString(key, _key, _iv);
        return PlayerPrefs.HasKey(newKey);
    }

    public static void DeleteAll()
    {
        PlayerPrefs.DeleteAll();
    }

    public static void DeleteKey(string key)
    {
        if(HasKey(key))
        {
            string newKey = Aes256Util.EncryptString(key, _key, _iv);

            PlayerPrefs.DeleteKey(newKey);
        }
    }

}


//저장할떄 암호화
// 암호화 기술
//   단방향 암호화 SHA-256 (서버에서 패스워드 확인)
//    [String] => [암호화된 String]

//   양방향 암호화 AES-256 (유저 데이터)
//    [String] <=> [암호화된 String]
//    
//   저장할때, String이랑 Key 를 같이 입력받습니다.
//   해독할때 필요한 열쇠
//   클라에 하드코딩형태로 저장합니다.


// 
//312433c28349f63c4f387953ff337046e794bea0f9b9ebfcb08e90046ded9c76
//1000