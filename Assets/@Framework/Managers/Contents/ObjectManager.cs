using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static Define;

public class ObjectManager
{

    #region Roots

    public Transform GetRootTransform(string name)
    {
        GameObject root = GameObject.Find(name);
        if (root == null)
            root = new GameObject { name = name };

        return root.transform;
    }
    #endregion

    public Dictionary<int, BaseObject> ObjectDic { get; private set; } = new Dictionary<int, BaseObject>();

    private static int _objectIndexer = 0;

    public T Spawn<T>(Vector3 position, int objectId, int templateID) where T : BaseObject
    {
        string prefabName = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate(prefabName, pooling: true);
        go.name = prefabName;
        go.transform.position = position;

        BaseObject obj = go.GetComponent<BaseObject>();

        if ( 0 == objectId )
        {
            objectId = ++_objectIndexer;
        }

        obj.ObjectId = objectId;
        obj.SetInfo(templateID);

        ObjectDic.Add(objectId, obj);

        return obj as T;
    }

    
    public T Spawn<T>(string prefabName, Vector3 position, int objectId, int templateID, Transform parent) where T : BaseObject
    {
        GameObject go = Managers.Resource.Instantiate(prefabName, parent, pooling: true);
        go.name = prefabName;
        go.transform.position = position;

        BaseObject obj = go.GetComponent<BaseObject>();

        if ( 0 == objectId )
        {
            objectId = ++_objectIndexer;
        }

        obj.ObjectId = objectId;
        obj.SetInfo(templateID);
        ObjectDic.Add(objectId, obj);

        return obj as T;
    }
    
    public T Spawn<T>(Vector3 position, int objectId, int templateID, Transform parent) where T : BaseObject
    {
        string prefabName = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate(prefabName, parent, pooling: true);
        go.name = prefabName;
        go.transform.position = position;

        BaseObject obj = go.GetComponent<BaseObject>();
        obj.SetInfo(templateID);

        if ( 0 == objectId )
        {
            objectId = ++_objectIndexer;
        }

        obj.ObjectId = objectId;

        ObjectDic.Add(objectId, obj);

        return obj as T;
    }

    public void Despawn<T>(T obj) where T : BaseObject
    {
        Managers.Resource.Destroy(obj.gameObject);
        ObjectDic.Remove(obj.ObjectId);
    }

    public T GetObject<T>(int objectId) where T : BaseObject
    {
        if (ObjectDic.TryGetValue(objectId, out BaseObject obj))
        {
            return obj as T;
        }

        return null;
    }

    public void Despawn(int objectId)
    {
        if (ObjectDic.TryGetValue(objectId, out BaseObject obj))
        {
            Managers.Resource.Destroy(obj.gameObject);
            ObjectDic.Remove(objectId);
        }
    }


    public void Clear()
    {
        ObjectDic.Clear();
    }
}
