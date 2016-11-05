using System;
using Newtonsoft.Json;

namespace DynNet
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

			return parameters.Substring(startQuote + 1, endQuote - startQuote - 1);
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
				case "connected":
					{
						string username = ExtractParameterValueInString(parameters);
						if (username != parameters)
							OnConnected(username);
					}
					break;

				case "disconnected":
					{
						string username = ExtractParameterValueInString(parameters);
						if (username != parameters)
						{
							int endIndexUsername = IndexOfQuote(parameters, IndexOfQuote(parameters) + 1);
							if (endIndexUsername > 0)
							{
								string reason = ExtractParameterValueInString(parameters, endIndexUsername + 1);
								if (reason != parameters)
								{
									OnDisconnected(username, reason);
									break;
								}
							}
							OnDisconnected(username);
						}
					}
					break;

				case "whoresponse":
					try
					{
						var obj = JsonConvert.DeserializeObject<string[]>(parameters);
						OnWhoResponse(obj);
					}
					catch (JsonException)
					{
					}
					break;

				case "m":
					try
					{
						var obj = JsonConvert.DeserializeObject<MessageParameter>(parameters);
						OnMessage(obj);
					}
					catch (JsonException)
					{
					}
					break;

				default:
					AdditionalCommands(command, parameters);
					break;
			}
		}

		protected virtual void AdditionalCommands(string command, string parameters) { }

		public event EventHandler<string> Connected;
		protected virtual void OnConnected(string username)
		{
			Connected?.Invoke(this, username);
		}

		public struct DisconnectedParameter
		{
			public string Username { get; set; }
			public string Reason { get; set; }
		}
		public event EventHandler<DisconnectedParameter> Disconnected;
		protected virtual void OnDisconnected(string username, string reason = null)
		{
			Disconnected?.Invoke(this, new DisconnectedParameter { Username = username, Reason = reason });
		}

		public event EventHandler<string[]> WhoResponse;
		protected virtual void OnWhoResponse(string[] usernames)
		{
			WhoResponse?.Invoke(this, usernames);
		}

		public struct MessageParameter
		{
			public string Username { get; set; }
			public string Message { get; set; }
		}
		public event EventHandler<MessageParameter> Message;
		protected virtual void OnMessage(MessageParameter message)
		{
			Message?.Invoke(this, message);
		}

		/// <summary>
		/// Message to send when establishing a new connection to the server to be accepted by it.
		/// </summary>
		/// <param name="username"></param>
		/// <returns></returns>
		public string ConstructMessageConnect(string username)
		{
			return "connect " + JsonConvert.SerializeObject(new { Version = VersionProtocol, Username = username }, SerializerSettings);
		}

		/// <summary>
		/// Public message in the chat.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public string ConstructChatMessage(string message)
		{
			return "m " + message;
		}
	}
}
