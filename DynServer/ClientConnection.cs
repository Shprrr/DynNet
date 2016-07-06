using System;
using System.Net;
using System.Net.Sockets;

namespace DynServer
{
	public class ClientConnection : ClientConnectionBase
	{
		private DynNetProtocol Protocol { get; set; }
		public string Username { get; set; }

		public ClientConnection(TcpClient socket) : base(socket, Program.BufferSize)
		{
			Protocol = new DynNetProtocol();
			Protocol.Connecting += Protocol_Connecting;

			ReceivingMessage += ClientConnection_ReceivingConnectMessage;
		}

		private void ClientConnection_ReceivingConnectMessage(object sender, string message)
		{
			try
			{
				if (!Protocol.ReceiveMessageConnect(message)) throw new ProtocolViolationException("Wrong protocol.");
			}
			catch (ProtocolViolationException ex)
			{
				Send("Disconnecting.  " + ex.Message);
				Dispose();
			}
		}

		private void Protocol_Connecting(object sender, string username)
		{
			Username = username;
			ReceivingMessage -= ClientConnection_ReceivingConnectMessage;

			ReceivingMessage += ClientConnection_ReceivingMessage;
			Disconnecting += ClientConnection_Disconnecting;

			Program.Broadcast(Username + " joined.", Username, false);
			Program.PendingClientsList.Remove(Socket);
			Program.ClientsList.Add(Username, this);
			Program.DebugWriteLine("Client accepted. Remaining clients pending:" + Program.PendingClientsList.Count + " online:" + Program.ClientsList.Count);
		}

		private void ClientConnection_ReceivingMessage(object sender, string message)
		{
			if (!string.IsNullOrWhiteSpace(message))
				Program.Broadcast(message, Username, true);
		}

		private void ClientConnection_Disconnecting(object sender, EventArgs e)
		{
			Program.Broadcast(Username + " has been disconnected.", Username, false);
		}

		#region IDisposable Support
		protected override void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// Supprimer l'état managé (objets managés).
					Program.PendingClientsList.Remove(Socket);
					if (Username != null)
						Program.ClientsList.Remove(Username);
					Program.DebugWriteLine("Client disposed. Remaining clients pending:" + Program.PendingClientsList.Count + " online:" + Program.ClientsList.Count);
				}

				// Libérer les ressources non managées (objets non managés) et remplacer un finaliseur ci-dessous.
				// Définir les champs de grande taille avec la valeur Null.
			}
			base.Dispose(disposing);
		}
		#endregion
	}
}
