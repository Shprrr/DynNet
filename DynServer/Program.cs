﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;

namespace DynServer
{
	class Program
	{
		public static string PromptHeader = "[%T] >> ";
		public const int BufferSize = 10025;

		public static Dictionary<System.Net.Sockets.TcpClient, ClientConnection> PendingClientsList = new Dictionary<System.Net.Sockets.TcpClient, ClientConnection>();
		public static Dictionary<string, ClientConnection> ClientsList = new Dictionary<string, ClientConnection>();

		private static ServerConnection server = null;
		static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += AppDomain_UnhandledException;

			if (ConfigurationManager.AppSettings["prompt"] != null)
				PromptHeader = ConfigurationManager.AppSettings["prompt"];

			int port = 8888;
			int.TryParse(ConfigurationManager.AppSettings["port"], out port);

			server = new ServerConnection(port, 5);
			ConsoleWriteLine("Server Started on port " + port);

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

			if (server != null)
				server.Dispose();
			ConsoleWriteLine("Exited.  Press a key to close the window.");
			Console.ReadKey();
		}

		private static void AppDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			ExceptionWriteLine("UnhandledException: " + e.ExceptionObject.ToString());

			for (int i = ClientsList.Count - 1; i >= 0; i--)
			{
				ClientsList.ElementAt(i).Value.Dispose();
			}

			if (server != null)
				server.Dispose();
			ConsoleWriteLine("Crashed.  Press a key to close the window.");
			Console.ReadKey();
			Environment.Exit(1);
		}

		private static string EvaluatePromptHeader()
		{
			var regexTime = new System.Text.RegularExpressions.Regex("(?<!%)%T");
			var regexEscape = new System.Text.RegularExpressions.Regex("(?<!%)%%");
			return regexEscape.Replace(regexTime.Replace(PromptHeader, DateTime.Now.ToShortTimeString()), "%");
		}

		/// <summary>
		/// Writes a message on the console with the prompt header.
		/// </summary>
		/// <param name="message">Message without prompt header.</param>
		public static void ConsoleWriteLine(string message)
		{
			Console.WriteLine(EvaluatePromptHeader() + message);
		}

		/// <summary>
		/// Writes a debug message on the console with the prompt header.
		/// </summary>
		/// <param name="message">Message without prompt header.</param>
		public static void DebugWriteLine(string message)
		{
#if DEBUG
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine(EvaluatePromptHeader() + "[DEBUG] " + message);
			Console.ResetColor();
#endif
		}

		/// <summary>
		/// Writes a message on the console about an exception.
		/// </summary>
		/// <param name="message">Message without prompt header.</param>
		public static void ExceptionWriteLine(string message)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(EvaluatePromptHeader() + "[EXCEPT] " + message);
			Console.ResetColor();

			new TraceSource("DynServer").TraceEvent(TraceEventType.Error, 0, message);
		}

		/// <summary>
		/// Writes a message on the console about an exception.
		/// </summary>
		/// <param name="exception">Exception to show.</param>
		public static void ExceptionWriteLine(Exception exception)
		{
			ExceptionWriteLine(exception.Message);
		}

		public static void Broadcast(string message)
		{
			ConsoleWriteLine(message);

			foreach (var client in ClientsList)
			{
				client.Value.Send(message);
			}
		}
	}
}
