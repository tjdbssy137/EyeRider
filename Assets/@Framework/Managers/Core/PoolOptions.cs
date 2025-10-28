using UnityEngine;

public class PoolOptions : MonoBehaviour
{
    [SerializeField]
    private PoolOptionsData _data = new PoolOptionsData();

    public PoolOptionsData Data => _data;
}

[System.Serializable]
public struct PoolOptionsData
{
    const int DefaultCapacity = 1;
    public int Capacity;

    public PoolOptionsData(int capacity = 0)
    {
        if(capacity <= 0)
            capacity = DefaultCapacity;
        Capacity = capacity;
    }
}