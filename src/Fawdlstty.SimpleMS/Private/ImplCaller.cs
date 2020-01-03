using Fawdlstty.SimpleMS.Datum;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Fawdlstty.SimpleMS.Private {
	internal class ImplCaller {
		public static object invoke_method (string _service_name, string _method_name, Dictionary<string, object> _params, Func<Dictionary<string, object>, Type, object> _deg_func, Type _ret_type) {
			if (_deg_func != null) {
				try {
					return _invoke_method_impl (_service_name, _method_name, _params, _ret_type);
				} catch (Exception) {
					return _deg_func (_params, _ret_type);
				}
			} else {
				return _invoke_method_impl (_service_name, _method_name, _params, _ret_type);
			}
		}

		// TODO:
		private static Task<object> _invoke_method_impl (string _service_name, string _method_name, Dictionary<string, object> _params, Type _ret_type) {
			string _content = JObject.FromObject (_params).ToString ();
			//var _task = Singletons.InvokeRemoteService (_service_name, _method_name, _content);
			//_task.ContinueWith ((_arg) => {
			//	_arg.Result
			//});
			return null;
		}
	}
}
