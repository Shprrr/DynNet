using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace DynServer
{
	class Program
	{
		public static string prompt = " >> ";
		public const int BufferSize = 10025;

		public static Dictionary<string, ClientConnection> ClientsList = new Dictionary<string, ClientConnection>();

		static void Main(string[] args)
		{
			if (ConfigurationManager.AppSettings["prompt"] != null)
				prompt = ConfigurationManager.AppSettings["prompt"];

			int port = 8888;
			int.TryParse(ConfigurationManager.AppSettings["port"], out port);

			ServerConnection server = new ServerConnection(port, 5);
			Console.WriteLine(prompt + "Server Started on port " + port);

			bool exit = false;
			while (!exit)
			{
				string line = Console.ReadLine();
				if (line.StartsWith("exit") || line.StartsWith("quit"))
					exit = true;
			}

			for (int i = ClientsList.Count - 1; i >= 0; i--)
			{
				ClientsList.ElementAt(i).Value.Dispose();
			}
			server.Dispose();
			Console.WriteLine(prompt + "exited.  Press a key to close the window.");
			Console.ReadKey();
		}

		public static void Broadcast(string message, string username, bool flag)
		{
			if (flag)
				message = username + " says : " + message;

			Console.WriteLine(prompt + message);

			foreach (var client in ClientsList)
			{
				client.Value.Send(message);
			}
		}
	}
}
