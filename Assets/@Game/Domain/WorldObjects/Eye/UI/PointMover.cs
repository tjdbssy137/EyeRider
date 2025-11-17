using UnityEngine;
using UniRx;

public class PointMover : UI_Base
{
    public RectTransform _point;
    public RectTransform _canvasRect;

    [Header("랜덤 이동 설정")]
    [Tooltip("랜덤 목적지로 이동하는 속도 (픽셀/초 느낌)")]
    public float _randomMoveSpeed = 200f;

    [Tooltip("몇 초마다 새로운 랜덤 목적지를 잡을지")]
    public float _randomTargetInterval = 2f;

    [Header("입력 기반 끌림 설정")]
    [Tooltip("입력 방향이 폭풍의눈 쪽을 향할 때, 얼마나 많이 차 쪽으로 끌어당길지 (0~1 비율)")]
    [Range(0f, 1f)]
    public float _inputPullFactor = 0.5f; // 0.5면 거리의 절반 정도만 줄어듦

    [Tooltip("입력에 의해 생기는 오프셋이 따라가는 속도")]
    public float _inputFollowSpeed = 8f;

    [Tooltip("입력 방향이 폭풍의 눈 방향과 얼마나 비슷해야 '끌어당기기'로 인정할지 (1=완전 같음, 0=-반대)")]
    [Range(-1f, 1f)]
    public float _directionThreshold = 0.3f; // 0.3 정도면 대충 같은 쪽

    private Vector2 _randomPos;      // 랜덤 이동 기준 위치 (캔버스 중앙 기준)
    private Vector2 _randomTarget;   // 랜덤 목적지
    private Vector2 _inputOffset;    // 입력 때문에 생기는 추가 오프셋

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        if (_point == null || _canvasRect == null)
        {
            Debug.LogWarning("PointMover: RectTransform 참조를 설정해주세요.");
            return false;
        }

        _randomPos = _point.anchoredPosition;
        _randomTarget = _randomPos;
        _inputOffset = Vector2.zero;

        // 일정 시간마다 랜덤 목적지 갱신
        Observable.Interval(System.TimeSpan.FromSeconds(_randomTargetInterval))
            .Subscribe(_ => SetRandomTarget())
            .AddTo(this);

        return true;
    }

    private void SetRandomTarget()
    {
        float halfWidth = _canvasRect.rect.width * 0.5f;
        float halfHeight = _canvasRect.rect.height * 0.5f;

        // 캔버스 중앙 기준 랜덤 위치 선택
        float x = Random.Range(-halfWidth, halfWidth);
        float y = Random.Range(-halfHeight, halfHeight);

        _randomTarget = new Vector2(x, y);
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // ==========================
        // 1) 랜덤 이동 업데이트
        // ==========================
        _randomPos = Vector2.MoveTowards(
            _randomPos,
            _randomTarget,
            _randomMoveSpeed * dt
        );

        // ==========================
        // 2) Car 이동키 기반 "끌어당김" 계산
        // ==========================
        Vector2 inputDir = ReadInputDirection(); // WASD → (-1~1, -1~1)

        Vector2 targetOffset = Vector2.zero;

        if (inputDir.sqrMagnitude > 0.0001f)
        {
            Vector2 inputNorm = inputDir.normalized;

            // 현재 폭풍의 눈 위치 (랜덤 기준) – 차는 화면 중앙(0,0)이라고 가정
            Vector2 stormPos = _randomPos;       // 차 기준 폭풍의 눈 위치
            if (stormPos.sqrMagnitude > 0.0001f)
            {
                Vector2 carToStorm = stormPos.normalized; // 차→폭풍의 눈 방향

                // 입력 방향이 폭풍의 눈 방향을 얼마나 향하고 있는지 (코사인 값)
                float dot = Vector2.Dot(inputNorm, carToStorm);
                // dot > 0 이면 "폭풍의 눈 쪽으로 핸들을 꺾었다" 는 뜻

                if (dot > _directionThreshold)
                {
                    // 폭풍의 눈을 "차 쪽(센터)으로" 끌어당김
                    // stormPos가 (차→폭풍의 눈 이니까)
                    // 그 반대(-stormPos)가 "폭풍의 눈→차" 방향이다.
                    Vector2 stormToCar = -stormPos;

                    // 현재 거리의 일부만큼만 당겨오기 (inputPullFactor 만큼)
                    Vector2 pull = stormToCar * _inputPullFactor;

                    targetOffset = pull;
                }
                else
                {
                    // 입력이 반대 방향이면 끌어당기지 않고, 서서히 0으로 복귀
                    targetOffset = Vector2.zero;
                }
            }
        }
        else
        {
            // 입력이 없으면 오프셋을 0으로 복귀
            targetOffset = Vector2.zero;
        }

        // 오프셋을 부드럽게 따라가게
        _inputOffset = Vector2.Lerp(_inputOffset, targetOffset, _inputFollowSpeed * dt);

        // ==========================
        // 3) 최종 위치 = 랜덤 + 입력 오프셋
        // ==========================
        Vector2 finalPos = _randomPos + _inputOffset;

        // 캔버스 안으로 클램프
        float halfW = _canvasRect.rect.width * 0.5f;
        float halfH = _canvasRect.rect.height * 0.5f;

        finalPos.x = Mathf.Clamp(finalPos.x, -halfW, halfW);
        finalPos.y = Mathf.Clamp(finalPos.y, -halfH, halfH);

        _point.anchoredPosition = finalPos;
    }

    private Vector2 ReadInputDirection()
    {
        Vector2 dir = Vector2.zero;

        // A / D → 좌우
        if (Contexts.InGame.AKey) dir.x -= 1f;
        if (Contexts.InGame.DKey) dir.x += 1f;

        // W / S → 위/아래
        if (Contexts.InGame.WKey) dir.y += 1f;
        if (Contexts.InGame.SKey) dir.y -= 1f;

        return dir;
    }
}