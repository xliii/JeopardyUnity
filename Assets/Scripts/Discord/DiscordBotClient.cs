using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using UnityEngine;

public class DiscordBotClient : MonoBehaviour
{
	private string API = "https://discordapp.com/api/";

	public DiscordBotConfig config;

	void Start ()
	{
		var request = WebRequest.Create($"{API}channels/{config.channelID}/messages");
		request.Method = "POST";
		request.Headers.Add("Authorization", $"Bot {config.token}");
		request.ContentType = "application/json";
		
		var message = JsonUtility.ToJson(new Message
		{
			content = "Test"
		});
		var data = Encoding.UTF8.GetBytes(message);
		request.ContentLength = data.Length;

		var stream = request.GetRequestStream();
		stream.Write(data, 0, data.Length);
		stream.Close();

		var response = request.GetResponse();
		Debug.Log(response);
	}

	class Message
	{
		public string content;
	}
}
