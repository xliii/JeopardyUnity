using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelloEventData : IGatewayEventData
{

	public int heartbeat_interval;
	public List<string> _trace;
}
