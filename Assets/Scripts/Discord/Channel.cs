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

		public HttpStatusCode AddMessage(string message)
		{
			var response = POST($"{id}/messages", new Message {content = message});
			return response.StatusCode;
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

