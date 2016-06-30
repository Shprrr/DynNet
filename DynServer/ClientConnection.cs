using System;
using System.Net.Sockets;

namespace DynServer
{
	public class ClientConnection : ClientConnectionBase
	{
		public string Username { get; set; }

		public ClientConnection(TcpClient socket, string username) : base(socket, Program.BufferSize)
		{
			Username = username;
			ReceivingMessage += ClientConnection_ReceivingMessage;
			Disconnecting += ClientConnection_Disconnecting;
		}

		private void ClientConnection_ReceivingMessage(object sender, string message)
		{
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
					// TODO: supprimer l'état managé (objets managés).
					Program.ClientsList.Remove(Username);
				}

				// TODO: libérer les ressources non managées (objets non managés) et remplacer un finaliseur ci-dessous.
				// TODO: définir les champs de grande taille avec la valeur Null.
			}
			base.Dispose(disposing);
		}
		#endregion
	}
}
