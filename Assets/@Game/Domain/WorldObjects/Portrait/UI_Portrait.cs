using UniRx;
using UniRx.Triggers;
using UnityEngine;
using DG.Tweening;

public class UI_Portrait : UI_Base
{
    private Animator _animator;
    private enum States
    {
        None,
        Fine,
        Embarrased,
        Panic,
        Faint,
    }
    float _center = 0;
    private States _state = States.Fine;
    public override bool Init()
    {
        if (base.Init() == false)
			return false;

        _animator = this.GetComponentInChildren<Animator>();
        if (_animator == null)
        {
            Debug.LogWarning("_animator is NULL");
        }

        this.UpdateAsObservable()
        .Subscribe(_=>
        {   
            if(Contexts.InGame.IsGameOver)
            {
                _animator.SetTrigger("Faint");
                return;
            }
            UpdatePortrait();
        }).AddTo(this);        

		return true;
    }

    private void UpdatePortrait()
    {
        if (Contexts.InGame.IsGameOver)
        {
            _animator.SetTrigger("Faint");
        }

        if(Contexts.InGame.AKey)
        {
            _center = -1;   
        }
        else if(Contexts.InGame.DKey)
        {
            _center = 1;         
        }
        else
        {
            _center = 0;
        }

        _animator.SetFloat("Center", _center);
        _animator.SetFloat("Panic", Contexts.InGame.PanicPoint);
        // switch(_state)
        // {
        //     case States.Fine:
        //     {
                    
        //     }
        //     break;
        //     case States.Embarrased:
        //     {
                    
        //     }
        //     break;
        //     case States.Panic:
        //     {
                    
        //     }
        //     break;
        //     case States.Faint:
        //     {
                    
        //     }
        //     break;
        //     case States.None:
        //     break;
        // }
    }



}
