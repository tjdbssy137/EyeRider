using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class Waterdrop : BaseObject
{
    private SpriteRenderer _sprite;
    public Sprite[] _waterdropList;

    private Vector2 _dir;
    private float _speed;
    private bool _isInit = false;
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

        if (_sprite == null)
        {
            _sprite = GetComponent<SpriteRenderer>();
        }
        _isInit = false;
        Observable.EveryUpdate().Subscribe(_ =>
        {
            if(!_isInit)
            {
                return;
            }
            if (Contexts.InGame.IsPaused || Contexts.InGame.IsEnd)
            {
                return;
            }
            Move();
        }).AddTo(this.gameObject);
        return true;
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    public void SetInfo(Vector2 spawnPos, Vector2 baseDir,float speed)
    {
        RectTransform rt = transform as RectTransform;
        rt.localPosition = spawnPos;
        _dir = (baseDir + new Vector2(Random.Range(-0.25f, 0.25f), Random.Range(-0.15f, 0.15f))).normalized;
        _speed = speed;

        _sprite.sprite = _waterdropList[Random.Range(0, _waterdropList.Length)];
        transform.localScale = _sizeList[Random.Range(0, _sizeList.Length)];
        _isInit = true;
    }

    private void Move()
    {
        float dt = Time.unscaledDeltaTime;

        RectTransform rt = transform as RectTransform;
        rt.anchoredPosition += _dir * _speed * dt;

        if (rt.anchoredPosition.y < -25f)
        {
            Managers.Resource.Destroy(this.gameObject);
        }
    }
}
