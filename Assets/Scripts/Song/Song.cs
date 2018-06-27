using System;
using System.Collections;
using System.IO;
using UnityEngine;

public abstract class Song : Question
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

	public string Filepath => $"{UnityUtils.ApplicationDataPath}/Music/{FullName}.wav";

	public string FullName => string.IsNullOrEmpty(Artist) ? Name : $"{Artist} - {Name}";

	protected abstract void Resolve();
}
