using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DynServer
{
	class Program
	{
		public static string prompt = " >> ";
		public const int BufferSize = 10025;

		public static Dictionary<string, TcpClient> clientsList = new Dictionary<string, TcpClient>();

		static void Main(string[] args)
		{
			if (ConfigurationManager.AppSettings["prompt"] != null)
				prompt = ConfigurationManager.AppSettings["prompt"];

			int port = 8888;
			int.TryParse(ConfigurationManager.AppSettings["port"], out port);
			TcpListener serverSocket = new TcpListener(IPAddress.Any, port);

			serverSocket.Start();
			Console.WriteLine(prompt + "Server Started on port " + ((IPEndPoint)serverSocket.LocalEndpoint).Port);
			Thread threadNewClient = new Thread(new ParameterizedThreadStart(AcceptNewClient));
			threadNewClient.Start(serverSocket);

			bool exit = false;
			while (!exit)
			{
				string line = Console.ReadLine();
				if (line.StartsWith("exit") || line.StartsWith("quit"))
					exit = true;
			}

			foreach (var client in clientsList)
			{
				client.Value.GetStream().Close();
				client.Value.Close();
			}
			serverSocket.Stop();
			Console.WriteLine(prompt + "exited.  Press a key to close the window.");
			Console.ReadKey();
		}

		private static void AcceptNewClient(object serverSocket) { AcceptNewClient(serverSocket as TcpListener); }

		private static void AcceptNewClient(TcpListener serverSocket)
		{
			try
			{
				while (true)
				{
					TcpClient clientSocket = serverSocket.AcceptTcpClient();
					byte[] bytesFrom = new byte[BufferSize];
					string dataFromClient = null;

					NetworkStream networkStream = clientSocket.GetStream();
					networkStream.Read(bytesFrom, 0, bytesFrom.Length);
					dataFromClient = Encoding.Unicode.GetString(bytesFrom);
					if (dataFromClient.IndexOf("$") == -1) continue;
					dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));

					clientsList.Add(dataFromClient, clientSocket);

					bytesFrom = Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(clientsList.Keys));
					networkStream.Write(bytesFrom, 0, bytesFrom.Length);

					Broadcast(dataFromClient + " joined.", dataFromClient, false);

					HandleClient client = new HandleClient();
					client.startClient(clientSocket, dataFromClient, clientsList);
				}
			}
			catch (Exception ex) when (ex is IOException || ex is SocketException)
			{
				SocketException socketEx = ex as SocketException;
				if (ex is IOException)
					socketEx = ex.InnerException as SocketException;

				if (socketEx.SocketErrorCode == SocketError.Interrupted)
					return;

				if (socketEx.SocketErrorCode == SocketError.ConnectionAborted)
				{
					Console.WriteLine(prompt + "ConnectionAborted NewClient");
					return;
				}
				throw;
			}
		}

		public static void Broadcast(string message, string username, bool flag)
		{
			if (flag)
				Console.WriteLine(prompt + username + " says : " + message);
			else
				Console.WriteLine(prompt + message);

			foreach (var client in clientsList)
			{
				try
				{
					TcpClient broadcastSocket = client.Value;
					if (!broadcastSocket.Connected) continue;
					NetworkStream broadcastStream = broadcastSocket.GetStream();
					byte[] broadcastBytes = null;

					if (flag)
					{
						broadcastBytes = Encoding.Unicode.GetBytes(username + " says : " + message);
					}
					else
					{
						broadcastBytes = Encoding.Unicode.GetBytes(message);
					}

					broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
					broadcastStream.Flush();
				}
				catch (SocketException ex)
				{
					if (ex.SocketErrorCode == SocketError.ConnectionAborted)
						Console.WriteLine(prompt + "ConnectionAborted with " + client.Key);
					throw;
				}
			}
		}
	}
}
