using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundPanel : MonoBehaviour
{
	public Round Round;

	public QuestionCell QuestionPrefab;
	public ThemeCell ThemePrefab;
	
	// Use this for initialization
	void Start () {
		Clear();
		SetRound(Round);
	}

	void SetRound(Round round)
	{
		foreach (var theme in round.Themes)
		{
			var themeCell = Instantiate(ThemePrefab, transform);
			themeCell.Initialize(theme.Name);

			foreach (var question in theme.Questions)
			{
				var questionCell = Instantiate(QuestionPrefab, transform);
				questionCell.Initialize(question);
			}
		}
	}

	void Clear()
	{
		for (var i = transform.childCount - 1; i >= 0; i--)
		{
			Destroy(transform.GetChild(i).gameObject);
		}
	}
}
