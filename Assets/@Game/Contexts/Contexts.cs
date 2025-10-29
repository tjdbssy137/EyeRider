using UnityEngine;

public partial class Contexts
{
    #region Contents
    private CarContext _car = new CarContext();
    public static CarContext Car { get { return Instance?._car; } }
    #endregion
    private static void Init_Game()
    {
    }
}
