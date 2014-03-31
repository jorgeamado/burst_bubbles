using System;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.IO;

namespace NetworkPeer
{
	public partial class Peer
	{
		enum EPeerState
		{
			None,
			Active,
		}

		EPeerState peerState;
		Socket socket;
		Thread networkThread;
		NetworkConfiguration configuration;
		public IPEndPoint LocalEndPoint
		{ get { return socket != null ? (IPEndPoint)socket.LocalEndPoint : null;}}

		public Peer(NetworkConfiguration config)
		{
			configuration = config;
			InternalInitialization();
		}

		public void Start()
		{
			InitializeNetwork();

			networkThread = new Thread(new ThreadStart(NetworkLoop));
			networkThread.IsBackground = true;
			peerState = EPeerState.Active;
			networkThread.Start();
			Logger.Log(string.Format("peer - started {0}", socket.LocalEndPoint));
		}

		public void HearBeat()
		{
			ReadMessages();
		}

		public void Stop()
		{
			if (peerState == EPeerState.Active)
			{
				peerState = EPeerState.None;
				DisconnectAll();
				OnNewConnection = null;
				if (null != socket)
					socket.Close();
				Logger.Log("peer - Stoped");
			}
		}

		void InitializeNetwork()
		{
			var ep = new IPEndPoint(configuration.LocalAddress, configuration.Port);
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

			socket.Bind(ep);
		}

		public Connection Connect(IPAddress ip, int port)
		{
			IPEndPoint ep = new IPEndPoint(ip, port);
			Connection connection = null;
			if (!connectedPeers.ContainsKey(ep))
			{
				connection = new Connection(this, ep, TIME_OUT);
				lock (connectedPeers)
				{
					connectedPeers.Add(ep, connection);
				}
			}
			else
			{
				connection = connectedPeers[ep];
			}
			connection.Connect();
			return connection;
		}
	}
}
