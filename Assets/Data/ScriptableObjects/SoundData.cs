using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundData", menuName = "Scriptable Objects/SoundData")]
public class SoundData : ScriptableObject
{
    public int DataTemplateId;
    public Define.ESound SoundType;
    public List<AudioClip> Clips;

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
