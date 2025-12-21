using UnityEngine;
using UniRx;

public class UI_Eye : UI_Base
{
    private enum GameObjects
    {
        Controller,
        Obstacles
    }


    private PointMover _pointMover;
    private HoleMaskController _holeMaskController;
    private GameObject _obstacleContainer;
    public override bool Init()
    {
        if (base.Init() == false)
			return false;

        BindObjects(typeof(GameObjects));
        _pointMover = GetObject((int)GameObjects.Controller).GetComponent<PointMover>();
        _pointMover.Init();
        _holeMaskController = GetObject((int)GameObjects.Controller).GetComponent<HoleMaskController>();
        _holeMaskController.Init();
        Canvas canvas = GetComponent<Canvas>();
        canvas.worldCamera = Camera.main;
        canvas.planeDistance = 5f;

        _obstacleContainer = GetObject((int)GameObjects.Obstacles);
        Contexts.InGame.OnEnterObstacle
            .Subscribe(Data =>
            {
                if(4 < Contexts.InGame.ObstacleQueue.Count)
                {
                    return;
                }
                UI_Obstacle obstacle = Managers.UI.ShowPopupUI<UI_Obstacle>();
                obstacle.transform.SetParent(_obstacleContainer.transform, false);
                obstacle.SetInfo(Data);
                Contexts.InGame.ObstacleQueue.Enqueue(obstacle);
            }).AddTo(this);
            

        return true;
    }
}
