using TMPro;
using UnityEngine;

public class ThemeCell : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI Text;

	public void Initialize(string theme)
	{
		Text.text = theme;
	}
}
