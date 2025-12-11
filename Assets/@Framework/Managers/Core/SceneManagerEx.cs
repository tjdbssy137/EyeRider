using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx
{
	public BaseScene CurrentScene { get { return UnityEngine.Object.FindFirstObjectByType<BaseScene>(); } }
	public Action<Define.EScene> OnSceneChanged = null;
	private Define.EScene _nextScene;
	public Define.EScene NextScene => _nextScene;

	private string _label = "";
	public string Label => _label;
	private List<string> _labels = new List<string>();
	public IReadOnlyList<string> Labels => _labels;
	public void LoadScene(Define.EScene type)
	{
		Managers.Clear();
        Contexts.Reset();
        OnSceneChanged?.Invoke(type);
        SceneManager.LoadScene(GetSceneName(type));
    }

	public void LoadSceneWithProgress(Define.EScene type, string label = "")
	{
		Managers.Clear();
		_nextScene = type;
		_label = label;
		//SceneManager.LoadScene(GetSceneName(Define.EScene.LoadingPageTimelineScene));
    }
	public void LoadSceneWithProgress(Define.EScene type, List<string> labels)
	{
		Managers.Clear();
		_nextScene = type;
		_labels = labels;
		//SceneManager.LoadScene(GetSceneName(Define.EScene.LoadingPageTimelineScene));
    }

	private string GetSceneName(Define.EScene type)
	{
		string name = System.Enum.GetName(typeof(Define.EScene), type);
		return name;
	}

	public void Clear()
	{
		CurrentScene.Clear();
	}
}
