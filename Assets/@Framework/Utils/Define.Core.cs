using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static partial class Define
{
	public static float Gravity = -9.8f;

	public enum EThread
	{
		Main,
		Background,
    }

    public enum EUIEvent
	{
		Click,
		PointerDown,
		PointerUp,
		BeginDrag,
		Drag,
		EndDrag,
    }

    public enum EGOEvent
    {
        MouseDown,
		MouseDrag,
        MouseEnter,
        MouseExit,
        MouseOver,
        MouseUp,
        MouseUpAsButton,

        TriggerEnter,
        TriggerStay,
        TriggerExit,
    }

    public enum ESound
	{
		Bgm,
		Effect,
		Max,
	}

	public enum EMouseEvent
	{
		Press,
        Click,
	}


	//Unity Tool Layer와 동일한 내용
	public enum ELayer
	{
		Default,
		TransparentFX,
		IgnoreRaycast,
		None,
		Water,
		UI,

		Ground,
		Object
	}
}
