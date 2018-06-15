using System.Timers;
using UnityEngine;

public abstract class AbstractHeartbeatService : IHeartbeatService {

	private AbstractGatewayClient gateway;    
	private Timer timer;
	private bool acknowledged = true;

	protected abstract GatewayOpCode OpCode { get; }

	protected abstract object Data { get; }
	
	protected AbstractHeartbeatService(AbstractGatewayClient gateway, int interval)
	{
		this.gateway = gateway;
		
        
		timer = new Timer(interval);
		timer.Elapsed += (sender, args) => SendHeartbeat();		
	}
	
	public void Start()
	{        
		timer.Start();        
		SendHeartbeat();
	}

	protected void Acknowledge()
	{
		//Debug.Log($"{gateway.Name}: Heartbeat ACK");
		acknowledged = true;
	}		
	
	private void SendHeartbeat()
	{
		//Debug.Log($"{gateway.Name}: Heartbeat");
		if (!acknowledged)
		{
			Debug.LogError($"{gateway.Name}: Previous heartbeat wasn't acknowledged");
		}
		var heartbeat = new GatewayPayload
		{
			OpCode = OpCode,
			Data = Data	
		};
		
		acknowledged = false;
		gateway.Send(heartbeat);
	}

	public void Dispose()
	{
		timer.Stop();
	}
}
