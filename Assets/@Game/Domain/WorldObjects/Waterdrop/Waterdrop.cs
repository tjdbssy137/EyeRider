using UnityEngine;
using UnityEngine.UI;

public class Waterdrop : BaseObject
{
    private Image _image;
    public Sprite[] _waterdropList;

    private Vector2 _dir;
    private float _speed;

    private static readonly Vector3[] _sizeList =
    {
        new Vector3(0.5f, 0.5f, 1f),
        new Vector3(0.8f, 0.8f, 1f),
        new Vector3(1.0f, 1.0f, 1f),
        new Vector3(1.3f, 1.3f, 1f),
    };

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        if (_image == null)
        {
            _image = GetComponent<Image>();
        }

        return true;
    }

    public void SetInfo(Vector2 baseDir,float speed)
    {
        RectTransform rt = transform as RectTransform;

        _dir = (baseDir + new Vector2(Random.Range(-0.25f, 0.25f), Random.Range(-0.15f, 0.15f))).normalized;
        _speed = speed;

        _image.sprite = _waterdropList[Random.Range(0, _waterdropList.Length)];
        transform.localScale = _sizeList[Random.Range(0, _sizeList.Length)];
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;

        RectTransform rt = transform as RectTransform;
        rt.anchoredPosition += _dir * _speed * dt;

        if (rt.anchoredPosition.y < -900f)
        {
            Managers.Resource.Destroy(this.gameObject);
        }
    }
}
