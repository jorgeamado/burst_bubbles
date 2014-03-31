using System.Net;

namespace NetworkPeer
{
	public partial class Peer
	{
		public void SendMessage(byte[] msg, IPEndPoint ep)
		{
			socket.SendTo(msg, ep);
		}

		public void SendMessageToAll(params string[] msg)
		{
			foreach (var conn in connectedPeers)
			{
				conn.Value.SendMessage(msg);
			}
		}
	}
}
