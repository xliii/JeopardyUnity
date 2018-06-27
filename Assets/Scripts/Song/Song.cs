using System;
using System.Collections;
using System.IO;
using UnityEngine;

public abstract class Song : ScriptableObject
{
	public string Artist;
	public string Name;
	public int StartTime;
	public int EndTime;

	public IEnumerator GetClip(Action<AudioClip> callback)
	{
		if (!File.Exists(Filepath))
		{
			Resolve();
		}
		
		WWW www = new WWW(Filepath);
		yield return www;

		callback(www.GetAudioClip(false, false));
	}

	//TODO: Expose Application.dataPath globally
	public string Filepath => $"E:/Dev/JeopardyUnity/Assets/Music/{FullName}.wav";

	public string FullName => string.IsNullOrEmpty(Artist) ? Name : $"{Artist} - {Name}";

	protected abstract void Resolve();
}
