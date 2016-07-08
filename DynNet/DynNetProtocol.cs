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

		public void ReceiveMessage(string message)
		{
			string command, parameters;
			ExtractCommand(message, out command, out parameters);

			switch (command)
			{
				case "connected":
					if (parameters.IndexOf('"') == 0 && parameters.LastIndexOf('"') == parameters.Length - 1)
						OnConnected(parameters.Substring(1, parameters.Length - 2));
					break;

				case "disconnected":
					if (parameters.IndexOf('"') == 0 && parameters.LastIndexOf('"') == parameters.Length - 1)
						OnDisconnected(parameters.Substring(1, parameters.Length - 2));
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

		public event EventHandler<string> Disconnected;
		protected virtual void OnDisconnected(string username)
		{
			Disconnected?.Invoke(this, username);
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
