using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Theme : ScriptableObject
{
	public String Name;
	
	public List<Question> Questions;

	public String Caption;
	
	public int Multiplier { get; set; }

	private void OnEnable()
	{
		var score = 100;
		foreach (var question in Questions)
		{
			question.Cost = score * Multiplier;
			score += 100;
		}
	}
}
