using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UniRx;
using UniRx.Triggers;
using static Define;

public abstract class BaseScene : InitBase
{
    public EScene SceneType { get; protected set; } = EScene.Unknown;
    public IInputSystem InputSystem { get; protected set; }
    protected readonly CompositeDisposable _disposables = new CompositeDisposable();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                if (InputSystem != null)
                    InputSystem.OnKeyAction();
            })
            .AddTo(_disposables);

        return true;
    }

    public virtual void Clear()
    {
        _disposables.Dispose();
    }
}
