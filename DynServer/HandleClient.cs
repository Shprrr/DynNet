using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DynServer
{
	/// <summary>
	/// Class to handle each client request separatly
	/// </summary>
	public class HandleClient
	{
		TcpClient clientSocket;
		public Dictionary<string, TcpClient> ClientsList { get; set; }

		public string Username { get; set; }

		public void startClient(TcpClient inClientSocket, string username, Dictionary<string, TcpClient> clientsList)
		{
			clientSocket = inClientSocket;
			Username = username;
			ClientsList = clientsList;
			Thread ctThread = new Thread(DoChat);
			ctThread.Start();
		}

		private void DoChat()
		{
			byte[] bytesFrom = new byte[Program.BufferSize];

			while (clientSocket.Connected)
			{
				try
				{
					Array.Clear(bytesFrom, 0, bytesFrom.Length);
					NetworkStream networkStream = clientSocket.GetStream();

					networkStream.Read(bytesFrom, 0, bytesFrom.Length);
					string dataFromClient = Encoding.Unicode.GetString(bytesFrom);
					if (dataFromClient.IndexOf("$") == -1) continue;
					dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));

					Program.Broadcast(dataFromClient, Username, true);
				}
				catch (Exception ex)
				{
					var ioEx = ex as IOException;
					var socketEx = ex as SocketException;
					if (ioEx != null && ioEx.InnerException is SocketException)
						socketEx = (SocketException)ioEx.InnerException;

					if (socketEx != null)
						switch (socketEx.SocketErrorCode)
						{
							case SocketError.ConnectionAborted:
							case SocketError.ConnectionReset:
							case SocketError.Interrupted:
								if (clientSocket.Connected) clientSocket.GetStream().Close();
								clientSocket.Dispose();
								continue;
						}

					Console.WriteLine(Program.prompt + ex.ToString());
				}
			}
			Program.Broadcast(Username + " has been disconnected.", Username, false);
			ClientsList.Remove(Username);
			if (clientSocket.Connected) clientSocket.GetStream().Close();
			clientSocket.Close();
		}
	}
}
