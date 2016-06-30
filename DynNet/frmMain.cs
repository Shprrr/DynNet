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
			MessageToConsole = "Connected to DynServer.";
			ClientConnection.Send(txtUsername.Text);

			//string returnData = GetMessageFromServer();
			//if (returnData != null)
			//{
			//	string[] connected = JsonConvert.DeserializeObject<string[]>(returnData);
			//	lstConnected.Items.AddRange(connected);
			//}

			ClientConnection.ReceivingMessage += ClientConnection_ReceivingMessage;
			ClientConnection.Disconnecting += ClientConnection_Disconnecting;

			SwitchView(true);
			Cursor = Cursors.Default;
		}

		private void ClientConnection_ReceivingMessage(object sender, string message)
		{
			MessageToConsole = message;
		}

		private void ClientConnection_Disconnecting(object sender, EventArgs e)
		{
			MessageToConsole = "Disconnected from the server.";
			if (!IsDisposed) Invoke((MethodInvoker)delegate { SwitchView(false); lstConnected.Items.Clear(); });
		}

		private void txtSendingMessage_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			if (e.KeyData == Keys.Enter)
				btnSend_Click(sender, e);
		}

		private void btnSend_Click(object sender, EventArgs e)
		{
			if (!(ClientConnection?.Socket.Connected).GetValueOrDefault()) return;

			ClientConnection.Send(txtSendingMessage.Text);
			txtSendingMessage.Text = "";
		}
	}
}
