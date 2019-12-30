using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Fawdlstty.SimpleMS {
	internal static class ExtensionMethods {
		// 获取服务名称
		public static string GetServiceName (this Type _type) {
			var _service_name = new StringBuilder ();
			var _method_infos = _type.GetMethods ();
			var _funcs = new List<string> ();
			foreach (var _method_info in _method_infos) {
				if (_funcs.Contains (_method_info.Name))
					throw new TypeLoadException ($"接口 {_type.Name} 不允许存在重名方法 {_method_info.Name}");
				_funcs.Add (_method_info.Name);
				//
				_service_name.Append ('/').Append (_method_info.Name).Append ('-').Append (_method_info.ReturnType);
				if ((_method_info.ReturnType.GenericTypeArguments?.Length ?? 0) > 0) {
					_service_name.Append ('<');
					foreach (var _arg in _method_info.ReturnType.GenericTypeArguments)
						_service_name.Append (',').Append (_arg.FullName);
					_service_name.Append ('>');
				}
				foreach (var _param in _method_info.GetParameters ()) {
					_service_name.Append (',').Append (_param.ParameterType.FullName);
					if ((_param.ParameterType.GenericTypeArguments?.Length ?? 0) > 0) {
						_service_name.Append ('<');
						foreach (var _arg in _param.ParameterType.GenericTypeArguments)
							_service_name.Append (',').Append (_arg.FullName);
						_service_name.Append ('>');
					}
				}
			}
			var _data = Encoding.UTF8.GetBytes (_service_name.ToString ());
			using MD5 _md5 = new MD5CryptoServiceProvider ();
			_data = _md5.ComputeHash (_data);
			_service_name.Clear ();
			for (int i = 0; i < _data.Length; ++i)
				_service_name.Append (_data [i].ToString ("x2"));
			return $"{_type.FullName}:{_service_name.ToString ()}";
		}
	}
}
