using UnityEngine;

public partial class Contexts
{
    #region Contents
    private CarContext _car = new CarContext();
    public static CarContext Car { get { return Instance?._car; } }

    private MapContext _map = new MapContext();
    public static MapContext Map { get { return Instance?._map; } }
    
    #endregion
    private static void Init_Game()
    {
    }
}
