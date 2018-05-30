using System.Net;
using UnityEngine;

public class CertificateFixer : MonoBehaviour {

	void Awake()
	{
		ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
	}
}
