using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fawdlstty.SimpleMS.Private {
	internal class ImplCaller {
		public static object invoke_method (string _full_name, Dictionary<string, object> _params, Func<Dictionary<string, object>, Type, object> _deg_func, Type _ret_type) {
			if (_deg_func != null) {
				try {
					return _invoke_method_impl (_full_name, _params, _ret_type);
				} catch (Exception) {
					return _deg_func (_params, _ret_type);
				}
			} else {
				return _invoke_method_impl (_full_name, _params, _ret_type);
			}
		}

		private static object _invoke_method_impl (string _full_name, Dictionary<string, object> _params, Type _ret_type) {
			string _data = JObject.FromObject (_params).ToString ();
			// TODO: 发送
		}
	}
}
