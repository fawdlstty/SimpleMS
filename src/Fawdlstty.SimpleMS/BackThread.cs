using Fawdlstty.SimpleMS.Datum;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fawdlstty.SimpleMS {
	/// <summary>
	/// 此类用于创建后台线程同步本地公共模块
	/// </summary>
	internal class BackThread {
		public static void Start () {
			if (s_task != null)
				return;
			s_task = Task.Run (() => {
				while (true) {
					try {

					} catch (Exception) {
					}
					Task.Delay (Singletons.Option.RefreshTime);
				}
			});
		}

		private static Task s_task = null;
	}
}
