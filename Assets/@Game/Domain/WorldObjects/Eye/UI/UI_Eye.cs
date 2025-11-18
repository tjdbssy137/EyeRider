using UnityEngine;

public class UI_Eye : UI_Base
{
    private enum Objects
    {
        Controller        
    }
    private PointMover _pointMover;
    private HoleMaskController _holeMaskController;
    public override bool Init()
    {
        if (base.Init() == false)
			return false;

        BindObjects(typeof(GameObject));
        _pointMover = GetObject((int)Objects.Controller).GetComponent<PointMover>();
        _pointMover.Init();
        _holeMaskController = GetObject((int)Objects.Controller).GetComponent<HoleMaskController>();
        _holeMaskController.Init();
        
		return true;
    }
}
