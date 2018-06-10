using System.Collections;
using System.Collections.Generic;
using Discord;
using UnityEngine;

public class DiscordTest : MonoBehaviour
{

	public DiscordBotConfig config;

	// Use this for initialization
	void Start () {
		DiscordBotClient client = new DiscordBotClient(config);
		var response = client.Channel(config.channelID).AddMessage("Hello world");
		Debug.Log($"Message added: {response}");
	}
}
