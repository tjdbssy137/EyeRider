using System;
using System.Data;
using UnityEngine;

public class TimeManager
{
    private GameObject _timeRoot = null;
    private UInt64 _recivedServerTime = 0; // timestamp
    private float _receivedClientTime = 0f;

    public void Init()
    {
        if (_timeRoot == null)
        {
            _timeRoot = GameObject.Find("@TimeRoot");

            if (_timeRoot == null)
            {
                _timeRoot = new GameObject { name = "@TimeRoot" };
                UnityEngine.Object.DontDestroyOnLoad(_timeRoot);
            }
        }
    }

    public void SyncServerTime(UInt64 receivedTime)
    {
        _recivedServerTime = receivedTime;
        _receivedClientTime = GetClientCurrentTick();
    }

    public float GetClientCurrentTick()
    {
        return Time.realtimeSinceStartup;
    }

    public DateTime GetClientCurrentTime()
    {
        return DateTime.Now;
    }

    // *** 서버틱은 MS단위다
    public UInt64 GetServerCurrentTick()
    {
        if (0 == _recivedServerTime)
        {
            return (UInt64)(GetClientCurrentTick() * 1000); // 서버 시간이 동기화되지 않음
        }

        // 클라이언트에서 흐른 시간 계산
        float elapsedTime = GetClientCurrentTick() - _receivedClientTime;

        // 서버의 현재 시간(ms) 반환
        return _recivedServerTime + (UInt64)(elapsedTime * 1000);
    }

    public DateTime GetServerCurrentTime()
    {
        // 서버의 현재 밀리초(ms) 계산
        double currentServerTick = GetServerCurrentTick();

        if ( 0 == currentServerTick)
        {
            return DateTime.Now.ToLocalTime(); // 서버 시간이 동기화되지 않음
        }

        // UTC 기준의 DateTime 생성 후 로컬 시간으로 변환
        DateTime utcTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(currentServerTick);
        return utcTime.ToLocalTime(); // 로컬 시간 변환
    }

}