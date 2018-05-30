using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SongPlayer : MonoBehaviour
{	
	public Song song;
	public float fadeDuration;

	private AudioSource audioSource;
	private string path;

	void Awake()
	{
		audioSource = GetComponent<AudioSource>();
	}

	public void Play()
	{
		Debug.Log($"Playing {song.Name}");
		StartCoroutine(song.GetClip(Play));
	}

	IEnumerator PlayCoroutine(AudioClip clip)
	{
		audioSource.clip = clip;
		audioSource.time = song.StartTime;
		audioSource.volume = 0;
		audioSource.Play();
		audioSource.SetScheduledEndTime(AudioSettings.dspTime + song.EndTime - song.StartTime);
		StartCoroutine(FadeIn());
		yield return new WaitForSeconds(song.EndTime - song.StartTime - fadeDuration);
		StartCoroutine(FadeOut());
	}

	void Play(AudioClip clip)
	{
		StartCoroutine(PlayCoroutine(clip));
	}

	IEnumerator FadeIn()
	{
		for (float t = 0f; t < 1f; t += Time.deltaTime / fadeDuration)
		{
			audioSource.volume = t;
			yield return null;
		}

		audioSource.volume = 1;
	}

	IEnumerator FadeOut()
	{
		for (float t = 1f; t > 0f; t -= Time.deltaTime / fadeDuration)
		{
			audioSource.volume = t;
			yield return null;
		}

		audioSource.volume = 0;
	}
}
