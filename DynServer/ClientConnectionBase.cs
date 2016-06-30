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
		public const int TIMEOUT = 1000;

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
			while (Socket.Connected)
			{
				protocol.DataReceived(Socket.GetStream());
			}

			OnDisconnecting();
			Dispose();
		}

		public event EventHandler Disconnecting;
		protected virtual void OnDisconnecting()
		{
			Disconnecting?.Invoke(this, EventArgs.Empty);
		}

		private void Receive(byte[] data, object param)
		{
			//_LastAlive = DateTime.Now;
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
				if ((DateTime.Now - _LastAlive).Milliseconds >= TIMEOUT)
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
					// TODO: supprimer l'état managé (objets managés).
					if (Socket.Connected) Socket.GetStream().Close();
					Socket.Close();
				}

				// TODO: libérer les ressources non managées (objets non managés) et remplacer un finaliseur ci-dessous.
				// TODO: définir les champs de grande taille avec la valeur Null.

				disposedValue = true;
			}
		}

		// TODO: remplacer un finaliseur seulement si la fonction Dispose(bool disposing) ci-dessus a du code pour libérer les ressources non managées.
		// ~ClientConnection() {
		//   // Ne modifiez pas ce code. Placez le code de nettoyage dans Dispose(bool disposing) ci-dessus.
		//   Dispose(false);
		// }

		// Ce code est ajouté pour implémenter correctement le modèle supprimable.
		public void Dispose()
		{
			// Ne modifiez pas ce code. Placez le code de nettoyage dans Dispose(bool disposing) ci-dessus.
			Dispose(true);
			// TODO: supprimer les marques de commentaire pour la ligne suivante si le finaliseur est remplacé ci-dessus.
			// GC.SuppressFinalize(this);
		}
		#endregion
	}
}
