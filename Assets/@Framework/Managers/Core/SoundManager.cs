using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager
{
	private AudioSource[] _audioSources = new AudioSource[(int)Define.ESound.Max];
	private Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();
	private GameObject _soundRoot = null;

	public void Init()
	{
		if (_soundRoot == null)
		{
			_soundRoot = GameObject.Find("@SoundRoot");

			if (_soundRoot == null)
			{
				_soundRoot = new GameObject { name = "@SoundRoot" };
				UnityEngine.Object.DontDestroyOnLoad(_soundRoot);

				string[] soundTypeNames = System.Enum.GetNames(typeof(Define.ESound));
				for (int count = 0; count < soundTypeNames.Length - 1; count++)
				{
					GameObject go = new GameObject { name = soundTypeNames[count] };
					_audioSources[count] = go.AddComponent<AudioSource>();
					go.transform.parent = _soundRoot.transform;
				}

				_audioSources[(int)Define.ESound.Bgm].loop = true;
			}
		}
	}

	public void Clear()
	{
		foreach (AudioSource audioSource in _audioSources)
			audioSource.Stop();

		_audioClips.Clear();
	}

	public void Play(Define.ESound type)
	{
		AudioSource audioSource = _audioSources[(int)type];
        audioSource.pitch = 1.0f;
        audioSource.Play();
	}

	public void Play(Define.ESound type, string key, float pitch = 1.0f)
	{
		AudioSource audioSource = _audioSources[(int)type];

		if (type == Define.ESound.Bgm)
		{

			LoadAudioClip(key, (audioClip) =>
			{
                if (audioSource.clip == audioClip && audioSource.isPlaying)
                {
                    return;
                }
                if (audioSource.isPlaying)
					audioSource.Stop();

				audioSource.clip = audioClip;
				audioSource.Play();
			});
		}
		else
		{
			LoadAudioClip(key, (audioClip) =>
			{
				audioSource.pitch = pitch;
				audioSource.PlayOneShot(audioClip);
			});
		}
	}

	public void Play(Define.ESound type, AudioClip audioClip, float pitch = 1.0f)
	{
		AudioSource audioSource = _audioSources[(int)type];

		if (type == Define.ESound.Bgm)
		{
            if (audioSource.clip == audioClip && audioSource.isPlaying)
			{
                return;
            }
            if (audioSource.isPlaying)
				audioSource.Stop();

			audioSource.clip = audioClip;
			audioSource.Play();
		}
		else
		{
			audioSource.pitch = pitch;
			audioSource.PlayOneShot(audioClip);
		}
	}

    public void Play(Define.ESound type, float minPitch, float maxPitch = 1.0f)
    {
        if (Managers.Data.SoundDic.TryGetValue(type, out var data) == false)
        {
            Debug.LogWarning($"[Sound] {type} 타입의 데이터가 설정되지 않았습니다.");
            return;
        }

        AudioSource audioSource = _audioSources[(int)type];

        if (data.Clips == null || data.Clips.Count == 0)
		{
            return;
        }
        AudioClip clip = data.Clips[UnityEngine.Random.Range(0, data.Clips.Count)];
        float pitch = UnityEngine.Random.Range(minPitch, maxPitch);
        if (type == Define.ESound.Bgm)
        {
            if (audioSource.clip == clip && audioSource.isPlaying)
            {
                return;
            }
            if (audioSource.isPlaying)
                audioSource.Stop();

            audioSource.pitch = pitch;
            audioSource.clip = clip;
            audioSource.Play();
        }
        else
        {
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(clip);
        }
    }

    public void Stop(Define.ESound type)
	{
		AudioSource audioSource = _audioSources[(int)type];
		audioSource.Stop();
	}

    public void Pause(Define.ESound type)
    {
        AudioSource audioSource = _audioSources[(int)type];
        audioSource.Pause();
    }

    public void Resume(Define.ESound type)
    {
        AudioSource audioSource = _audioSources[(int)type];
        audioSource.UnPause();
    }

    private void LoadAudioClip(string key, Action<AudioClip> callback)
	{
		AudioClip audioClip = null;
		if (_audioClips.TryGetValue(key, out audioClip))
		{
			callback?.Invoke(audioClip);
			return;
		}

		audioClip = Managers.Resource.Load<AudioClip>(key);

		if (_audioClips.ContainsKey(key) == false)
			_audioClips.Add(key, audioClip);

		callback?.Invoke(audioClip);
	}
}
