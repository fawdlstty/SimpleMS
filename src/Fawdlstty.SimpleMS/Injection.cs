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
using System.Text;
using System.Threading.Tasks;

namespace Fawdlstty.SimpleMS {
	public static class Injection {
		public static IServiceCollection AddSimpleMS (this IServiceCollection services, Action<ServiceUpdateOption> option) {
			var _option = new ServiceUpdateOption ();
			option?.Invoke (_option);
			if (_option.GatewayAddrs.Count == 0)
				throw new ArgumentException ("网关数量不可为0");
			var (_local, _remote) = ImplTypeBuilder.InitInterfaces (_option);
			BackThread.Start (_local, _remote);
			return services;
		}

		// 对外提供服务
		public static IApplicationBuilder UseSimpleMSService (this IApplicationBuilder app) {
			app.Use (async (_ctx, _next) => {
				if (_ctx.Request.Path == "/_simplems_/api") {
					string _module = _ctx.Request.Query ["module"].ToString ();
					if (!_module.Contains (':'))
						_module = $"{_module}:";
					foreach (var (_key, _obj) in Singletons.CallerMap) {
						if (!_key.Item1.StartsWith (_module))
							continue;
						// 调用
						var _method_name = _ctx.Request.Query ["method"].ToString ();
						var _method = _key.Item2.GetMethod (_method_name);
						if (_method == null) {
							await _ctx.Response.WriteAsync ($"方法 {_method_name} 不存在");
							return;
						}
						// TODO: 生成参数列表并传参
						var _ret = _method.Invoke (_obj, );
						if (_method.ReturnType == typeof (Task)) {
							await (Task) _ret;
							await _ctx.Response.WriteAsync (JsonConvert.SerializeObject (new { result = "success" }));
						} else {
							_ret = await (Task<object>) _ret;
							await _ctx.Response.WriteAsync (JsonConvert.SerializeObject (_ret));
						}
					}
				} else {
					await _next ();
				}
			});
			return app;
		}

		// 对外提供网关/注册中心
		public static IApplicationBuilder UseSimpleMSGateway (this IApplicationBuilder app, string prefix = "/api") {
			app.Use (async (_ctx, _next) => {
				string _path = _ctx.Request.Path.Value;
				bool _register = _path == "/_simplems_/register", _callmethod = _path.StartsWith (prefix);
				if (_register || _callmethod) {
					var _bytes = new byte [_ctx.Request.ContentLength ?? 0];
					if (_register && _bytes.Length == 0) {
						await _ctx.Response.WriteAsync ("网关注册中心已架设成功，可通过这个地址来注册微服务");
						return;
					}
					await _ctx.Request.Body.ReadAsync (_bytes);
					JObject _obj = JObject.Parse (Encoding.UTF8.GetString (_bytes));
					if (_register) {
						var _local = _obj ["local"].ToObject<List<string>> ();
					} else {

					}
				} else {
					await _next ();
				}
			});
			return app;
		}
	}
}
