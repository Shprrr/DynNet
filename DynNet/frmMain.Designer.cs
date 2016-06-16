namespace DynNet
{
	partial class frmMain
	{
		/// <summary>
		/// Variable nécessaire au concepteur.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Nettoyage des ressources utilisées.
		/// </summary>
		/// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Code généré par le Concepteur Windows Form

		/// <summary>
		/// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
		/// le contenu de cette méthode avec l'éditeur de code.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
			this.label1 = new System.Windows.Forms.Label();
			this.txtConsole = new System.Windows.Forms.TextBox();
			this.btnSend = new System.Windows.Forms.Button();
			this.txtSendingMessage = new System.Windows.Forms.TextBox();
			this.btnConnect = new System.Windows.Forms.Button();
			this.txtUsername = new System.Windows.Forms.TextBox();
			this.panConnection = new System.Windows.Forms.Panel();
			this.txtConsoleLogin = new System.Windows.Forms.TextBox();
			this.txtServerAddress = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.grbConnected = new System.Windows.Forms.GroupBox();
			this.lstConnected = new System.Windows.Forms.ListBox();
			this.grbGlobalChat = new System.Windows.Forms.GroupBox();
			this.panConnection.SuspendLayout();
			this.grbConnected.SuspendLayout();
			this.grbGlobalChat.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 32);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(61, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Username :";
			// 
			// txtConsole
			// 
			this.txtConsole.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtConsole.Location = new System.Drawing.Point(6, 19);
			this.txtConsole.Multiline = true;
			this.txtConsole.Name = "txtConsole";
			this.txtConsole.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtConsole.Size = new System.Drawing.Size(414, 337);
			this.txtConsole.TabIndex = 2;
			this.txtConsole.TabStop = false;
			// 
			// btnSend
			// 
			this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSend.Location = new System.Drawing.Point(345, 362);
			this.btnSend.Name = "btnSend";
			this.btnSend.Size = new System.Drawing.Size(75, 23);
			this.btnSend.TabIndex = 1;
			this.btnSend.Text = "&Send";
			this.btnSend.UseVisualStyleBackColor = true;
			this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
			// 
			// txtSendingMessage
			// 
			this.txtSendingMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtSendingMessage.Location = new System.Drawing.Point(6, 364);
			this.txtSendingMessage.Name = "txtSendingMessage";
			this.txtSendingMessage.Size = new System.Drawing.Size(333, 20);
			this.txtSendingMessage.TabIndex = 0;
			this.txtSendingMessage.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.txtSendingMessage_PreviewKeyDown);
			// 
			// btnConnect
			// 
			this.btnConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnConnect.Location = new System.Drawing.Point(178, 55);
			this.btnConnect.Name = "btnConnect";
			this.btnConnect.Size = new System.Drawing.Size(150, 23);
			this.btnConnect.TabIndex = 4;
			this.btnConnect.Text = "&Connect";
			this.btnConnect.UseVisualStyleBackColor = true;
			this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
			// 
			// txtUsername
			// 
			this.txtUsername.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtUsername.Location = new System.Drawing.Point(94, 29);
			this.txtUsername.Name = "txtUsername";
			this.txtUsername.Size = new System.Drawing.Size(234, 20);
			this.txtUsername.TabIndex = 3;
			// 
			// panConnection
			// 
			this.panConnection.Controls.Add(this.txtConsoleLogin);
			this.panConnection.Controls.Add(this.txtServerAddress);
			this.panConnection.Controls.Add(this.label2);
			this.panConnection.Controls.Add(this.btnConnect);
			this.panConnection.Controls.Add(this.txtUsername);
			this.panConnection.Controls.Add(this.label1);
			this.panConnection.Location = new System.Drawing.Point(12, 12);
			this.panConnection.Name = "panConnection";
			this.panConnection.Size = new System.Drawing.Size(331, 205);
			this.panConnection.TabIndex = 0;
			this.panConnection.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.panConnection_PreviewKeyDown);
			// 
			// txtConsoleLogin
			// 
			this.txtConsoleLogin.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtConsoleLogin.Location = new System.Drawing.Point(6, 84);
			this.txtConsoleLogin.Multiline = true;
			this.txtConsoleLogin.Name = "txtConsoleLogin";
			this.txtConsoleLogin.ReadOnly = true;
			this.txtConsoleLogin.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtConsoleLogin.Size = new System.Drawing.Size(322, 118);
			this.txtConsoleLogin.TabIndex = 5;
			this.txtConsoleLogin.TabStop = false;
			// 
			// txtServerAddress
			// 
			this.txtServerAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtServerAddress.Location = new System.Drawing.Point(94, 3);
			this.txtServerAddress.Name = "txtServerAddress";
			this.txtServerAddress.Size = new System.Drawing.Size(234, 20);
			this.txtServerAddress.TabIndex = 1;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(3, 6);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(85, 13);
			this.label2.TabIndex = 0;
			this.label2.Text = "Server Address :";
			// 
			// grbConnected
			// 
			this.grbConnected.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.grbConnected.Controls.Add(this.lstConnected);
			this.grbConnected.Location = new System.Drawing.Point(12, 12);
			this.grbConnected.Name = "grbConnected";
			this.grbConnected.Size = new System.Drawing.Size(140, 391);
			this.grbConnected.TabIndex = 2;
			this.grbConnected.TabStop = false;
			this.grbConnected.Text = "Connected";
			// 
			// lstConnected
			// 
			this.lstConnected.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lstConnected.FormattingEnabled = true;
			this.lstConnected.Location = new System.Drawing.Point(3, 16);
			this.lstConnected.Name = "lstConnected";
			this.lstConnected.Size = new System.Drawing.Size(134, 372);
			this.lstConnected.TabIndex = 0;
			// 
			// grbGlobalChat
			// 
			this.grbGlobalChat.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.grbGlobalChat.Controls.Add(this.txtConsole);
			this.grbGlobalChat.Controls.Add(this.txtSendingMessage);
			this.grbGlobalChat.Controls.Add(this.btnSend);
			this.grbGlobalChat.Location = new System.Drawing.Point(158, 12);
			this.grbGlobalChat.Name = "grbGlobalChat";
			this.grbGlobalChat.Size = new System.Drawing.Size(426, 391);
			this.grbGlobalChat.TabIndex = 1;
			this.grbGlobalChat.TabStop = false;
			this.grbGlobalChat.Text = "Global Chat";
			// 
			// frmMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(596, 415);
			this.Controls.Add(this.panConnection);
			this.Controls.Add(this.grbGlobalChat);
			this.Controls.Add(this.grbConnected);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "frmMain";
			this.Text = "DynNet";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmMain_FormClosed);
			this.Load += new System.EventHandler(this.frmMain_Load);
			this.panConnection.ResumeLayout(false);
			this.panConnection.PerformLayout();
			this.grbConnected.ResumeLayout(false);
			this.grbGlobalChat.ResumeLayout(false);
			this.grbGlobalChat.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtConsole;
		private System.Windows.Forms.Button btnSend;
		private System.Windows.Forms.TextBox txtSendingMessage;
		private System.Windows.Forms.Button btnConnect;
		private System.Windows.Forms.TextBox txtUsername;
		private System.Windows.Forms.Panel panConnection;
		private System.Windows.Forms.TextBox txtServerAddress;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.GroupBox grbConnected;
		private System.Windows.Forms.ListBox lstConnected;
		private System.Windows.Forms.GroupBox grbGlobalChat;
		private System.Windows.Forms.TextBox txtConsoleLogin;
	}
}

