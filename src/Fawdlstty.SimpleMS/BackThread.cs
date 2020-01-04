using Fawdlstty.SimpleMS.Datum;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

			// 获取请求内容
			int port = Singletons.Option.LocalPort;
			var _post_query = new StringContent (JToken.FromObject (new { port, local, remote }).ToString (Formatting.None));
			var _post = new StringContent (JToken.FromObject (new { port, local, remote = Array.Empty<string> () }).ToString (Formatting.None));

			s_task = Task.Run (async () => {
				// 生成本地服务描述字符串
				var _dt = DateTime.Now;
				while (true) {
					// 如果作为服务并且没有注册，那么给与警告
					if (Singletons.EnableService && !Singletons.EnableGateway && Singletons.Option.GatewayAddrs.Count == 0)
						Console.WriteLine ("The service is not register to any gateway.");

					// 连接网关注册中心，更新自己作为服务端角色的其他服务信息
					bool _update = false;
					foreach (var _addr in Singletons.Option.GatewayAddrs) {
						try {
							var _cancel = new CancellationTokenSource (Singletons.Option.Timeout);
							using var _client = Singletons._get_client ();
							var _resp = await _client.PostAsync ($"http://{_addr.Item1}:{_addr.Item2}/_simplems_/register", _update ? _post : _post_query, _cancel.Token);
							if (!_update) {
								var _response = await _resp.Content.ReadAsStringAsync ();
								var _dic_outside = JsonConvert.DeserializeObject<Dictionary<string, List<(string, int)>>> (_response);
								lock (Singletons.ServiceLock)
									Singletons.OutsideAddrs = _dic_outside;
								_update = true;
							}
						} catch (Exception ex) {
							Console.WriteLine (ex.Message);
						}
					}

					// 更新自己作为网关角色的其他服务信息
					if (Singletons.EnableGateway) {
						var _dic_inside = new Dictionary<string, List<(string, int)>> ();
						lock (s_items) {
							var _remove_date = DateTime.Now - Singletons.Option.Timeout - Singletons.Option.RefreshTime;
							while (true) {
								var (_date, _host, _port, _) = s_items.Peek ();
								if (_date > _remove_date)
									break;
								if ((from p in s_items where p.Item2 == _host && p.Item3 == _port select 1).Count () < 2)
									Console.WriteLine ($"Service {_host}:{_port} offline.");
								s_items.Dequeue ();
							}
							foreach (var _item in s_items) {
								foreach (var _module_name in _item.Item4) {
									if (!_dic_inside.ContainsKey (_module_name))
										_dic_inside.Add (_module_name, new List<(string, int)> ());
									if (!_dic_inside [_module_name].Contains ((_item.Item2, _item.Item3)))
										_dic_inside [_module_name].Add ((_item.Item2, _item.Item3));
								}
							}
						}
						lock (Singletons.ServiceLock)
							Singletons.InsideAddrs = _dic_inside;
					}

					// 延时
					_dt += Singletons.Option.RefreshTime;
					var _now = DateTime.Now;
					if (_dt > _now) {
						await Task.Delay (_dt - _now);
					} else {
						_dt = _now;
					}
				}
			});
		}

		public static Dictionary<string, List<(string, int)>> AddService (string host, int port, List<string> local, List<string> remotes) {
			if (port > 0) {
				lock (s_items) {
					if ((from p in s_items where p.Item2 == host && p.Item3 == port select 1).Count () == 0)
						Console.WriteLine ($"Service {host}:{port} online.");
					s_items.Enqueue ((DateTime.Now, host, port, local));
				}
			}
			return Singletons.query_addrs (remotes);
		}

		private static Task s_task = null;
		private static Queue<(DateTime, string, int, List<string>)> s_items = new Queue<(DateTime, string, int, List<string>)> ();
	}
}
