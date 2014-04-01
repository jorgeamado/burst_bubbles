using System;
using System.Net;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Linq;

namespace NetworkPeer
{
	public partial class Peer
	{
		public System.Action<Connection> OnNewConnection;

		Dictionary<IPEndPoint, Connection> connectedPeers;
		Queue<KeyValuePair<IPEndPoint, byte[]>> messageQueue;
		int maxConnections = int.MaxValue;

		EndPoint remoteSender;
		internal byte[] receiveBuffer;
		const int TIME_OUT = 15; //seconds
		DateTime lastPing;

		public int ConnectedPeersCount
		{
			get { return connectedPeers.Count; }
		}

		void InternalInitialization()
		{
			remoteSender = (EndPoint)new IPEndPoint(IPAddress.Any, 0);
			connectedPeers = new Dictionary<IPEndPoint, Connection>();
			messageQueue = new Queue<KeyValuePair<IPEndPoint, byte[]>>();
			receiveBuffer = new byte[512];
			maxConnections = configuration.MaxConnections;
		}

		void ReadMessages()
		{
			lock (messageQueue)
			{
				if (messageQueue.Count > 0)
				{
					var msg = messageQueue.Dequeue();
					HandleMessage(msg.Key, msg.Value);
				}
			}
		}

		void NetworkLoop()
		{
			try
			{
				while (peerState == EPeerState.Active)
				{
					RemoveDisconnectedPeers();
					PingAllConnections();

					if (socket.Poll(1000, SelectMode.SelectRead)) // wait up to 1 ms for data to arrive
					{
						socket.ReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref remoteSender);
						IPEndPoint ipsender = (IPEndPoint)remoteSender;
						lock (messageQueue) 
						{
							messageQueue.Enqueue(new KeyValuePair<IPEndPoint, byte[]>(ipsender, (byte[])receiveBuffer.Clone()));
							Array.Clear(receiveBuffer, 0, receiveBuffer.Length);
						}

					}
				}
			}
			catch (Exception ex)
			{
				Logger.LogError(ex.Message);
			}

		}

		protected void HandleMessage(IPEndPoint ipsender, byte[] receivedBytes)
		{
			Connection currentConnection = null;
			if (!connectedPeers.ContainsKey(ipsender))
			{
				Logger.Log(string.Format("New peer connected: {0}", ipsender.ToString()));

				lock (connectedPeers)
				{
					currentConnection = new Connection(this, ipsender, TIME_OUT);
					Logger.Log("maxConnections " + maxConnections);
					if (connectedPeers.Count < maxConnections)
					{
						Logger.Log("Add new connection");
						connectedPeers.Add(ipsender, currentConnection);
					}
					else
					{
						Logger.Log("no free space");
						currentConnection.Disconnect("No free connection slots");
						return;
					}
				}
			}
			else
			{
				currentConnection = connectedPeers[ipsender];
			}
			var previousStatus = currentConnection.Status;
			currentConnection.HandleIncomingMessage(receivedBytes);

			if (null != OnNewConnection 
				&& (previousStatus == Connection.EConnectionStatus.Connecting || previousStatus == 	Connection.EConnectionStatus.None)
				&& currentConnection.Status == Connection.EConnectionStatus.Connected)
			{
				OnNewConnection(currentConnection);
			}
		}

		void RemoveDisconnectedPeers()
		{
			lock (connectedPeers)
			{
				var disconnectedConnections = connectedPeers.Where(connectedPeer => 
					connectedPeer.Value.Status == Connection.EConnectionStatus.Disconnected
					|| connectedPeer.Value.Status == Connection.EConnectionStatus.TimedOut).ToList();

				foreach (var connectedPeer in disconnectedConnections)
				{
					Logger.Log(string.Format("{0} - REMOVED", connectedPeer.Value));
					connectedPeer.Value.Disconnect("Timed out");
					connectedPeers.Remove(connectedPeer.Key);
				}
			}
		}

		void PingAllConnections()
		{
			foreach (var connection in connectedPeers)
			{
				connection.Value.Ping();
			}
		}

		void DisconnectAll()
		{
			foreach (var connection in connectedPeers)
			{
				connection.Value.Disconnect("disconnect all");
			}
			RemoveDisconnectedPeers();
		}
	}
}
