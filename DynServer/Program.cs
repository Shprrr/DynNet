using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace DynServer
{
	class Program
	{
		public static string PromptHeader = "[%T] >> ";
		public const int BufferSize = 10025;

		public static Dictionary<System.Net.Sockets.TcpClient, ClientConnection> PendingClientsList = new Dictionary<System.Net.Sockets.TcpClient, ClientConnection>();
		public static Dictionary<string, ClientConnection> ClientsList = new Dictionary<string, ClientConnection>();

		private static ServerConnection server = null;
		public static TraceSource Trace { get; private set; }

		private static void Main(string[] args)
		{
			Trace = new TraceSource("DynServer");

			if (!Environment.UserInteractive)
			{
				System.ServiceProcess.ServiceBase.Run(new Service());
				return;
			}

			StartServer(args);

			bool exit = false;
			while (!exit && server != null)
			{
				string line = Console.ReadLine();
				if (server == null) break;
				if (line != null && (line.StartsWith("exit") || line.StartsWith("quit")))
					exit = true;
			}

			if (server != null)
				StopServer();
		}

		internal static void StartServer(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += AppDomain_UnhandledException;

			if (ConfigurationManager.AppSettings["prompt"] != null)
				PromptHeader = ConfigurationManager.AppSettings["prompt"];

			int port = 8888;
			int.TryParse(ConfigurationManager.AppSettings["port"], out port);
			if (args.Length > 0)
				int.TryParse(args[0], out port);

			server = new ServerConnection(port, 5);
			ConsoleWriteLine("Server Started on port " + port);
		}

		[DllImport("User32.Dll", EntryPoint = "PostMessageA")]
		private static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

		const int VK_RETURN = 0x0D;
		const int WM_KEYDOWN = 0x100;

		internal static void StopServer(bool crashed = false)
		{
			for (int i = ClientsList.Count - 1; i >= 0; i--)
			{
				ClientsList.ElementAt(i).Value.Dispose();
			}

			if (server != null)
			{
				server.Dispose();
				server = null;
			}

			if (crashed)
				ConsoleWriteLine("Crashed.  Press a key to close the window.");
			else
				ConsoleWriteLine("Exited.  Press a key to close the window.");

			if (Environment.UserInteractive)
			{
				if (crashed)
					PostMessage(Process.GetCurrentProcess().MainWindowHandle, WM_KEYDOWN, VK_RETURN, 0); // Stop Console.ReadLine.
				Console.ReadKey(true);
			}
			if (crashed)
				Environment.Exit(1);
		}

		private static void AppDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			ExceptionWriteLine("UnhandledException: " + e.ExceptionObject.ToString());

			StopServer(true);
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

			Trace.TraceEvent(TraceEventType.Error, 0, message);
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
