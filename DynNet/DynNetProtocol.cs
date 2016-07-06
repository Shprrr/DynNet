using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
				default:
					AdditionalCommands(command, parameters);
					break;
			}
		}

		public virtual void AdditionalCommands(string command, string parameters) { }

		public string SendMessageConnect(string username)
		{
			return "connect " + JsonConvert.SerializeObject(new { Version = VersionProtocol, Username = username }, SerializerSettings);
		}
	}
}
