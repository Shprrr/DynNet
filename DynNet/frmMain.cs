using System;
using System.Configuration;
using System.Net.Sockets;
using System.Windows.Forms;
using DynServer;

namespace DynNet
{
	public partial class frmMain : Form
	{
		public const int DefaultPort = 8888;
		public const int BufferSize = 10025;

		public ClientConnectionBase ClientConnection { get; private set; }
		public DynNetProtocol Protocol { get; private set; }

		#region MessageToConsole
		private string _MessageToConsole = null;
		public string MessageToConsole { set { _MessageToConsole = value; AddMessageToConsole(); } }

		private void AddMessageToConsole()
		{
			if (IsDisposed) return;

			if (InvokeRequired)
			{
				try
				{
					Invoke(new MethodInvoker(AddMessageToConsole));
				}
				catch (ObjectDisposedException)
				{
					// ARRÊTE DE ME GOSSER !!!!
				}
			}
			else
			{
				string newText = (string.IsNullOrWhiteSpace(txtConsole.Text) ? "" : Environment.NewLine) +
					"[" + DateTime.Now.ToShortTimeString() + "] >> " + _MessageToConsole;
				txtConsole.AppendText(newText);
				txtConsoleLogin.AppendText(newText);
				_MessageToConsole = null;
			}
		}
		#endregion

		public frmMain()
		{
			InitializeComponent();

			txtServerAddress.Text = ConfigurationManager.AppSettings["server"];
			Protocol = new DynNetProtocol();
			Protocol.Connected += Protocol_Connected;
			Protocol.Disconnected += Protocol_Disconnected;
			Protocol.WhoResponse += Protocol_WhoResponse;
			Protocol.Message += Protocol_Message;
		}

		private void frmMain_Load(object sender, EventArgs e)
		{
			SwitchView(false);
		}

		private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (ClientConnection != null) ClientConnection.Dispose();
		}

		private void SwitchView(bool connected)
		{
			panConnection.Visible = !connected;
			panConnection.Enabled = !connected;
			grbConnected.Visible = connected;
			grbConnected.Enabled = connected;
			grbGlobalChat.Visible = connected;
			grbGlobalChat.Enabled = connected;

			if (connected)
				txtSendingMessage.Focus();
			else
			{
				txtConsoleLogin.SelectionStart = txtConsoleLogin.Text.Length;
				txtConsoleLogin.ScrollToCaret();
				if (!string.IsNullOrWhiteSpace(txtServerAddress.Text))
					ActiveControl = txtUsername;

				lstConnected.Items.Clear();
			}
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			object sender = FromHandle(msg.HWnd);
			if (keyData == Keys.Enter)
			{
				if (panConnection.ContainsFocus)
				{
					panConnection_PreviewKeyDown(sender, new PreviewKeyDownEventArgs(keyData));
					return true;
				}
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		private void panConnection_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				btnConnect_Click(sender, e);
		}

		private void btnConnect_Click(object sender, EventArgs e)
		{
			MessageToConsole = "Connecting to a DynServer ...";
			Cursor = Cursors.WaitCursor;
			try
			{
				TcpClient clientSocket = new TcpClient();
				var serverAddress = txtServerAddress.Text.Split(':');
				if (serverAddress.Length == 2)
					clientSocket.Connect(serverAddress[0], int.Parse(serverAddress[1]));
				else
					clientSocket.Connect(txtServerAddress.Text, DefaultPort);
				ClientConnection = new ClientConnectionBase(clientSocket, BufferSize);
			}
			catch (SocketException)
			{
				MessageToConsole = "Can't connect to the server.";
				Cursor = Cursors.Default;
				return;
			}
			ClientConnection.ReceivingMessage += ClientConnection_ReceivingMessage;
			ClientConnection.Disconnecting += ClientConnection_Disconnecting;

			MessageToConsole = "Connected to DynServer.";
			ClientConnection.Send(Protocol.ConstructMessageConnect(txtUsername.Text));

			SwitchView(true);
			Cursor = Cursors.Default;
		}

		private void ClientConnection_ReceivingMessage(object sender, string message)
		{
			if (!string.IsNullOrWhiteSpace(message))
				Protocol.ReceiveMessage(message);
		}

		private void Protocol_Connected(object sender, string username)
		{
			MessageToConsole = username + " joined.";
			lstConnected.Items.Add(username);
		}

		private void Protocol_Disconnected(object sender, string username)
		{
			MessageToConsole = username + " has been disconnected.";
			lstConnected.Items.Remove(username);
		}

		private void Protocol_WhoResponse(object sender, string[] usernames)
		{
			usernames = Array.FindAll(usernames, u => u != txtUsername.Text);
			if (!IsDisposed)
				Invoke((MethodInvoker)delegate
				{
					lstConnected.Items.Clear();
					lstConnected.Items.AddRange(usernames);
				});
		}

		private void Protocol_Message(object sender, DynNetProtocol.MessageParameter message)
		{
			MessageToConsole = message.Username + " says : " + message.Message;
		}

		private void ClientConnection_Disconnecting(object sender, EventArgs e)
		{
			MessageToConsole = "Disconnected from the server.";
			if (!IsDisposed) Invoke((MethodInvoker)delegate { SwitchView(false); });
		}

		private void txtSendingMessage_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			if (e.KeyData == Keys.Enter)
				btnSend_Click(sender, e);
		}

		private void btnSend_Click(object sender, EventArgs e)
		{
			if (!(ClientConnection?.Socket.Connected).GetValueOrDefault()) return;

			ClientConnection.Send(Protocol.ConstructChatMessage(txtSendingMessage.Text));
			txtSendingMessage.Text = "";
		}
	}
}
