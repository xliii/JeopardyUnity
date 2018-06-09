using TMPro;
using UnityEngine;

public class QuestionCell : MonoBehaviour {

	[SerializeField]
	private TextMeshProUGUI Text;

	private Question question;

	public void Initialize(Question question)
	{
		this.question = question;
		Text.text = $"${question.Cost}";
	}

	public void OnClick()
	{
		Debug.Log("Click: " + question.name);
		if (question is MusicalQuestion)
		{
			var song = (question as MusicalQuestion).Song;
		 	var player = FindObjectOfType<SongPlayer>();
			player.song = song;
			player.Play();
		}
		
	}
}
