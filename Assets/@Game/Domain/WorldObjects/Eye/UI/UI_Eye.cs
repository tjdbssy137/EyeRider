using UnityEngine;

public class UI_Eye : UI_Base
{
    private enum GameObjects
    {
        Controller        
    }
    private PointMover _pointMover;
    private HoleMaskController _holeMaskController;
    public override bool Init()
    {
        if (base.Init() == false)
			return false;

        BindObjects(typeof(GameObjects));
        _pointMover = GetObject((int)GameObjects.Controller).GetComponent<PointMover>();
        _pointMover.Init();
        _holeMaskController = GetObject((int)GameObjects.Controller).GetComponent<HoleMaskController>();
        _holeMaskController.Init();

		return true;
    }
}
