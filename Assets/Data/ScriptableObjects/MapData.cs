using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "Scriptable Objects/MapData")]
public class MapData : ScriptableObject
{
    public int DataTemplateId;
    public GameObject LoadPrefab;
    public LoadDirection Direction = LoadDirection.none;
}

public enum LoadDirection
{
    none,
    Left,
    Right
}
