using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DynServer
{
	public class ClientConnectionBase : IDisposable
	{
		/// <summary>
		/// Time in milisecond before checking if there is a half-open connection.
		/// </summary>
		public const int TIMEOUT = 30000;

		public TcpClient Socket { get; set; }
		public int BufferSize { get; protected set; }
		private Thread _ThreadReceive;
		private Thread _ThreadKeepAlive;
		private DateTime _LastAlive = DateTime.Now;

		public ClientConnectionBase(TcpClient socket, int bufferSize)
		{
			Socket = socket;
			BufferSize = bufferSize;
			_ThreadReceive = new Thread(Receiving);
			_ThreadReceive.Start();
			_ThreadKeepAlive = new Thread(KeepAlive);
			_ThreadKeepAlive.Start();
		}

		private void Receiving()
		{
			PacketProtocol protocol = new PacketProtocol(BufferSize);
			protocol.MessageArrived = Receive;
			// Wait until the event ReceivingMessage is assigned before receving anything.
			while (ReceivingMessage == null)
			{
				Thread.Sleep(0);
			}
			while (Socket.Connected && ReceivingMessage != null)
			{
				if (!protocol.DataReceived(Socket.GetStream())) break;
			}

			OnDisconnecting();
			Dispose();
		}

		public event EventHandler Disconnecting;
		protected virtual void OnDisconnecting()
		{
			Disconnecting?.Invoke(this, EventArgs.Empty);
		}

		private void Receive(byte[] data)
		{
			string message = Encoding.UTF8.GetString(data);
			OnReceivingMessage(message);
		}

		public event EventHandler<string> ReceivingMessage;
		protected virtual void OnReceivingMessage(string message)
		{
			ReceivingMessage?.Invoke(this, message);
		}

		private void KeepAlive()
		{
			while (_ThreadReceive.IsAlive)
			{
				if ((DateTime.Now - _LastAlive).TotalMilliseconds >= TIMEOUT)
					Send(PacketProtocol.WrapKeepaliveMessage());

				Thread.Sleep(TIMEOUT);
			}
		}

		public void Send(string message)
		{
			if (!Socket.Connected) return;

			Send(PacketProtocol.WrapMessage(message));
		}

		public void Send(byte[] data)
		{
			if (!Socket.Connected) return;

			NetworkStream stream = Socket.GetStream();
			stream.Write(data, 0, data.Length);
			stream.Flush();
			_LastAlive = DateTime.Now;
		}

		#region IDisposable Support
		protected bool disposedValue = false; // Pour détecter les appels redondants

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// Supprimer l'état managé (objets managés).
					if (Socket.Connected) Socket.GetStream().Close();
					Socket.Close();
				}

				// Libérer les ressources non managées (objets non managés) et remplacer un finaliseur ci-dessous.
				// Définir les champs de grande taille avec la valeur Null.

				disposedValue = true;
			}
		}

		// Remplacer un finaliseur seulement si la fonction Dispose(bool disposing) ci-dessus a du code pour libérer les ressources non managées.
		// ~ClientConnection() {
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
