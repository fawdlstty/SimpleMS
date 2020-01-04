using Fawdlstty.SimpleMS.Private;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fawdlstty.SimpleMS.Attributes {
	public class ServiceDegradationAttribute: Attribute {
		public Func<Dictionary<string, object>, Type, Exception, Task> DegradationFunc { get; set; } = Degradation.DefaultFunc;
	}
}
