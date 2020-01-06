using Fawdlstty.SimpleMS.Datum;
using Fawdlstty.SimpleMS.Options;
using Fawdlstty.SimpleMS.Private;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fawdlstty.SimpleMS {
	public static class Injection {
		public static IServiceCollection AddSimpleMS (this IServiceCollection services, Action<ServiceUpdateOption> option = null) {
			if (Singletons.Option != null)
				throw new NotSupportedException ("请确保 services.AddSimpleMS () 只被调用一次");
			var _option = new ServiceUpdateOption (services);
			option?.Invoke (_option);
			Singletons.Option = _option;
			var (_local, _remote) = ImplTypeBuilder.InitInterfaces ();
			if (Singletons.Option.DiscoveryType == DiscoveryTypeEnum.RegCenter)
				BackThread.Start (_local, _remote);
			return services;
		}

		// 对外提供服务
		public static IApplicationBuilder UseSimpleMSService (this IApplicationBuilder app) {
			if (Singletons.Option.LocalPort == 0)
				throw new ArgumentException ("必须指定本地服务端口号");
			Singletons.EnableService = true;
			app.Use (async (_ctx, _next) => {
				if (_ctx.Request.Path == "/_simplems_/load_dll") {
					// TODO: 返回dll二进制数据
				} else if (_ctx.Request.Path == "/_simplems_/api") {
					string _module_name = _ctx.Request.Query ["module"].ToString ();
					if (!_module_name.Contains (':'))
						_module_name = $"{_module_name}:";
					var _method_name = _ctx.Request.Query ["method"].ToString ();
					var _bytes = new byte [_ctx.Request.ContentLength ?? 0];
					if (_bytes.Length > 0) {
						await _ctx.Request.Body.ReadAsync (_bytes);
					}
					var _resp = await _process_method_call (_module_name, _method_name, Encoding.UTF8.GetString (_bytes));
					await _ctx.Response.WriteAsync (_resp);
				} else {
					await _next ();
				}
			});
			return app;
		}

		// 对外提供网关/注册中心
		public static IApplicationBuilder UseSimpleMSGateway (this IApplicationBuilder app, string prefix = "/api") {
			if (Singletons.Option.LocalPort == 0)
				throw new ArgumentException ("必须指定本地服务端口号");
			Singletons.EnableGateway = true;
			app.Use (async (_ctx, _next) => {
				string _path = _ctx.Request.Path.Value;
				bool _register = _path == "/_simplems_/register", _callmethod = _path.StartsWith (prefix);
				if (_register || _callmethod) {
					var _bytes = new byte [_ctx.Request.ContentLength ?? 0];
					if (_register && _bytes.Length == 0) {
						await _ctx.Response.WriteAsync ("网关注册中心已架设成功，可通过这个地址来注册微服务");
						return;
					}
					if (_bytes.Length > 0)
						await _ctx.Request.Body.ReadAsync (_bytes, 0, _bytes.Length);
					string _resp = "";
					if (_register) {
						JObject _obj = JObject.Parse (Encoding.UTF8.GetString (_bytes));
						string _host = _ctx.Request.Host.Host;
						int _port = _obj ["port"].ToObject<int> ();
						var _local = _obj ["local"].ToObject<List<string>> ();
						var _remote = _obj ["remote"].ToObject<List<string>> ();
						var _ret = BackThread.AddService (_host, _port, _local, _remote);
						_resp = JsonConvert.SerializeObject (_ret);
					} else if (_callmethod) {
						string _module_name = _ctx.Request.Query ["module"].ToString ();
						if (!_module_name.Contains (':'))
							_module_name = $"{_module_name}:";
						var _method_name = _ctx.Request.Query ["method"].ToString ();
						_resp = await _process_method_call (_module_name, _method_name, Encoding.UTF8.GetString (_bytes));
					}
					await _ctx.Response.WriteAsync (_resp);
				} else {
					await _next ();
				}
			});
			return app;
		}

		// 处理外部调用
		private static async Task<string> _process_method_call (string _module_name, string _method_name, string _content) {
			string _resp = "";
			// 确认是否能直接调用
			foreach (var (_key, _obj) in Singletons.CallerMap) {
				if (!_key.Item1.StartsWith (_module_name))
					continue;

				// 获取参数内容
				var _method = _key.Item2.GetMethod (_method_name);
				if (_method == null)
					throw new MissingMethodException ($"未在模块 {_module_name} 中找到");
				var _param_infos = _method.GetParameters ();
				object [] _params = new object [_param_infos?.Length ?? 0];
				if (_content?.Length > 0 && _params.Length > 0) {
					JObject _param_obj = JObject.Parse (_content);
					for (int i = 0; i < _params.Length; ++i)
						_params [i] = _param_obj [_param_infos [i].Name].ToObject (_param_infos [i].ParameterType);
				}

				// 调用
				try {
					var _ret = _method.Invoke (_obj, _params);
					if (_method.ReturnType == typeof (Task)) {
						await (Task) _ret;
						_resp = JsonConvert.SerializeObject (new { result = "success" });
					} else {
						await (Task) _ret;
						_ret = _ret.GetType ().InvokeMember ("Result", BindingFlags.GetProperty, null, _ret, Array.Empty<object> ());
						_resp = JsonConvert.SerializeObject (new { result = "success", content = _ret });
					}
				} catch (Exception ex) {
					_resp = JsonConvert.SerializeObject (new { result = "failure", reason = ex.Message });
				}
				return _resp;
			}

			// 转发调用
			try {
				return await Singletons.InvokeRemoteService (_module_name, _method_name, _content);
			} catch (Exception ex) {
				_resp = JsonConvert.SerializeObject (new { result = "failure", reason = ex.Message });
			}
			return _resp;
		}
	}
}
