using UnityEngine;
using Unity.Cinemachine;
using UniRx;

[SaveDuringPlay]
[ExecuteAlways]
public class CinemachineClampExtension : CinemachineExtension
{
    public float _halfWidth = 40f;
    public Transform _car;
    public Vector3 _mapCenter;

    protected override void Awake()
    {
        base.Awake();

        Contexts.InGame.CurrentMapXZ
            .Subscribe(pos => _mapCenter = pos)
            .AddTo(this);
    }

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Body)
            return;

        if (_car == null)
        {
            _car = Contexts.InGame.Car.transform;
        }

        Vector3 pos = state.RawPosition;

        // 차량의 local axis
        Vector3 right = _car.right;
        right.y = 0f;
        right.Normalize();

        Vector3 forward = _car.forward;
        forward.y = 0f;
        forward.Normalize();

        // 카메라 기준 offset 계산
        Vector3 toCam = pos - _mapCenter;
        float lateral = Vector3.Dot(toCam, right);
        float forwardOffset = Vector3.Dot(toCam, forward);
        float height = pos.y - _mapCenter.y;

        // Clamp
        float clampedLateral = Mathf.Clamp(lateral, -_halfWidth, _halfWidth);

        // 다시 월드 좌표로 변환
        Vector3 newPos =
            _mapCenter
            + right * clampedLateral
            + forward * forwardOffset
            + Vector3.up * height;

        // 최종 위치 수정
        state.PositionCorrection += (newPos - pos);
    }
}
