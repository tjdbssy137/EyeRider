using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "ObstacleData", menuName = "Scriptable Objects/ObstacleData")]
public class ObstacleData : ScriptableObject
{
    public int DataTemplateId;
    public GameObject ObstaclePrefab;
    public ObstacleType Type = ObstacleType.None;
    public ItemData[] Items;
    public float[] ItemWeights;
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (DataTemplateId != 0)
            return;

        string[] guids = AssetDatabase.FindAssets("t:ObstacleData");
        int count = guids.Length;

        string path = AssetDatabase.GetAssetPath(this);
        int index = System.Array.IndexOf(guids, AssetDatabase.AssetPathToGUID(path));

        if (0 <= index)
        {
            DataTemplateId = index;
            EditorUtility.SetDirty(this);
            Debug.Log($"[ObstacleData] 자동 ID 할당: {DataTemplateId} ({name})");
        }
    }
#endif
}

public enum ObstacleType
{
    None,
    Guard,
    ItemBox,
}

[CustomEditor(typeof(ObstacleData))]
public class ObstacleDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var typeProp = serializedObject.FindProperty("Type");
        EditorGUILayout.PropertyField(typeProp);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("DataTemplateId"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ObstaclePrefab"));

        // 아이템 박스일 때만 노출
        if ((ObstacleType)typeProp.enumValueIndex == ObstacleType.ItemBox)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("ItemBox Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("Items"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ItemWeights"), true);

            var itemsProp = serializedObject.FindProperty("Items");
            var weightsProp = serializedObject.FindProperty("ItemWeights");
            if (itemsProp.arraySize != weightsProp.arraySize)
            {
                EditorGUILayout.HelpBox("Items 배열과 ItemWeights 배열 길이가 다릅니다.", MessageType.Warning);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
