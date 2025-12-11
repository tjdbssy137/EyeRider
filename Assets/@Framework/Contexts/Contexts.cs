using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Unity.VisualScripting;
using UnityEngine;

public partial class Contexts
{
    private static Contexts s_instance;
    private static Contexts Instance { get { Init_Framework(); return s_instance; } }

    #region Contents
    private UserContext _user = new UserContext();

    public static UserContext User { get { return Instance?._user; } }
    #endregion


    public static void Init_Framework()
    {
        if (s_instance == null)
        {
            s_instance = new Contexts();


            // 초기화
            s_instance._user.Init();
            Contexts.Init_Game();
        }
    }

    public static void Reset()
    {
        if (Instance == null)
        {
            return;
        }

        Instance._car = new CarContext
        {
            VerticalAccelerationSpeed = 0f,
            MaxFuel = 0f,
            MaxCondition = 0f
        };

        Instance._gameProfile = new GameProfileContext
        {
            CurrentLevel = 1,
            NextLevel = 1,
            CarId = 0
        };

        Instance._map = new MapContext();

        Instance._inGame = new InGameContext
        {
            Car = null,
            MaxLevel = 0,
            SpawnPosition = Vector3.zero,

            WKey = false,
            AKey = false,
            SKey = false,
            DKey = false,

            IsGameOver = false,
            IsPaused = false,
            IsGameWin = false,
            MaxRunTime = 0,

            IsCollisionObstacle = 0,
            PanicPoint = 0f,
            GameScore = 0,
            Metre = 0f
        };
    }

    /// <summary>
    /// 이 함수 있는 내용은 @Game안에 정의되어야하는 내용입니다.
    /// @Game에서 정의하고 이 함수의 내용을 주석해주세요.
    /// </summary>
    //private static void Init_Game()
    //{
    //    // @Game에서 정의된 내용이 있다면 여기에 작성해주세요.
    //    // 예시: s_instance._mycontent.Init();
    //}
}
