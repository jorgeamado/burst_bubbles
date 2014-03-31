using System.Net;

namespace NetworkPeer
{
	public class NetworkConfiguration
	{
		public int MaxConnections;
		public IPAddress LocalAddress;
		public int Port;

		public NetworkConfiguration(string ip, int port, int maxConnnections)
		{
			LocalAddress = string.IsNullOrEmpty(ip) ? IPAddress.Any : IPAddress.Parse(ip);
			Port = port;
			this.MaxConnections = maxConnnections;
		}
	}
}
