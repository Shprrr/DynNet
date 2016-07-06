using System;
using Newtonsoft.Json;

namespace DynServer
{
	public class DynNetProtocol
	{
		public const string VersionProtocol = "0.0";

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
				case "connect":
					Connect(parameters);
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
		public virtual void AdditionalCommands(string command, string parameters) { }

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

			Connect(parameters);
			return true;
		}

		private void Connect(string parameters)
		{
			/*Envoi : connect { version: “1.0”, username: “username” }
			La version correspond à la version du protocole utilisée (et non à la version de l’application).
			Reçoit: Déconnexion si la version du protocole n’est pas supporté par le serveur.*/

			var obj = JsonConvert.DeserializeAnonymousType(parameters, new { Version = VersionProtocol, Username = "username" });

			if (obj.Version != VersionProtocol)
				throw new System.Net.ProtocolViolationException("Protocol not supported.");

			OnConnecting(obj.Username);
		}

		public event EventHandler<string> Connecting;
		protected virtual void OnConnecting(string username)
		{
			Connecting?.Invoke(this, username);
		}
		#endregion
	}
}
