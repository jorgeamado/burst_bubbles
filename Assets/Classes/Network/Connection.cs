using System.Net;
using System.Text;
using System;

namespace NetworkPeer
{
	public class Connection
	{
		public const string MESSAGE_SEPARATOR = "%";
		public const string END_OF_MESSAGE = "^";

		public enum EConnectionStatus
		{
			None,
			Connecting,
			Connected,
			Disconnected,
			TimedOut,
		}

		public enum ETechnicalMessages
		{
			None,
			InternalMessage,
			Connect,
			Approved,
			Abort,
			Ping,
			Pong,
		}

		IPEndPoint endPoint;
		public IPEndPoint EndPoint
		{get { return endPoint;}}

		private EConnectionStatus status = EConnectionStatus.None;
		public EConnectionStatus Status
		{
			get 
			{ 
				if ((DateTime.Now - lastMessageTimeStamp).Seconds > timeOut)
					status = EConnectionStatus.TimedOut;
			
				return status; 
			}
			protected set 
			{ 
				Logger.Log(this.ToString() + "status changed to " + value);
				this.status = value; 
			}
		}

		Peer peer;
		public System.Action<string> OnMessageReceived;
		public System.Action OnDisconnected;
		DateTime lastMessageTimeStamp;
		int timeOut;

		DateTime lastPing;
		const int PING_FREQUENCY = 3; //seconds

		public Connection(Peer peer, IPEndPoint endPoint, int timeOut)
		{
			this.peer = peer;
			this.endPoint = endPoint;
			this.timeOut = timeOut;
			lastMessageTimeStamp = DateTime.Now;
		}

		public void SendMessage(params string[] msg)
		{
			var message = string.Join(MESSAGE_SEPARATOR, msg);
			Logger.Log("SEND:" + message);

			if (status == EConnectionStatus.Connected)
			{
				SendTechicalMessage(ETechnicalMessages.InternalMessage, message);
			}
			else
			{
				Logger.LogWarning("Attemption to send message to " + this.ToString());
			}
		}

		private void SendTechicalMessage(ETechnicalMessages type, string message = "")
		{
			var fullMessage = type.ToString();
			if (!String.IsNullOrEmpty(message))
				fullMessage = fullMessage + MESSAGE_SEPARATOR + message;
			fullMessage += END_OF_MESSAGE;
			var bytes = Encoding.UTF8.GetBytes(fullMessage);
			Logger.Log(this.ToString() + "Message send '--" + fullMessage + "--'");

			SendBytes(bytes);
		}

		private void SendBytes(byte[] bytes)
		{
			if (bytes.Length > 512)
			{
				Logger.LogWarning("!!!! msg length is more than 512");
			}
			peer.SendMessage(bytes, endPoint);
		}

		public void HandleIncomingMessage(byte[] bytesReceived)
		{
			var msg = Encoding.UTF8.GetString(bytesReceived);
			msg = msg.Remove(msg.IndexOf(END_OF_MESSAGE));
			Logger.Log(this.ToString() + "Message received '--" + msg + "--'");
			var blocks = msg.Split(MESSAGE_SEPARATOR.ToCharArray(), 2);
			//block[0] - technical message
			//block[1] - other message

			var technicalMessage = Helper.StringToEnum<ETechnicalMessages>(blocks[0], ETechnicalMessages.None);
			Logger.Log("technicalMessage: '" + technicalMessage.ToString()+ "'");

			if (status != EConnectionStatus.Disconnected)
			{
				switch (technicalMessage)
				{
					case ETechnicalMessages.Abort:
						{
							Status = EConnectionStatus.Disconnected;
							if(null != OnDisconnected)
								OnDisconnected();
							Logger.Log("Disconnected: " + (blocks.Length == 2 ? blocks[1] : "No explanation"));
							break;
						}
					case ETechnicalMessages.Ping:
						{
							lastMessageTimeStamp = DateTime.Now;
							Pong();
							break;
						}
					case ETechnicalMessages.Pong:
						{
							if (status != EConnectionStatus.TimedOut)
							{
								Logger.LogWarning(string.Format("status = {0}, but connection received Pong", status));
								lastMessageTimeStamp = DateTime.Now;
								Status = EConnectionStatus.Connected;
							}
							break;
						}
					case ETechnicalMessages.Connect:
						{
							lastMessageTimeStamp = DateTime.Now;
							Status = EConnectionStatus.Connected;
							SendTechicalMessage(ETechnicalMessages.Approved);
							break;
						}
					case ETechnicalMessages.Approved:
						{
							lastMessageTimeStamp = DateTime.Now;
							Status = EConnectionStatus.Connected;
							break;
						}
					case ETechnicalMessages.InternalMessage:
						{
							lastMessageTimeStamp = DateTime.Now;
							Status = EConnectionStatus.Connected;
							if(blocks.Length == 2 && null != OnMessageReceived)
								OnMessageReceived(blocks[1]);
							break;
						}
					default:
						{
							Logger.LogWarning("Unknown state: " + msg);
							break;
						}
				}
			}
		}

		public void Connect()
		{
			this.Status = EConnectionStatus.Connecting;
			SendTechicalMessage(ETechnicalMessages.Connect);
		}

		public void Ping()
		{
			if ((DateTime.Now - lastPing).Seconds > PING_FREQUENCY
			   && Status == EConnectionStatus.Connected)
			{
				SendTechicalMessage(ETechnicalMessages.Ping);
				lastPing = DateTime.Now;
			}
		}

		public void Pong()
		{
			SendTechicalMessage(ETechnicalMessages.Pong);
		}

		public void Disconnect(string message)
		{
			Logger.Log("Disconnect with message :" + message);
			SendTechicalMessage(ETechnicalMessages.Abort, message);
			Status = EConnectionStatus.Disconnected;
			if(null != OnDisconnected)
				OnDisconnected();
		}

		public override string ToString()
		{
			return string.Format("[Connection: EndPoint={0}, Status={1}]", EndPoint, Status);
		}
	}
}