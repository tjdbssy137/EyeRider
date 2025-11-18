using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class Eye : BaseObject
{
    public float _moveSpeed = 20;
    public float _turnSpeed = 3f;
    public CapsuleCollider _capsuleCollider;

    private Vector3 _currentDirection = Vector3.forward;
    private Vector3 _targetDirection = Vector3.forward;
    private Camera _cam;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        if (_capsuleCollider == null)
        {
            Debug.LogWarning("_capsuleCollider is NULL");
        }

        _cam = Camera.main;
        if (_cam == null)
        {
            Debug.LogWarning("_cam is NULL");
        }

        Contexts.InGame.OnStartGame.Subscribe(_ =>
        {

        }).AddTo(_disposables);

        return true;
    }

    public override bool OnSpawn()
    {
        if (false == base.OnSpawn())
        {
            return false;
        }

        _capsuleCollider.OnTriggerExitAsObservable()
        .Where(collision => collision.gameObject.CompareTag("Player"))
        .Subscribe(_ =>
        {
            // eye에서 자동차가 벗어났을 시
            Contexts.InGame.OnExitEye.OnNext(Unit.Default);
        }).AddTo(_disposables);

        Observable
            .Defer(() => Observable.Timer(TimeSpan.FromSeconds(UnityEngine.Random.Range(1f, 3f))))
            .Repeat()
            .ObserveOnMainThread()
            .Subscribe(_ =>
            {
                _targetDirection = ChangeDirection();
            })
            .AddTo(_disposables);

        Observable.EveryLateUpdate()
            .ObserveOnMainThread()
            .Subscribe(_ =>
            {
                _currentDirection = Vector3.Slerp(_currentDirection, _targetDirection, Time.deltaTime * _turnSpeed).normalized;
                transform.position += _currentDirection * _moveSpeed * Time.deltaTime;

                ClampInsideScreen();

                Vector3 p = transform.position;
                p.y = 0;
                transform.position = p;
            })
            .AddTo(_disposables);

        return true;
    }
    public override void SetInfo(int dataTemplate)
    {
        base.SetInfo(dataTemplate);
    }

    private void ClampInsideScreen()
    {
        Vector3 pos = transform.position;
        Vector3 viewPos = _cam.WorldToViewportPoint(pos);

        if (viewPos.z <= 0f)
        {
            _targetDirection = (_cam.transform.position - transform.position).normalized;
            viewPos.z = 0.01f;
        }

        float paddingX = 0.05f;
        float paddingY = 0.05f;

        if (_capsuleCollider != null)
        {
            float worldRadius = Mathf.Max(_capsuleCollider.radius, _capsuleCollider.height * 0.5f);
            Vector3 rightEdgeWorld = pos + _cam.transform.right * worldRadius;
            Vector3 rightEdgeView = _cam.WorldToViewportPoint(rightEdgeWorld);
            paddingX = Mathf.Abs(rightEdgeView.x - viewPos.x);
            if (paddingX <= 0f)
            {
                paddingX = 0.02f;
            }

            Vector3 upEdgeWorld = pos + _cam.transform.up * worldRadius;
            Vector3 upEdgeView = _cam.WorldToViewportPoint(upEdgeWorld);
            paddingY = Mathf.Abs(upEdgeView.y - viewPos.y);
            if (paddingY <= 0f)
            {
                paddingY = 0.02f;
            }
        }

        Vector3 originalViewPos = viewPos;

        viewPos.x = Mathf.Clamp(viewPos.x, 0f + paddingX, 1f - paddingX);
        viewPos.y = Mathf.Clamp(viewPos.y, 0f + paddingY, 1f - paddingY);

        bool wasTryingToLeave =
            originalViewPos.x < (0f + paddingX) || originalViewPos.x > (1f - paddingX) ||
            originalViewPos.y < (0f + paddingY) || originalViewPos.y > (1f - paddingY);

        if (wasTryingToLeave)
        {
            _targetDirection = ChangeDirection();
        }

        Vector3 newWorld = _cam.ViewportToWorldPoint(viewPos);
        newWorld.y = 0;
        transform.position = newWorld;
    }

    private Vector3 ChangeDirection()
    {
        float xPos = UnityEngine.Random.Range(-1f, 1f);
        xPos = Mathf.Clamp(xPos, -1f, 1f);
        float zPos = UnityEngine.Random.Range(-1f, 1f);
        zPos = Mathf.Clamp(zPos, -1f, 1f);
        Vector3 direction = new Vector3(xPos, 0, zPos).normalized;
        return direction;
    }
}