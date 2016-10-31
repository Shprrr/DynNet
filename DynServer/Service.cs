using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace DynServer
{
	partial class Service : ServiceBase
	{
		TraceSource trace;
		public Service()
		{
			InitializeComponent();
			trace = new TraceSource("DynServer");
		}

		protected override void OnStart(string[] args)
		{
			Program.StartServer(args);
			base.OnStart(args);
		}

		protected override void OnStop()
		{
			Program.StopServer();
			base.OnStop();
		}
	}
}
