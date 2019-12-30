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
			var _post_query = new StringContent (JToken.FromObject (new { local, remote }).ToString (Formatting.None));
			var _post = new StringContent (JToken.FromObject (new { local }).ToString (Formatting.None));

			s_task = Task.Run (async () => {
				// 生成本地服务描述字符串
				var _dt = DateTime.Now;
				while (true) {
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
					_dt += Singletons.Option.RefreshTime;
					await Task.Delay (_dt - DateTime.Now);
				}
			});
		}

		private static Task s_task = null;
	}
}
