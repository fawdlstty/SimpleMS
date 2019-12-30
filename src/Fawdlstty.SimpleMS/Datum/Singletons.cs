using Fawdlstty.SimpleMS.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fawdlstty.SimpleMS.Datum {
	internal class Singletons {
		// 选项参数
		public static ServiceUpdateOption Option { get; set; } = null;

		// 调用类字典，用于指定通过哪个对象来使用接口功能
		public static Dictionary<(string, Type), object> CallerMap { get; } = new Dictionary<(string, Type), object> ();

		// 远程接口指示类，用于指定某个接口有哪些（微）服务函数地址
		public static Dictionary<string, List<(string, int)>> ServiceAddrs { private get; set; } = new Dictionary<string, List<(string, int)>> ();
		public static object ServiceLock { get; } = new object ();
		private static int s_inc = 0;

		// 获取服务地址
		public static (string, int) GetServiceAddr (string _service_name) {
			lock (ServiceLock) {
				if (ServiceAddrs.TryGetValue (_service_name, out var _addrs))
					return _addrs [++s_inc % _addrs.Count];
			}
			throw new KeyNotFoundException ($"服务 {_service_name} 未找到");
		}
	}
}
