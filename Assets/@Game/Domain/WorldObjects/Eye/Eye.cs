using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class Eye : BaseObject
{
    public float _moveSpeed = 5;
    public float _turnSpeed = 3f;
    public CapsuleCollider _capsuleCollider;

    private Vector3 _currentDirection = Vector3.forward;
    private Vector3 _targetDirection = Vector3.forward;
    private Camera _cam;

    public override bool Init()
	{
        if (base.Init() == false)
            return false;

        if(_capsuleCollider == null)
        {
            Debug.LogWarning("_capsuleCollider is NULL");
        }

        Contexts.InGame.OnStartGame.Subscribe(_ =>
        {

        }).AddTo(_disposables);

        // _capsuleCollider.OnTriggerExitAsObservable()
        // .Where(collision => collision.gameObject.CompareTag("Player"))
        // .Subscribe(_ =>
        // {
        //     // eye에서 자동차가 벗어났을 시
        //     Contexts.InGame.OnEyeExit.OnNext(Unit.Default);
        // }).AddTo(_disposables);

        // Observable
        // .Defer(() => Observable.Timer(TimeSpan.FromSeconds(UnityEngine.Random.Range(1f, 3f))))
        // .Repeat()
        // .ObserveOnMainThread()
        // .Subscribe(_ =>
        // {
        //     Vector3 direction = ChangeDirection();
        //     Move(direction);
        // })
        // .AddTo(_disposables);

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
            Contexts.InGame.OnEyeExit.OnNext(Unit.Default);
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

        Observable.EveryUpdate()
            .ObserveOnMainThread()
            .Subscribe(_ =>
            {
                _currentDirection = Vector3.Slerp(_currentDirection, _targetDirection, Time.deltaTime * _turnSpeed).normalized;

                transform.position += _currentDirection * _moveSpeed * Time.deltaTime;

                ClampInsideScreen();
            })
            .AddTo(_disposables);

        return true;
    }
    public override void SetInfo(int dataTemplate)
    {
        base.SetInfo(dataTemplate);
    }

    private void Move(Vector3 direction)
    {
        this.transform.position += direction * _moveSpeed * Time.deltaTime;
        ClampInsideScreen();
    }

    private void ClampInsideScreen()
    {
        if (_cam == null)
        {
            Debug.LogWarning("_cam is NULL");
            return;
        }

        Vector3 pos = transform.position;
        Vector3 viewPos = _cam.WorldToViewportPoint(pos);

        float halfSize = _capsuleCollider.radius/2;

        if (_capsuleCollider != null)
        {
            float worldRadius = Mathf.Max(_capsuleCollider.radius, _capsuleCollider.height * 0.5f);

            Vector3 edge = _cam.WorldToViewportPoint(pos + transform.right * worldRadius);
            halfSize = Mathf.Abs(edge.x - viewPos.x);

            if (halfSize <= 0f)
            {
                halfSize = 0.1f;
            }
        }

        viewPos.x = Mathf.Clamp(viewPos.x, -halfSize, 1f + halfSize);
        viewPos.y = Mathf.Clamp(viewPos.y, -halfSize, 1f + halfSize);

        viewPos.z = Mathf.Max(viewPos.z, 0.01f);

        transform.position = _cam.ViewportToWorldPoint(viewPos);
    }

    private Vector3 ChangeDirection()
    {
        float xPos = UnityEngine.Random.Range(-1, 1);
        xPos = Mathf.Clamp(xPos, -1, 1);
        float zPos = UnityEngine.Random.Range(-1, 1);
        zPos = Mathf.Clamp(zPos, -1, 1);
        Vector3 direction = new Vector3(xPos, 0, zPos).normalized;
        return direction;
    }

    
}