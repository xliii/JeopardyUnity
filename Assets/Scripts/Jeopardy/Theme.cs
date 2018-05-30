using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Theme : ScriptableObject
{
	public String Name;
	
	public List<Question> Questions;

	public String Caption;
}
