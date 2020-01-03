using Fawdlstty.SimpleMS.Datum;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fawdlstty.SimpleMS {
	/// <summary>
	/// 此类用于创建后台线程同步本地公共模块
	/// </summary>
	internal class BackThread {
		public static void Start (List<string> local, List<string> remote) {
			if (s_task != null)
				return;

			// 获取客户端工厂
			var _service_collection = new ServiceCollection ();
			_service_collection.AddHttpClient ();
			using var _services = _service_collection.BuildServiceProvider ();
			var _client_factory = _services.GetService<IHttpClientFactory> ();

			// 获取请求内容
			int port = Singletons.Option.LocalPort;
			var _post_query = new StringContent (JToken.FromObject (new { port, local, remote }).ToString (Formatting.None));
			var _post = new StringContent (JToken.FromObject (new { port, local, remote = Array.Empty<string> () }).ToString (Formatting.None));

			s_task = Task.Run (async () => {
				// 生成本地服务描述字符串
				var _dt = DateTime.Now;
				while (true) {
					// 连接网关注册中心，更新自己作为服务端角色的其他服务信息
					bool _update = false;
					foreach (var _addr in Singletons.Option.GatewayAddrs) {
						try {
							var _cancel = new CancellationTokenSource (Singletons.Option.Timeout);
							using var _client = _client_factory.CreateClient ();
							var _resp = await _client.PostAsync ($"http://{_addr.Item1}:{_addr.Item2}/_simplems_/register", _update ? _post : _post_query, _cancel.Token);
							if (!_update) {
								var _response = await _resp.Content.ReadAsStringAsync ();
								var _dic = JsonConvert.DeserializeObject<Dictionary<string, List<(string, int)>>> (_response);
								lock (Singletons.ServiceLock)
									Singletons.ServiceAddrs = _dic;
								_update = true;
							}
						} catch (Exception) {
						}
					}

					// 更新自己作为网关角色的其他服务信息

					// 延时
					_dt += Singletons.Option.RefreshTime;
					await Task.Delay (_dt - DateTime.Now);
				}
			});
		}

		public static Dictionary<string, List<(string, int)>> AddService (string host, int port, List<string> local, List<string> remote) {
			lock (s_items) {
				s_items.Enqueue ((DateTime.Now, host, port, local));
				while (s_items.Peek ().Item1 < DateTime.Now - TimeSpan.FromSeconds (15))
					s_items.Dequeue ();
				var _dic = new Dictionary<string, List<(string, int)>> ();
				if ((remote?.Count ?? 0) > 0) {
					foreach (var _remote_item in remote) {
						_dic.Add (_remote_item, new List<(string, int)> ());
					}
					foreach (var _item in s_items) {
						foreach (var _dic_item in _dic) {
							if (_item.Item4.Contains (_dic_item.Key))
								_dic_item.Value.Add ((_item.Item2, _item.Item3));
						}
					}
				}
				return _dic;
			}
		}

		private static Task s_task = null;
		private static Queue<(DateTime, string, int, List<string>)> s_items = new Queue<(DateTime, string, int, List<string>)> ();
	}
}
