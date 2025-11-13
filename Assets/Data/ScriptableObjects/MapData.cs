using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "MapData", menuName = "Scriptable Objects/MapData")]
public class MapData : ScriptableObject
{
    public int DataTemplateId;
    public GameObject RoadPrefab;
    public RoadDirection Direction = RoadDirection.none;
    [Tooltip("Prefab's model default facing: 0=+Z,1=+X,2=-Z,3=-X")]
    public int BaseFacing = 0;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (DataTemplateId != 0)
            return;

        string[] guids = AssetDatabase.FindAssets("t:MapData");
        int count = guids.Length;

        string path = AssetDatabase.GetAssetPath(this);
        int index = System.Array.IndexOf(guids, AssetDatabase.AssetPathToGUID(path));

        if (0 <= index)
        {
            DataTemplateId = index;
            EditorUtility.SetDirty(this);
            Debug.Log($"[MapData] 자동 ID 할당: {DataTemplateId} ({name})");
        }
    }
#endif
}

public enum RoadDirection
{
    none,
    Left,
    Right
}
