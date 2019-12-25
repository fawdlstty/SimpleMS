using Fawdlstty.SimpleMS.Private;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fawdlstty.SimpleMS.Attributes {
	public class ServiceDegradationAttribute: Attribute {
		public Func<Type [], object [], Type, object> DegradationFunc { get; set; } = Degradation.DefaultFunc;
	}
}
