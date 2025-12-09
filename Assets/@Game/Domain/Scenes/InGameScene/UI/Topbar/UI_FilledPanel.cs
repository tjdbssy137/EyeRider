using UnityEngine;

public class UI_FilledPanel : UI_Base
{
    private enum FilledObjects
    {
        FilledObject1,
        FilledObject2,
        FilledObject3,
        FilledObject4,
        FilledObject5,
    }

    public override bool Init()
    {
        if (base.Init() == false)
        {
            return false;
        }

        BindObjects(typeof(FilledObjects));
        return true;
    }

    public void UpdateValue(float current, float max)
    {
        float unit = max / 5f; // 각 칸이 담당하는 양

        for (int i = 0; i < 5; i++)
        {
            var obj = GetObject(i);
            if (obj == null)
            {
                Debug.LogError($"FilledObject{i + 1} 없음");
                continue;
            }

            var cell = obj.GetComponent<UI_FilledObject>();

            float start = unit * i;
            float end = unit * (i + 1);

            float fill;
            if (current <= start)
            {
                fill = 0f;
            }
            else if (current >= end)
            {
                fill = 1f;
            }
            else
            {
                fill = (current - start) / unit;
            }
            cell.SetFill(fill);
        }
    }
}