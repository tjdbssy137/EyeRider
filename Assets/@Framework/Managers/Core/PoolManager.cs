using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

internal class Pool
{
	private GameObject _prefab;
	private IObjectPool<GameObject> _pool;

	private Transform _root;
	private Transform Root
	{
		get
		{
			if (_root == null)
			{
				GameObject go = new GameObject() { name = $"@{_prefab.name}Pool" };
				_root = go.transform;
			}

			return _root;
		}
	}

	public Pool(GameObject prefab, PoolOptionsData poolOptionsData)
	{
		_prefab = prefab;
		//if(poolOptionsData == null)
		//	poolOptionsData = new PoolOptionsData();
        _pool = new ObjectPool<GameObject>(OnCreate, OnGet, OnRelease, OnDestroy, defaultCapacity: poolOptionsData.Capacity);
	}

	public void Push(GameObject go)
	{
		if (go.activeSelf)
			_pool.Release(go);
	}

	public GameObject Pop()
	{
		return _pool.Get();
	}

	#region Funcs
	private GameObject OnCreate()
	{
		GameObject go = GameObject.Instantiate(_prefab);
		go.transform.SetParent(Root);
		go.name = _prefab.name;
		return go;
	}

	private void OnGet(GameObject go)
	{
		go.SetActive(true);
		var obj = go.GetComponent<BaseObject>();
		if (obj != null)
		{
            obj.OnSpawn();
        }
	}

	private void OnRelease(GameObject go)
	{
		go.SetActive(false);

		var obj = go.GetComponent<BaseObject>();
		if (obj != null)
		{
			obj.OnDespawn();
		}
	}

	private void OnDestroy(GameObject go)
	{
		GameObject.Destroy(go);
	}
	#endregion
}

public class PoolManager
{
	private Dictionary<string, Pool> _pools = new Dictionary<string, Pool>();

	public GameObject Pop(GameObject prefab)
	{
		if (_pools.ContainsKey(prefab.name) == false)
		{
            PoolOptions poolOptions = prefab.GetComponent<PoolOptions>();
			PoolOptionsData poolOptionsData = (poolOptions != null) ? poolOptions.Data : new PoolOptionsData();

			CreatePool(prefab, poolOptionsData);
		}

		return _pools[prefab.name].Pop();
	}

	public bool Push(GameObject go)
	{
		if (_pools.ContainsKey(go.name) == false)
			return false;

		_pools[go.name].Push(go);
		return true;
	}

	public void Clear()
	{
		_pools.Clear();
	}

	private void CreatePool(GameObject original, PoolOptionsData poolOptionsData)
	{
		Pool pool = new Pool(original, poolOptionsData);
		_pools.Add(original.name, pool);
	}
}
