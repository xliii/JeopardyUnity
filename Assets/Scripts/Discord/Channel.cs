using System.Net;

namespace Discord
{
	public class Channel : AbstractDiscordApi
	{
		
		private string id;

		public Channel(DiscordClient client, string id) : base(client)
		{
			this.id = id;
		}

		public MessageCreateEventData AddMessage(string message)
		{
			return POST<MessageCreateEventData>($"{id}/messages", new Message {content = message});			
		}

		protected override string Path()
		{
			return "channels";
		}
	}

	class Message
	{
		public string content;
	}
}

