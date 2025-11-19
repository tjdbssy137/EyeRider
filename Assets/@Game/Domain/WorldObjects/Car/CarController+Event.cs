using UnityEngine;

public partial class CarController : BaseObject
{
    private Animator _animator;
    private float _shakeIntensity = 0f;
    private float _maxShakeIntensity = 1.5f;  // 흔들림 최대값
    private float _shakeLerpSpeed = 2f;  
    private float _controlDifficulty = 0f; 
    public float ControlDifficulty {get { return _controlDifficulty; }}

    public void DistancePenalty(float distance)
    {
        float target = 0f;
        if(distance <= 10)
        {
            target = 0.2f;
        }
        else if(distance <= 30)
        {
            target = 0.4f;
        }
        else if(distance <= 50)
        {
            target = 0.6f;
        }
        else if(distance <= 70)
        {
            target = 0.8f;
        }
        else
        {
            target = 1f;
        }
        target = Mathf.Clamp(target, 0f, _maxShakeIntensity);

        _shakeIntensity = Mathf.Lerp(_shakeIntensity, target, Time.deltaTime * _shakeLerpSpeed);

        float normalizedValue = Mathf.InverseLerp(0f, 1f, target);
        _animator.SetFloat("Distance", normalizedValue);

        _controlDifficulty = normalizedValue;
    }
}