using Fawdlstty.SimpleMS.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fawdlstty.SimpleMS.Service {
	public static class InjectionService {
		public static IServiceCollection AddSimpleMSService (this IServiceCollection services) {
			// 初始化注册列表
			services.AddSimpleMSClient ((option) => {

			});

			// TODO: 枚举实例类并创建中转对象


			return services;
		}
	}
}
