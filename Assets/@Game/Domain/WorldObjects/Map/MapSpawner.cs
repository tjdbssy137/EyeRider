using System.Collections.Generic;
using UnityEngine;

public class MapSpawner : BaseObject
{
    public Queue<Map> _loadStorage;
    private int _lastSpawnPosX = 0;
    private int _lastSpawnPosZ = 0;
    private int _maxMapCapacity = 10;
    private int MAP_SIZE = 100;
    private int _mapCount = 0;
    private GameObject _spawnParent;
    public override bool Init()
	{
		if (base.Init() == false)
			return false;
        _spawnParent = new GameObject("@Map");
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
        _mapCount = Managers.Data.MapDatas.Count;

        
    }
    
    private void RandomMap()
    {
        Vector3 spawnPos = new Vector3(MAP_SIZE * _lastSpawnPosX, 0, MAP_SIZE * _lastSpawnPosZ);
        int index = Random.Range(0, _mapCount);
        string name = Managers.Data.MapDatas[index].LoadPrefab.name;
        Managers.Object.Spawn<Map>(name, spawnPos, 0, 0, _spawnParent.transform);
    }
}
