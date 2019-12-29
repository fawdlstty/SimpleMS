using Fawdlstty.SimpleMS.Options;
using Fawdlstty.SimpleMS.Private;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fawdlstty.SimpleMS {
	public static class Injection {
		public static IServiceCollection AddSimpleMS (this IServiceCollection services, Action<ServiceUpdateOption> option) {
			var _option = new ServiceUpdateOption ();
			option?.Invoke (_option);
			if (_option.GatewayAddrs.Count == 0)
				throw new ArgumentException ("网关数量不可为0");

			ImplTypeBuilder.InitInterfaces (_option);
			return services;
		}
	}
}
