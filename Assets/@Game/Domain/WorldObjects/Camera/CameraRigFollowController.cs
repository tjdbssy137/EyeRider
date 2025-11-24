using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class CameraRigFollowController : BaseObject
{
    [SerializeField] private Transform _car;
    [SerializeField] private float _followDamping = 12f;
    [SerializeField] private float _rotateDamping = 8f;
    [SerializeField] private float _sideLimit = 15f;

    private Vector3 _center;
    private Vector3 _smoothPos;
    private float _smoothYaw = 0f;
    private bool _initialized = false;

    private Vector3 _worldRight;
    private Vector3 _worldForward;

    private bool _inCorner = false;

    public override bool OnSpawn()
    {
        if (!base.OnSpawn()) return false;

        _car = Contexts.InGame.Car.transform;
        if (_car == null)
        {
            Debug.Log("_car is NULL");
        }

        Contexts.InGame.WorldRightDir
            .Subscribe(r => _worldRight = r)
            .AddTo(_disposables);

        Contexts.InGame.WorldForwardDir
            .Subscribe(f => _worldForward = f)
            .AddTo(_disposables);

        Contexts.InGame.CurrentMapXZ
            .Subscribe(mapPos =>
            {
                _center = mapPos;
            }).AddTo(_disposables);

        Contexts.InGame.OnEnterCorner
        .Subscribe(_ =>
        {
            //_cornerCount = _cornerCount + 1;
            _inCorner = true;
            Debug.Log($"_inCorner : {_inCorner}");
        })
        .AddTo(_disposables);

        Contexts.InGame.OnExitCorner
        .Subscribe(_ =>
        {
            _inCorner = false;
            Debug.Log($"_inCorner : {_inCorner}");
        })
        .AddTo(_disposables);


        this.FixedUpdateAsObservable()
            .Subscribe(_ => UpdateCamera())
            .AddTo(_disposables);

        return true;
    }

    private void UpdateCamera()
    {
        float carYaw = _car.rotation.eulerAngles.y;

        _smoothYaw = Mathf.LerpAngle(
            _smoothYaw,
            carYaw,
            Time.fixedDeltaTime * _rotateDamping
        );

        Quaternion camRot = Quaternion.Euler(0f, _smoothYaw, 0f);

        float height = 25f;
        float back = 5f;

        Vector3 desiredOffset =
            camRot * new Vector3(0f, 0f, -back)
            + new Vector3(0f, height, 0f);

        Vector3 desiredPos = _car.position + desiredOffset;

        if(!_initialized)
        {
            _initialized = true;
            _smoothPos = desiredPos;
        }

        Vector3 nextPos = Vector3.Lerp(
            _smoothPos,
            desiredPos,
            Time.fixedDeltaTime * _followDamping
        );

        float pitch =0;
        if (_inCorner)
        {
            float yawDiff = Mathf.Abs(Mathf.DeltaAngle(_smoothYaw, _car.rotation.eulerAngles.y));

            if (yawDiff < 0.1f)
            {
                //회전 시작 전
                transform.position = _smoothPos;

                pitch = 55f;
                transform.rotation = Quaternion.Euler(pitch, _smoothYaw, 0f);
                return;
            }

            //회전이 실제로 시작됨
            _smoothPos = nextPos;
            transform.position = _smoothPos;

            float pitch2 = 55f;
            transform.rotation = Quaternion.Euler(pitch2, _smoothYaw, 0f);
            return;
        }


        float dist = Vector3.Dot(nextPos - _center, _worldRight);
        float absDist = Mathf.Abs(dist);

        float t = absDist / _sideLimit;
        float decay = Mathf.Clamp01(1f - t);

        Vector3 move = nextPos - _smoothPos;
        Vector3 rightComponent = Vector3.Project(move, _worldRight);
        Vector3 nonRight = move - rightComponent;

        Vector3 limitedMove = nonRight + rightComponent * decay;
        Vector3 newPos = _smoothPos + limitedMove;

        if(_worldRight.x < 0.99f && _worldRight.z < 0.99f)
        {
            _smoothPos = newPos;
        }
        else
        {
            if(0.99f < _worldRight.z || _worldRight.z < -0.99f)
            {
                float camZ = newPos.z;
                float centerZ = _center.z;
                float minZ = centerZ - _sideLimit;
                float maxZ = centerZ + _sideLimit;

                if(camZ < minZ)
                {
                    camZ = minZ;
                }

                if(maxZ < camZ)
                {
                    camZ = maxZ;
                }

                newPos.z = camZ;
                _smoothPos = newPos;
            }
            else if(0.99f < _worldRight.x || _worldRight.x < -0.99f)
            {
                float camX = newPos.x;
                float centerX = _center.x;
                float minX = centerX - _sideLimit;
                float maxX = centerX + _sideLimit;

                if(camX < minX)
                {
                    camX = minX;
                }

                if(maxX < camX)
                {
                    camX = maxX;
                }

                newPos.x = camX;
                _smoothPos = newPos;
            }
            else
            {
                _smoothPos = newPos;
            }
        }

        transform.position = _smoothPos;

        pitch = 55f;
        transform.rotation = Quaternion.Euler(pitch, _smoothYaw, 0f);
    }
}
