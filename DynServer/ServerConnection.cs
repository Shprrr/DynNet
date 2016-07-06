using System;
using System.Net;
using System.Net.Sockets;

namespace DynServer
{
	/// <summary>
	/// Accept multiple client connections on a known port.
	/// </summary>
	public class ServerConnection : IDisposable
	{
		private TcpListener _ServerSocket;
		private IAsyncResult _AcceptClient;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="port"></param>
		/// <param name="maxPendingConnection">Usually between 2 and 5.</param>
		public ServerConnection(int port, int maxPendingConnection)
		{
			_ServerSocket = new TcpListener(IPAddress.Any, port);

			_ServerSocket.Start(maxPendingConnection);
			_AcceptClient = _ServerSocket.BeginAcceptTcpClient(AcceptClient, this);
		}

		private static void AcceptClient(IAsyncResult ar)
		{
			ServerConnection server = (ServerConnection)ar.AsyncState;
			TcpClient clientSocket;
			try
			{
				clientSocket = server._ServerSocket.EndAcceptTcpClient(ar);
			}
			catch (ObjectDisposedException)
			{
				// Object already gone.
				return;
			}
			server._AcceptClient = server._ServerSocket.BeginAcceptTcpClient(AcceptClient, server);

			Program.PendingClientsList.Add(clientSocket, new ClientConnection(clientSocket));
			Program.DebugWriteLine("New Client. Remaining clients pending:" + Program.PendingClientsList.Count + " online:" + Program.ClientsList.Count);
		}

		#region IDisposable Support
		private bool disposedValue = false; // Pour détecter les appels redondants

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// Supprimer l'état managé (objets managés).
					if (_ServerSocket != null)
						_ServerSocket.Stop();
				}

				// Libérer les ressources non managées (objets non managés) et remplacer un finaliseur ci-dessous.
				// Définir les champs de grande taille avec la valeur null.

				disposedValue = true;
			}
		}

		// Remplacer un finaliseur seulement si la fonction Dispose(bool disposing) ci-dessus a du code pour libérer les ressources non managées.
		// ~ServerConnection() {
		//   // Ne modifiez pas ce code. Placez le code de nettoyage dans Dispose(bool disposing) ci-dessus.
		//   Dispose(false);
		// }

		// Ce code est ajouté pour implémenter correctement le modèle supprimable.
		public void Dispose()
		{
			// Ne modifiez pas ce code. Placez le code de nettoyage dans Dispose(bool disposing) ci-dessus.
			Dispose(true);
			// Supprimer les marques de commentaire pour la ligne suivante si le finaliseur est remplacé ci-dessus.
			// GC.SuppressFinalize(this);
		}
		#endregion
	}
}
