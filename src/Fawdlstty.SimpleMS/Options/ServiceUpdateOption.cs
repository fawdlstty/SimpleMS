using System;
using System.Collections.Generic;
using System.Text;

namespace Fawdlstty.SimpleMS.Options {
	public class ServiceUpdateOption {
		public TimeSpan RefreshTime { get; set; } = TimeSpan.FromSeconds (10);
		public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds (1);
		public List<(string, int)> GatewayAddrs { get; } = new List<(string, int)> ();
	}
}
