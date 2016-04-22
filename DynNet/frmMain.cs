using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace DynNet
{
	public partial class frmMain : Form
	{
		public const int DEFAULT_PORT = 8888;

		TcpClient clientSocket;
		NetworkStream serverStream;

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
			if (clientSocket != null) clientSocket.Close();
			if (serverStream != null) serverStream.Close();
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
				clientSocket = new TcpClient();
				var serverAddress = txtServerAddress.Text.Split(':');
				if (serverAddress.Length == 2)
					clientSocket.Connect(serverAddress[0], int.Parse(serverAddress[1]));
				else
					clientSocket.Connect(txtServerAddress.Text, DEFAULT_PORT);
			}
			catch (SocketException)
			{
				MessageToConsole = "Can't connect to the server.";
				Cursor = Cursors.Default;
				return;
			}
			serverStream = clientSocket.GetStream();
			MessageToConsole = "Connected to DynServer.";

			byte[] outStream = Encoding.Unicode.GetBytes(txtUsername.Text + "$");
			serverStream.Write(outStream, 0, outStream.Length);
			serverStream.Flush();

			string returnData = GetMessageFromServer();
			//if (returnData != null)
			//{
			//	string[] connected = JsonConvert.DeserializeObject<string[]>(returnData);
			//	lstConnected.Items.AddRange(connected);
			//}

			Thread ctThread = new Thread(GetMessagesFromServer);
			ctThread.Start();

			SwitchView(true);
			Cursor = Cursors.Default;
		}

		private void txtSendingMessage_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			if (e.KeyData == Keys.Enter)
				btnSend_Click(sender, e);
		}

		private void btnSend_Click(object sender, EventArgs e)
		{
			try
			{
				byte[] outStream = Encoding.Unicode.GetBytes(txtSendingMessage.Text + "$");
				serverStream.Write(outStream, 0, outStream.Length);
				serverStream.Flush();
			}
			catch (IOException ex)
			{
				SocketException socketEx = ex.InnerException as SocketException;

				if (socketEx != null)
					switch (socketEx.SocketErrorCode)
					{
						case SocketError.ConnectionAborted:
						case SocketError.ConnectionReset:
						case SocketError.Interrupted:
							clientSocket.Dispose();
							serverStream.Close();
							return;
					}

				throw;
			}

			txtSendingMessage.Text = "";
		}

		private void GetMessagesFromServer()
		{
			while (clientSocket.Connected)
			{
				string returnData = GetMessageFromServer();
				if (returnData != null)
					MessageToConsole = returnData;
			}

			MessageToConsole = "Disconnected from the server.";
			clientSocket.Dispose();
			serverStream.Close();
			if (!IsDisposed) Invoke((MethodInvoker)delegate { SwitchView(false); lstConnected.Items.Clear(); });
		}

		private string GetMessageFromServer()
		{
			try
			{
				//serverStream = clientSocket.GetStream();
				//int buffSize = 0;
				byte[] inStream = new byte[10025];
				//buffSize = clientSocket.ReceiveBufferSize;
				int length = serverStream.Read(inStream, 0, inStream.Length);//TODO: Tester très long message.
				if (length == 0) return null; //TODO: Comprendre pourquoi le Read continue quand le server est déconnecté.
				return Encoding.Unicode.GetString(inStream, 0, length);
			}
			catch (IOException ex)
			{
				SocketException socketEx = ex.InnerException as SocketException;

				if (socketEx != null)
					switch (socketEx.SocketErrorCode)
					{
						case SocketError.ConnectionAborted:
						case SocketError.ConnectionReset:
						case SocketError.Interrupted:
							clientSocket.Dispose();
							serverStream.Close();
							return null;
					}

				throw;
			}
		}
	}
}
