using UniRx;
using UniRx.Triggers;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class Missile : BaseObject
{
    private Transform _target;
    private float _speed = 60f; // 이동 속도
    public float _rotateSpeed = 50f; // 회전 속도 (유도 성능)

    private Collider _collider;
    public GameObject _particle;
    private float _damage = 20f;
    private float _lifeTime = 8f;
    private GameObject _missile;

    public override bool Init()
    {
        if(base.Init() == false)
        {
            return false;
        }

        GameObject car = Contexts.InGame.Car.gameObject;
        if (car == null)
        {
            Debug.LogError("car is Null");
        }
        _target = car.transform;

        Observable.EveryFixedUpdate()
            .Subscribe(_ =>
            {
                if (_target == null)
                {
                    return;
                }
                Vector3 direction = (_target.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, _rotateSpeed * Time.fixedDeltaTime);
                transform.Translate(Vector3.forward * _speed * Time.fixedDeltaTime);
            }).AddTo(_disposables);

        _collider.OnTriggerEnterAsObservable()
            .Where(collision => collision.gameObject.CompareTag("Player"))
            .Subscribe(other =>
            {
                Managers.Object.Spawn<ParticleObject>($"{_particle.name}", this.transform.position, 0, 0);
                Contexts.InGame.OnCollisionMissile.OnNext(_damage);
                Managers.Resource.Destroy(this.gameObject);
            }).AddTo(_disposables);

        Observable.Timer(System.TimeSpan.FromSeconds(_lifeTime))
            .Subscribe(_ =>
            {
                Managers.Resource.Destroy(this.gameObject);
            }).AddTo(_disposables);
        return true;
    }

    public override bool OnSpawn()
    {
        if (base.OnSpawn() == false)
        {
            return false;
        }

        return true;
    }

    public void SetInfo(ObstacleData data)
    {
        if (data == null)
        {
            Debug.LogWarning("Missile SetInfo data is NULL");
            return;
        }
        _damage = data.CrashDamage;
        _missile = data.ObstaclePrefab;
        _collider = GetComponent<Collider>();
        if (_collider == null)
        {
            Debug.LogError("Collider is Null");
        }

    }
    public override void OnDespawn()
    {
        base.OnDespawn();
    }
}
