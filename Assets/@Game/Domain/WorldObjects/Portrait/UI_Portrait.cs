using UniRx;
using UniRx.Triggers;
using UnityEngine;
using DG.Tweening;

public class UI_Portrait : UI_Base
{
    public override bool Init()
    {
        if (base.Init() == false)
			return false;

        this.UpdateAsObservable().Subscribe(_=>
        {
            
        }).AddTo(this);

		return true;
    }

    public void SetInfo(bool can)
    {
        if(!can)
        {
            return;
        }
        

    }
}
