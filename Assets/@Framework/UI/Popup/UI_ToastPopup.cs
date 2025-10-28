using Data;
using System;
using System.Collections;
using UnityEngine;
using static Define;

public class UI_ToastPopup : UI_Popup
{
    public enum Type
    {
        Debug,
        Info,
        Warning,
        Error,
        Critical
    }
    private enum Images
    {
        Background_Image,
    }
    private enum Texts
    {
        Notice_Text,
    }
    private string _notice;
    private Type _type;
    private float _time;
    public override bool Init()
    {
        if (base.Init() == false)
        {
            return false;
        }
        BindImages(typeof(Images));
        BindTexts(typeof(Texts));

        return true;
    }
    
    private void SetInfo(string notice, Type type = Type.Info, float time = 2f, Action onCompleteCallback = null)
    {
        _notice = notice;
        _type = type;
        _time = time;
        SetBackgroundColor();
        StartCoroutine(ToastPopup_Co(onCompleteCallback));
    }
    private void SetBackgroundColor()
    {
        Color color = new();
        switch(_type)
        {
            case Type.Debug:
                color = ToastPopupColor.DebugColor;
                break;
            case Type.Info:
                color = ToastPopupColor.InfoColor;
            break;
            case Type.Warning:
                color = ToastPopupColor.WarningColor;
            break;
            case Type.Error:
                color = ToastPopupColor.ErrorColor;
            break;
            case Type.Critical:
                color = ToastPopupColor.CriticalColor;
                break;

            default:
                color = ToastPopupColor.InfoColor;
                break;
        }
        GetImage((int)Images.Background_Image).color = color;
    }

    private IEnumerator ToastPopup_Co(Action onCompleteCallback = null)
    {
        GetText((int)Texts.Notice_Text).text = _notice;
        yield return new WaitForSeconds(_time);
        Managers.UI.ClosePopupUI(this);
        onCompleteCallback?.Invoke();
    }

    public static void ShowInfo(NoticeInfo noticeInfo, float time = 2f, Action onCompleteCallback = null)
    {
        UI_ToastPopup toast = Managers.UI.ShowPopupUI<UI_ToastPopup>();
        toast.SetInfo(noticeInfo.Notice, UI_ToastPopup.Type.Info, time, onCompleteCallback);
    }
    public static void ShowWarning(NoticeInfo noticeInfo, float time = 2f, Action onCompleteCallback = null)
    {
        UI_ToastPopup toast = Managers.UI.ShowPopupUI<UI_ToastPopup>();
        toast.SetInfo(noticeInfo.Notice, UI_ToastPopup.Type.Warning, time, onCompleteCallback);
    }
    public static void ShowError(NoticeInfo noticeInfo, float time = 2f, Action onCompleteCallback = null)
    {
        UI_ToastPopup toast = Managers.UI.ShowPopupUI<UI_ToastPopup>();
        toast.SetInfo(noticeInfo.Notice, UI_ToastPopup.Type.Error, time, onCompleteCallback);
    }
    public static void ShowCritical(NoticeInfo noticeInfo, float time = 2f, Action onCompleteCallback = null)
    {
        UI_ToastPopup toast = Managers.UI.ShowPopupUI<UI_ToastPopup>();
        toast.SetInfo(noticeInfo.Notice, UI_ToastPopup.Type.Critical, time, onCompleteCallback);
    }
    public static void Show(NoticeInfo noticeInfo, float time = 2f, Action onCompleteCallback = null)
    {
#if !UNITY_EDITOR
        if(noticeInfo.Type == NoticeInfo.EType.Debug)
            return;
#endif

        UI_ToastPopup toast = Managers.UI.ShowPopupUI<UI_ToastPopup>();
        
        switch(noticeInfo.Type)
        {
            case NoticeInfo.EType.Debug:
                toast.SetInfo(noticeInfo.Notice, Type.Debug, time, onCompleteCallback);
                break;
            case NoticeInfo.EType.Info:
                toast.SetInfo(noticeInfo.Notice, Type.Info, time, onCompleteCallback);
                break;
            case NoticeInfo.EType.Warning:
                toast.SetInfo(noticeInfo.Notice, Type.Warning, time, onCompleteCallback);
                break;
            case NoticeInfo.EType.Error:
                toast.SetInfo(noticeInfo.Notice, Type.Error, time, onCompleteCallback);
                break;
            case NoticeInfo.EType.Critical:
                toast.SetInfo(noticeInfo.Notice, Type.Critical, time, onCompleteCallback);
                break;
            default:
                toast.SetInfo(noticeInfo.Notice, Type.Info, time, onCompleteCallback);
                break;
        }
    }

    public static void Show(string message, Type type, float time = 2f, Action onCompleteCallback = null)
    {
#if !UNITY_EDITOR
        if(type == Type.Debug)
            return;
#endif

#if UNITY_EDITOR
        if (type == Type.Debug)
        {
            message = $"[DEBUG] {message}";
        }
#endif
        // ToastPopup이 겹치면 Peek쪽에 문제가 생겨서 임시로 하나만 띄울 수 있게 수정
        Managers.UI.CloseAllPopupUI();

        UI_ToastPopup toast = Managers.UI.ShowPopupUI<UI_ToastPopup>();
        toast.SetInfo(message, type, time, onCompleteCallback);
    }

    public class ToastPopupColor
    {
        public static readonly Color DebugColor = new Color(0, 1, 0, 1f);                                       // Green
        public static readonly Color InfoColor = new Color(255f / 255f, 246f / 255f, 225f / 255f, 1f);          // White
        public static readonly Color WarningColor = new Color(255f / 255f, 186f / 255f, 28f / 255f, 1f);        // Yellow
        public static readonly Color ErrorColor = new Color(159f / 255f, 159f / 255f, 159f / 255f, 1f);         // Gray
        public static readonly Color CriticalColor = new Color(255f / 255f, 177f / 255f, 177f / 255f, 1f);      // Red
    }
}
