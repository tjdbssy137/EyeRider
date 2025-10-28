using AnimalWars.Dto.InGame.Login;
using Global.Shared.Dto;
using Newtonsoft.Json;
using NUnit;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

public partial class UserContext
{
    public string AccessToken { get; private set; }
    public string RefreshToken { get; private set; }

    public void Init()
    {
    }

    public void Login(string accessToken, string refreshToken)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
    }

    // JWT에서 Payload 부분만 추출해서 Base64 디코딩 후, exp 값을 확인
    public bool IsTokenExpired(string jwt)
    {
        if(string.IsNullOrWhiteSpace(jwt))
            return true; // 빈 토큰은 만료로 간주

        try
        {
            // JWT는 3부분: header.payload.signature
            string[] parts = jwt.Split('.');
            if (parts.Length != 3)
                return true; // 잘못된 토큰

            string payload = parts[1];

            // Base64Url → Base64 변환
            payload = payload.Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            // 디코딩
            var bytes = Convert.FromBase64String(payload);
            var json = Encoding.UTF8.GetString(bytes);

            // exp 값 찾기 (정규식 이용)
            // exp는 초 단위 유닉스 타임스탬프임
            var match = Regex.Match(json, "\"exp\"\\s*:\\s*(\\d+)");
            if (match.Success)
            {
                long exp = long.Parse(match.Groups[1].Value);
                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                return now > exp; // true면 만료됨
            }

            return true; // exp가 없으면 무효로 취급
        }
        catch
        {
            return true; // 파싱 실패 → 만료로 간주
        }
    }
}
