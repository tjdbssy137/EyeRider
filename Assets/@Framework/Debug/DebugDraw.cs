using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.HableCurve;

public static class DebugDraw
{
    private static int _segments = 24;       // 원을 구성할 선분 수
    private static float _forwardLength = 5f;  // 전방 벡터의 길이

    /// <summary>
    /// 게임 Scene에 구체(와이어프레임)와 전방 벡터 선을 그립니다.
    /// </summary>
    /// <param name="center">구의 중심</param>
    /// <param name="radius">구의 반지름</param>
    /// <param name="forward">전방 벡터 (이미 정규화 되어있지 않으면 내부에서 정규화 처리)</param>
    /// <param name="color">선의 색상</param>
    /// <param name="duration">선이 유지되는 시간 (0 이하이면 삭제하지 않음)</param>
    public static void DrawSphere(Vector3 center, float radius, Vector3 forward, Color color, float duration)
    {
        // 세 평면에 원을 그림 (구체의 와이어프레임 효과)
        DrawCircle(center, radius, color, duration, PlaneType.XY);
        DrawCircle(center, radius, color, duration, PlaneType.XZ);
        DrawCircle(center, radius, color, duration, PlaneType.YZ);

        // 전방 벡터 선 그리기 (forward는 정규화하지 않은 값일 수 있으므로 내부에서 정규화)
        DrawLine(center, center + forward.normalized * _forwardLength, color, duration, "ForwardVector");
    }

    public static void DrawPoint(Vector3 center, Color color, float duration)
    {
        // 세 평면에 원을 그림 (구체의 와이어프레임 효과)
        DrawCircle(center, 0.1f, color, duration, PlaneType.XY);
        DrawCircle(center, 0.1f, color, duration, PlaneType.XZ);
        DrawCircle(center, 0.1f, color, duration, PlaneType.YZ);
    }

    private enum PlaneType { XY, XZ, YZ };

    /// <summary>
    /// 지정된 평면에 원 형태의 LineRenderer를 생성합니다.
    /// </summary>
    private static void DrawCircle(Vector3 center, float radius, Color color, float duration, PlaneType plane)
    {
        GameObject circleGO = new GameObject("DebugCircle_" + plane);
        LineRenderer lr = circleGO.AddComponent<LineRenderer>();
        SetupLineRenderer(lr, color);
        lr.positionCount = _segments + 1;

        for (int i = 0; i <= _segments; i++)
        {
            float angle = i * 2 * Mathf.PI / _segments;
            Vector3 pos = center;
            switch (plane)
            {
                case PlaneType.XY:
                    pos += new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
                    break;
                case PlaneType.XZ:
                    pos += new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                    break;
                case PlaneType.YZ:
                    pos += new Vector3(0, Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
                    break;
            }
            lr.SetPosition(i, pos);
        }

        if (duration > 0)
        {
            Object.Destroy(circleGO, duration);
        }
    }

    /// <summary>
    /// 두 점을 잇는 선(LineRenderer)을 생성합니다.
    /// </summary>
    public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration, string name)
    {
        GameObject lineGO = new GameObject("DebugLine_" + name);
        LineRenderer lr = lineGO.AddComponent<LineRenderer>();
        SetupLineRenderer(lr, color);
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        if (duration > 0)
        {
            Object.Destroy(lineGO, duration);
        }
    }

    /// <summary>
    /// LineRenderer의 기본 설정을 적용합니다.
    /// </summary>
    private static void SetupLineRenderer(LineRenderer lr, Color color)
    {
        // Sprites/Default 셰이더를 사용하여 간단한 선 재질을 만듭니다.
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.useWorldSpace = true;
    }
}
