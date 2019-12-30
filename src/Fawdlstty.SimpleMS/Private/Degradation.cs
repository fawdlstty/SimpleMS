using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fawdlstty.SimpleMS.Private {
	internal class Degradation {
		public static Task DefaultFunc (Dictionary<string, object> _params, Type _return_type) {
			if (_return_type == typeof (void))
				return Task.CompletedTask;
			return Task.FromResult<object> (_return_type.BaseType?.FullName switch {
				"System.String" => @"{""result"":""failure"",""reason"":""degradation""}",
				"Newtonsoft.Json.Linq.JObject" => new JObject { ["result"] = "failure", ["reason"] = "degradation" },
				"System.Int16" => -1,
				"System.Int32" => -1,
				"System.Int64" => -1,
				"System.UInt16" => -1,
				"System.UInt32" => -1,
				"System.UInt64" => -1,
				_ => null
			});
		}
	}
}
