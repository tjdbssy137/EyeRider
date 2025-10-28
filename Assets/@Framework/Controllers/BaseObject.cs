using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public partial class BaseObject : InitBase
{
    public int DataTemplateID { get; set; }

    public int ObjectId { get; set; }

    protected CompositeDisposable _disposables { get; private set; } = new CompositeDisposable();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    bool _spwanInit = false;
    public virtual bool OnSpawn()
    {
        if (_spwanInit)
            return false;

        _spwanInit = true;

        return true;
    }

    public virtual void OnDespawn()
    {
        _spwanInit = false;
        _disposables.Dispose();
        _disposables = new CompositeDisposable();
    }

    public virtual void SetInfo(int dataTemplate)
    {
        DataTemplateID = dataTemplate;
    }

    public virtual void Clear()
    {
        _init = false;
        _disposables.Dispose();
        _disposables = new CompositeDisposable();
    }

    #region Static Functions
    public static void BindEvent(GameObject go, Action action, Define.EGOEvent type)
    {
        GO_EventHandler evt = Util.GetOrAddComponent<GO_EventHandler>(go);

        switch (type)
        {
            case Define.EGOEvent.MouseEnter:
                evt.OnMouseEnterHandler -= action;
                evt.OnMouseEnterHandler += action;
                break;
            case Define.EGOEvent.MouseExit:
                evt.OnMouseExitHandler -= action;
                evt.OnMouseExitHandler += action;
                break;
            case Define.EGOEvent.MouseDown:
                evt.OnMouseDownHandler -= action;
                evt.OnMouseDownHandler += action;
                break;
            case Define.EGOEvent.MouseDrag:
                evt.OnMouseDragHandler -= action;
                evt.OnMouseDragHandler += action;
                break;
            case Define.EGOEvent.MouseOver:
                evt.OnMouseOverHandler -= action;
                evt.OnMouseOverHandler += action;
                break;
            case Define.EGOEvent.MouseUp:
                evt.OnMouseUpHandler -= action;
                evt.OnMouseUpHandler += action;
                break;
            case Define.EGOEvent.MouseUpAsButton:
                evt.OnMouseUpAsButtonHandler -= action;
                evt.OnMouseUpAsButtonHandler += action;
                break;
        }
    }

    public static void BindEvent(GameObject go, Action<Collider> action, Define.EGOEvent type)
    {
        GO_EventHandler evt = Util.GetOrAddComponent<GO_EventHandler>(go);

        switch (type)
        {
            case Define.EGOEvent.TriggerEnter:
                evt.OnTriggerEnterHandler -= action;
                evt.OnTriggerEnterHandler += action;
                break;
            case Define.EGOEvent.TriggerStay:
                evt.OnTriggerStayHandler -= action;
                evt.OnTriggerStayHandler += action;
                break;
            case Define.EGOEvent.TriggerExit:
                evt.OnTriggerExitHandler -= action;
                evt.OnTriggerExitHandler += action;
                break;
        }
    }
    #endregion
}
