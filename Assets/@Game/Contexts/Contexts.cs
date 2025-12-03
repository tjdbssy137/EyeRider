using UnityEngine;

public partial class Contexts
{
    #region Contents
    private CarContext _car = new CarContext();
    public static CarContext Car { get { return Instance?._car; } }

    private MapContext _map = new MapContext();
    public static MapContext Map { get { return Instance?._map; } }

    private InGameContext _inGame = new InGameContext();
    public static InGameContext InGame { get { return Instance?._inGame; } }

    private GameProfileContext _gameProfile = new GameProfileContext();
    public static GameProfileContext GameProfile { get { return Instance?._gameProfile; } }
    
    #endregion
    private static void Init_Game()
    {
    }
}
