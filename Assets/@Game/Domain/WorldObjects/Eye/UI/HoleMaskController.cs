using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class HoleMaskController : UI_Base
{
    [Range(0f, 1f)]
    public float _backgroundAlpha = 1f;
    public Image _backgroundImage;
    public RectTransform _point; // 움직이는 점
    public RectTransform _canvasRect;
    public RectTransform _eyeImages;
    
    public Camera _canvasCamera; // overlay->camera 모드 대응 카메라. 이거 없으면 크기 이상함
    public float _rotateSpeed = 15f;

    public override bool Init()
    {
        if (!base.Init())
            return false;

        _canvasCamera = Camera.main;
        if (_canvasCamera != null)
        {
            Debug.Log($"_canvasCamera is NULL");
        }

        Vector2 canvasSize = new Vector2(_canvasRect.rect.width, _canvasRect.rect.height);
        _backgroundImage.material.SetVector("_CanvasSize", canvasSize);

        _backgroundImage.material.SetFloat("_BackgroundAlpha", _backgroundAlpha);

        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                if (true == Contexts.InGame.IsEnd)
                {
                    return;
                }
                if (true == Contexts.InGame.IsPaused)
                {
                    return;
                }
                Move();
                RotateEyeCloud();
            })
            .AddTo(this);

        return true;
    }

    private void Move()
    {
        if (_backgroundImage.material == null)
            return;

        // 변경: WorldToScreenPoint 호출 시 _canvasCamera 사용
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(_canvasCamera, _point.position);

        // 변경: ScreenPointToLocalPointInRectangle 호출 시에도 _canvasCamera 사용
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect, screenPos, _canvasCamera, out localPos // null 대신 _canvasCamera 사용
        );

        Vector2 canvasPos = localPos + _canvasRect.rect.size * 0.5f;

        _backgroundImage.material.SetVector("_CenterPx", canvasPos);


        // ======== 수정된 부분: 정확한 픽셀 크기 계산 ========

        // _point의 로컬 폭(rect.width)을 기반으로 화면상의 픽셀 폭을 계산합니다.

        // 1. RectTransform의 로컬 폭의 절반을 나타내는 월드 좌표를 얻기 위해 오른쪽 모서리 지점을 계산
        Vector3 rightEdgeLocal = new Vector3(_point.rect.width * 0.5f, 0f, 0f);

        // 2. _point의 중앙(WorldToScreenPoint에서 사용됨)의 월드 좌표를 얻습니다.
        // _point.position 대신 _point.TransformPoint(Vector3.zero)를 사용해도 되지만, 
        // 이미 _point.position이 월드 포지션이므로 이 지점은 굳이 다시 계산하지 않습니다. 

        // 3. 오른쪽 모서리 지점을 월드 좌표로 변환 (동일)
        Vector3 rightEdgeWorld = _point.TransformPoint(rightEdgeLocal);

        // 4. 오른쪽 모서리 지점과 중앙 지점 사이의 스크린 픽셀 거리를 계산
        Vector3 centerScreen = new Vector3(screenPos.x, screenPos.y, 0f);
        // 변경: WorldToScreenPoint 호출 시 _canvasCamera 사용
        Vector3 rightEdgeScreen = RectTransformUtility.WorldToScreenPoint(_canvasCamera, rightEdgeWorld);

        // 5. 중앙에서 모서리까지의 픽셀 거리 = 화면상의 반경
        float radiusPxScreen = Vector3.Distance(centerScreen, rightEdgeScreen);

        // 6. 셰이더에 전달할 최종 Radius 계산 (원본 코드의 0.4f 조정치 적용)
        // 원본 코드는 (로컬 폭 * lossyScale)의 0.4배를 사용했습니다.
        // 여기서 radiusPxScreen은 이미 화면상의 실제 반경이므로, 
        // 최종 Radius는 이 값에 0.8f;를 곱한 값 (즉, 실제 반경의 40%)이 됩니다.
        float radiusPx = radiusPxScreen * 0.8f;

        // ===============================================

        _backgroundImage.material.SetFloat("_RadiusPx", radiusPx);

        // float featherPx = radiusPx * 0.3f;
        // _backgroundImage.material.SetFloat("_FeatherPx", featherPx);

        _backgroundImage.material.SetVector("_HoleSize", new Vector2(1f, 1f));
    }

    private void RotateEyeCloud()
    {
        _eyeImages.Rotate(0f, 0f, _rotateSpeed * Time.deltaTime);
    }

}
