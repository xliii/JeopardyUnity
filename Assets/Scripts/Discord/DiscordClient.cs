using System.Net;

namespace Discord
{
    public abstract class DiscordClient
    {
        public abstract HttpWebRequest AddAuthorization(HttpWebRequest request);

        public Channel Channel(string id)
        {
            return new Channel(this, id);
        }

        public Gateway Gateway()
        {
            return new Gateway(this);
        }
    }
}


