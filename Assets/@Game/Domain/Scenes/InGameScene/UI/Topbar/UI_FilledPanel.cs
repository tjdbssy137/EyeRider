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
        float unit = max / 5f;

        for (int i = 0; i < 5; i++)
        {
            var obj = GetObject(i);
            if (obj == null)
            {
                continue;
            }

            var cell = obj.GetComponent<UI_FilledObject>();

            int reverseIndex = 4 - i;
            float start = unit * reverseIndex;
            float end = unit * (reverseIndex + 1);

            float fill;
            if (current <= start)
            {
                fill = 0f;
            }
            else if (end <= current)
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