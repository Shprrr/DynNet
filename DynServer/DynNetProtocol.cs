using System;
using Newtonsoft.Json;

namespace DynServer
{
	public class DynNetProtocol
	{
		public const string VersionProtocol = "0.0";
		private static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings();

		static DynNetProtocol()
		{
			SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
			SerializerSettings.Formatting = Formatting.None;
		}

		public static void ExtractCommand(string message, out string command, out string parameters)
		{
			int indexEndCommand = message.IndexOf(' ');
			if (indexEndCommand > 0) // Can't have empty as command.
			{
				command = message.Remove(indexEndCommand).ToLowerInvariant();
				parameters = message.Substring(indexEndCommand + 1);
			}
			else
			{
				command = message.ToLowerInvariant();
				parameters = "";
			}
		}

		public static int IndexOfQuote(string parameters, int startIndex = 0)
		{
			int indexQuote = parameters.IndexOf('"', startIndex);
			while (indexQuote > 0 && parameters[indexQuote - 1] == '\\')
				indexQuote = parameters.IndexOf('"', indexQuote + 1);
			return indexQuote;
		}

		public static string ExtractParameterValueInString(string parameters, int startIndex = 0)
		{
			int startQuote = IndexOfQuote(parameters, startIndex);
			if (startQuote == -1)
				return parameters;

			int endQuote = IndexOfQuote(parameters, startQuote + 1);
			if (endQuote == -1)
				return parameters;

			return parameters.Substring(startQuote + 1, endQuote - startQuote - 1).Replace("\\\"", "\"");
		}

		public static string SerializeInString(string value)
		{
			if (value == null) return null;
			return value.Replace("\"", "\\\"");
		}

		public void ReceiveMessage(string message)
		{
			string command, parameters;
			ExtractCommand(message, out command, out parameters);

			switch (command)
			{
				case "connect":
					ValidateConnect(parameters);
					break;

				case "m":
					OnMessage(parameters);
					break;

				default:
					AdditionalCommands(command, parameters);
					break;
			}
		}

		/// <summary>
		/// Adds others commands to be accepted.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="parameters"></param>
		protected virtual void AdditionalCommands(string command, string parameters) { }

		#region connect
		/// <summary>
		/// Check if the message is for this action and use it.
		/// </summary>
		/// <param name="message"></param>
		/// <returns>If the message is for this action or not.</returns>
		public bool ReceiveMessageConnect(string message)
		{
			string command, parameters;
			ExtractCommand(message, out command, out parameters);

			if (command != "connect") return false;

			ValidateConnect(parameters);
			return true;
		}

		private void ValidateConnect(string parameters)
		{
			try
			{
				var obj = JsonConvert.DeserializeAnonymousType(parameters, new { Version = VersionProtocol, Username = "username" });

				if (obj.Version != VersionProtocol)
					throw new System.Net.ProtocolViolationException("Protocol not supported.");

				OnConnect(obj.Username);
			}
			catch (JsonException ex)
			{
				Program.ExceptionWriteLine(ex);
			}
		}

		public event EventHandler<string> Connect;
		protected virtual void OnConnect(string username)
		{
			Connect?.Invoke(this, username);
		}
		#endregion

		public event EventHandler<string> Message;
		protected virtual void OnMessage(string message)
		{
			Message?.Invoke(this, message);
		}

		/// <summary>
		/// Indicates that the username just connected.
		/// </summary>
		/// <param name="username"></param>
		/// <returns></returns>
		public string ConstructMessageConnected(string username)
		{
			return "connected \"" + SerializeInString(username) + "\"";
		}

		/// <summary>
		/// Indicates that the username just disconnected.
		/// </summary>
		/// <param name="username"></param>
		/// <returns></returns>
		public string ConstructMessageDisconnected(string username)
		{
			return "disconnected \"" + SerializeInString(username) + "\"";
		}

		/// <summary>
		/// Indicates that the username just disconnected with a reason.
		/// </summary>
		/// <param name="username"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public string ConstructMessageDisconnected(string username, string reason)
		{
			return "disconnected \"" + SerializeInString(username) + "\" \"" + SerializeInString(reason) + "\"";
		}

		/// <summary>
		/// List all connected users.
		/// </summary>
		/// <param name="usernames"></param>
		/// <returns></returns>
		public string ConstructMessageWhoResponse(string[] usernames)
		{
			return "whoresponse " + JsonConvert.SerializeObject(usernames, SerializerSettings);
		}

		/// <summary>
		/// Public message in the chat.
		/// </summary>
		/// <param name="username"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public string ConstructChatMessage(string username, string message)
		{
			return "m " + JsonConvert.SerializeObject(new { Username = username, Message = message }, SerializerSettings);
		}
	}
}
