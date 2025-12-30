using UniRx;
using UnityEngine;

public class UI_MissileIndicator : UI_Base
{
    private Transform _missile;
    private RectTransform _indicatorRect;
    private Camera _mainCam;

    private enum RectTransforms
    {
        IndicatorArrow,
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindRectTransforms(typeof(RectTransforms));
        _indicatorRect = GetRectTransform((int)RectTransforms.IndicatorArrow);

        _mainCam = Camera.main;

        Observable.EveryUpdate()
            .Subscribe(_ =>
            {
                if (_missile == null || !_missile.gameObject.activeInHierarchy)
                {
                    Managers.Resource.Destroy(this.gameObject);
                    return;
                }

                Vector3 screenPos = _mainCam.WorldToScreenPoint(_missile.position);

                // 미사일이 화면 안에 있는지 체크
                bool isOffScreen = screenPos.x <= 0 || Screen.width <= screenPos.x ||
                                   screenPos.y <= 0 || Screen.height <= screenPos.y ||
                                   screenPos.z < 0;

                if (isOffScreen)
                {
                    _indicatorRect.gameObject.SetActive(true);
                    UpdateIndicatorPosition(screenPos);
                }
                else
                {
                    _indicatorRect.gameObject.SetActive(false);
                }

            })
            .AddTo(this);
        return true;
    }
    public void SetTargetMissile(Transform missileTransform)
    {
        _missile = missileTransform;
    }

    private void UpdateIndicatorPosition(Vector3 screenPos)
    {
        if (screenPos.z < 0)
        {
            screenPos *= -1f;
        }

        Vector2 screenCenter = new Vector2(Screen.width, Screen.height) * 0.5f;
        Vector2 direction = ((Vector2)screenPos - screenCenter).normalized;

        // Screen.width * 0.4f 는 화면 가로 폭의 40% 지점에 배치하겠다는 뜻입니다.
        // 9:16 비율에서 가로 폭을 넘지 않으면서 적절한 안쪽 위치를 잡기에 좋습니다.
        float distance = Screen.width * 0.4f;

        // 중심에서 방향으로 distance만큼 떨어진 곳
        Vector2 indicatorPos = direction * distance;

        _indicatorRect.anchoredPosition = indicatorPos;

        float rotationAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        _indicatorRect.rotation = Quaternion.Euler(0, 0, rotationAngle - 90f);
    }
}
