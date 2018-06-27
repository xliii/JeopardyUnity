using UnityEngine;

public class UnityUtils : MonoBehaviour
{

	public static string ApplicationDataPath;

	// Use this for initialization
	private void Awake()
	{
		ApplicationDataPath = Application.dataPath;
		gameObject.hideFlags = HideFlags.HideInHierarchy;
	}

}
