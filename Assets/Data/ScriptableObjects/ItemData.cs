using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    public int DataTemplateId;
    public GameObject ItemPrefab;
    public ItemType Type;
    public float Value;
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (DataTemplateId != 0)
            return;

        string[] guids = AssetDatabase.FindAssets("t:ItemData");
        int count = guids.Length;

        string path = AssetDatabase.GetAssetPath(this);
        int index = System.Array.IndexOf(guids, AssetDatabase.AssetPathToGUID(path));

        if (0 <= index)
        {
            DataTemplateId = index;
            EditorUtility.SetDirty(this);
            Debug.Log($"[ItemData] 자동 ID 할당: {DataTemplateId} ({name})");
        }
    }
#endif
}

public enum ItemType
{
    None,
    Fuel,
    Repair,
    Magnet,
    Coin
}
