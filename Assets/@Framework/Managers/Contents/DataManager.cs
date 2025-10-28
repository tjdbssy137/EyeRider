using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

public interface ILoader<Key, Value>
{
	Dictionary<Key, Value> MakeDict();
}

public class DataManager
{
    public Dictionary<int, Data.LocalizationData> LocalizationDic { get; private set; } = new Dictionary<int, Data.LocalizationData>();

    public void Init()
    {
        LocalizationDic = LoadJson<Data.LocalizationDataLoader, int, Data.LocalizationData>("LocalizationData").MakeDict();
        // TestDic = LoadJson<Data.TestDataLoader, int, Data.TestData>("TestData").MakeDict();
    }

	private Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
	{
		TextAsset textAsset = Managers.Resource.Load<TextAsset>(path);
		return JsonConvert.DeserializeObject<Loader>(textAsset.text);
	}

    public static Dictionary<int, T> LoadJsonDirect<T>(string path) where T : Data.IData
    {
        TextAsset textAsset = Managers.Resource.Load<TextAsset>(path);

        List<T> dataList = JsonConvert.DeserializeObject<List<T>>(textAsset.text);
        Dictionary<int, T> result = new Dictionary<int, T>();

        foreach (var data in dataList)
        {
            result[data.Id] = data;
        }

        return result;
    }
}
