using Fawdlstty.SimpleMS.Options;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Fawdlstty.SimpleMS.Datum {
	internal class Singletons {
		// 选项参数
		public static ServiceUpdateOption Option { get; set; } = null;

		// 调用类字典，用于指定通过哪个对象来使用接口功能
		public static Dictionary<(string, Type), object> CallerMap { get; } = new Dictionary<(string, Type), object> ();

		// 远程接口指示类，用于指定某个接口有哪些（微）服务函数地址
		// 这个对象由Caller函数使用，同时也由网关使用
		public static Dictionary<string, List<(string, int)>> OutsideAddrs { private get; set; } = new Dictionary<string, List<(string, int)>> ();
		public static Dictionary<string, List<(string, int)>> InsideAddrs { private get; set; } = new Dictionary<string, List<(string, int)>> ();
		public static object ServiceLock { get; } = new object ();
		private static int s_inc = 0;

		// 是否启用了服务或网关
		public static bool EnableService { get; set; } = false;
		public static bool EnableGateway { get; set; } = false;

		// 调用远程服务
		public static async Task<string> InvokeRemoteService (string _service_name, string _method_name, string _content) {
			string _host = "";
			int _port = 0;
			lock (ServiceLock) {
				if (_service_name.EndsWith (":")) {
					foreach (var (_key, _val) in InsideAddrs) {
						if (_key.StartsWith (_service_name)) {
							(_host, _port) = _val [++s_inc % _val.Count];
							break;
						}
					}
					if (_port == 0) {
						foreach (var (_key, _val) in OutsideAddrs) {
							if (_key.StartsWith (_service_name)) {
								(_host, _port) = _val [++s_inc % _val.Count];
								break;
							}
						}
					}
				} else {
					if (InsideAddrs.TryGetValue (_service_name, out var _addrs1)) {
						(_host, _port) = _addrs1 [++s_inc % _addrs1.Count];
					} else if (OutsideAddrs.TryGetValue (_service_name, out var _addrs2)) {
						(_host, _port) = _addrs2 [++s_inc % _addrs2.Count];
					}
				}
				if (_port == 0)
					throw new MethodAccessException ($"服务 {_service_name} 未找到");
			}
			using var _client = _get_client ();
			using var _str_cnt = new StringContent (_content);
			using var _resp = await _client.PostAsync ($"http://{_host}:{_port}/_simplems_/api?module={_service_name}&method={_method_name}", _str_cnt);
			return await _resp.Content.ReadAsStringAsync ();
		}

		// 查询本轮服务地址列表
		public static Dictionary<string, List<(string, int)>> query_addrs (List<string> remotes) {
			var _dic = new Dictionary<string, List<(string, int)>> ();
			var _notcatch_remotes = new List<string> ();
			lock (InsideAddrs) {
				foreach (var _remote_item in remotes) {
					if (InsideAddrs.TryGetValue (_remote_item, out var _hosts)) {
						_dic.Add (_remote_item, _hosts);
					} else {
						_notcatch_remotes.Add (_remote_item);
					}
				}
			}
			lock (OutsideAddrs) {
				foreach (var _remote_item in _notcatch_remotes) {
					if (OutsideAddrs.TryGetValue (_remote_item, out var _hosts)) {
						_dic.Add (_remote_item, _hosts);
					}
				}
			}
			return _dic;
		}

		// 获取连接句柄
		public static HttpClient _get_client () {
			lock (s_collection) {
				if (s_factory == null) {
					s_collection.AddHttpClient ();
					s_provider = s_collection.BuildServiceProvider ();
					s_factory = s_provider.GetService<IHttpClientFactory> ();
				}
				return s_factory.CreateClient ();
			}
		}
		private static ServiceCollection s_collection = new ServiceCollection ();
		private static ServiceProvider s_provider = null;
		private static IHttpClientFactory s_factory = null;
	}
}
