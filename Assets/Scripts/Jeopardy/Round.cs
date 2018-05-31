using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Round : ScriptableObject {

	public int Multiplier { get; set; }

	public List<Theme> Themes;

	private void OnEnable()
	{
		Multiplier = 1;
		foreach (var theme in Themes)
		{
			theme.Multiplier = Multiplier;
		}
	}
}
