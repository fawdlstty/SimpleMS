using Fawdlstty.SimpleMS.Datum;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Fawdlstty.SimpleMS.Private {
	public class ImplCaller {
		// 调用远程函数的中转：接受IL的请求并判断是否处降级
		public static object invoke_method (string _service_name, string _method_name, Dictionary<string, object> _params, Func<Dictionary<string, object>, Type, Exception, object> _deg_func, Type _ret_type) {
			if (_deg_func != null) {
				try {
					return _invoke_method_impl (_service_name, _method_name, _params, _ret_type);
				} catch (Exception ex) {
					return _deg_func (_params, _ret_type, ex);
				}
			} else {
				return _invoke_method_impl (_service_name, _method_name, _params, _ret_type);
			}
		}

		// 调用远程函数的中转：将远程请求的结果解析并返回给调用者
		private static async Task<object> _invoke_method_impl (string _service_name, string _method_name, Dictionary<string, object> _params, Type _ret_type) {
			string _content = JObject.FromObject (_params).ToString ();
			var _ret = await Singletons.InvokeRemoteService (_service_name, _method_name, _content);
			JObject _o = JObject.Parse (_ret);
			if (_o ["result"].ToObject<string> () == "success") {
				if (_ret_type == typeof (void) || _ret_type == typeof (Task)) {
					return null;
				} else if (_ret_type?.BaseType == typeof (Task)) {
					return _o ["content"].ToObject (_ret_type.GenericTypeArguments [0]);
				} else {
					throw new MethodAccessException ("返回类型必须基于Task");
				}
			}
			throw new MethodAccessException (_o ["reason"].ToObject<string> ());
		}
	}
}
