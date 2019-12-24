﻿using Fawdlstty.SimpleMS.Options;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace Fawdlstty.SimpleMS.Client {
	public static class InjectionClient {
		private static bool s_init = true;

		public static IServiceCollection AddSimpleMSClient (this IServiceCollection services, Action<ServiceUpdateOption> option) {
			if (s_init) {
				s_init = false;
			} else {
				throw new NotSupportedException ("请确保 services.AddSimpleMSClient () 与 services.AddSimpleMSService () 一共只被调用一次");
			}

			var _option = new ServiceUpdateOption ();
			option?.Invoke (_option);
			if (_option.GatewayAddrs.Count == 0)
				throw new ArgumentException ("网关");

			// TODO: 枚举接口
			Assembly.GetExecutingAssembly ().GetTypes ()
			// TODO: 创建访问对象
			return services;
		}
	}
}
