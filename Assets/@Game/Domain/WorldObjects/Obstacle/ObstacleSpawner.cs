using UnityEngine;

public class ObstacleSpawner : BaseObject
{
    private int _allowedRange = 30;
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

        return true;
    }
    
    public override void SetInfo(int dataTemplate)
    {
        base.SetInfo(dataTemplate);
    }

    private void SpawnItem(Vector3 position)
    {
        int randomSpawn = Random.Range(0, Managers.Data.ObstacleData.Count);
        if (!Managers.Data.ObstacleData.TryGetValue(randomSpawn, out ObstacleData data))
        {
            Debug.Log($"data is NULL");
            return;
        }

        int randomX = Random.Range((int)position.x - _allowedRange,(int)position.x + _allowedRange);
        int randomZ = Random.Range((int)position.z - _allowedRange,(int)position.z + _allowedRange);
        Vector3 pos = new Vector3(randomX, position.y, randomZ);
        Obstacle item = Managers.Object.Spawn<Obstacle>(data.ObstaclePrefab.name, pos, 0, data.DataTemplateId);
    }

}
