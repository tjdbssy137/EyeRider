using System.Collections.Generic;
using UnityEngine;

public class MapSpawner : BaseObject
{
    public List<GameObject> _loads;
    private int _lastSpawnPosX = 0;
    private int _lastSpawnPosZ = 0;
    private int _maxMapCapacity = 5;
    private int MAP_SIZE = 100;
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
    
    private void RandomMap()
    {
        Vector3 spawnPos = new Vector3(MAP_SIZE * _lastSpawnPosX, 0, MAP_SIZE * _lastSpawnPosZ);
        int index = Random.Range(0, _loads.Count);
        Managers.Object.Spawn<Map>(spawnPos, 0, 0);
    }
}
