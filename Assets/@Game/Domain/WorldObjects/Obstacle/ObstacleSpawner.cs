using UniRx;
using UnityEngine;

public class ObstacleSpawner : BaseObject
{
    private int _allowedRange = 25;
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    public override bool OnSpawn()
    {
        if (false == base.OnSpawn())
        {
            return false;
        }
        Contexts.InGame.OnSpawnMap
            .Subscribe(transform =>
            {
                SpawnItem(transform);
            }).AddTo(_disposables);
        return true;
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    public override void SetInfo(int dataTemplate)
    {
        base.SetInfo(dataTemplate);
    }

    private void SpawnItem(Transform transform)
    {
        int randomSpawn = Random.Range(0, Managers.Data.ObstacleData.Count);
        if (!Managers.Data.ObstacleData.TryGetValue(randomSpawn, out ObstacleData data))
        {
            Debug.Log($"data is NULL");
            return;
        }
        int randomX = Random.Range((int)transform.position.x - _allowedRange,(int)transform.position.x + _allowedRange);
        int randomZ = Random.Range((int)transform.position.z - _allowedRange,(int)transform.position.z + _allowedRange);
        Vector3 pos = new Vector3(randomX, transform.position.y, randomZ);
        Obstacle item = Managers.Object.Spawn<Obstacle>(data.ObstaclePrefab.name, pos, 0, data.DataTemplateId);

        float y = Random.Range(0, 180);
        item.transform.rotation = Quaternion.Euler(0, y, 0);
    }
}