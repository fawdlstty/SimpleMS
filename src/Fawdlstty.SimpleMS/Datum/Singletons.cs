using Fawdlstty.SimpleMS.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fawdlstty.SimpleMS.Datum {
	internal class Singletons {
		// 选项参数
		public static ServiceUpdateOption Option { get; set; } = null;

		// 调用类字典，用于指定通过哪个对象来使用接口功能
		public static Dictionary<Type, object> InterfaceMap { get; } = new Dictionary<Type, object> ();

		// 远程接口指示类，用于指定某个接口有哪些（微）服务函数地址
		public static Dictionary<string, List<(string, int)>> ServerAddrs { get; set; } = new Dictionary<string, List<(string, int)>> ();
	}
}
