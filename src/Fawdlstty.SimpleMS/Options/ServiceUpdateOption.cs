using Fawdlstty.SimpleMS.Private;
using Fawdlstty.SimpleMS.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Fawdlstty.SimpleMS.Options {
	public enum DiscoveryTypeEnum { None, RegCenter, Custom }

	public class ServiceUpdateOption {
		public ServiceUpdateOption (IServiceCollection _services) {
			ServicesCollection = _services;
		}

		/// <summary>
		/// 本地服务端口，当对外提供服务或作为网关时必填
		/// </summary>
		public int LocalPort { get; set; } = 0;

		// 公共属性，服务发现方式
		internal DiscoveryTypeEnum DiscoveryType { get; private set; } = DiscoveryTypeEnum.None;


		// 以下几个为注册中心服务发现专用
		internal TimeSpan RefreshTime { get; private set; } = TimeSpan.FromSeconds (10);
		internal TimeSpan Timeout { get; private set; } = TimeSpan.FromSeconds (1);
		internal List<(string, int)> GatewayAddrs { get; } = new List<(string, int)> ();

		// 以下几个为自定义服务发现专用
		internal Func<string, (string, int)> GetServiceAddr { get; private set; } = null;

		// 以下几个为 swagger 文档专用
		internal IServiceCollection ServicesCollection { get; private set; } = null;

		/// <summary>
		/// 设置注册中心方式的服务发现
		/// </summary>
		/// <param name="refresh_time">刷新服务时间</param>
		/// <param name="timeout">刷新服务超时时间</param>
		/// <param name="gateway_addrs">网关地址，一般只需要一个</param>
		public void SetRegCenterDiscovery (TimeSpan refresh_time, TimeSpan timeout, params (string, int) [] gateway_addrs) {
			if (DiscoveryType != DiscoveryTypeEnum.None)
				throw new MethodAccessException ("不能指定两次服务发现");
			DiscoveryType = DiscoveryTypeEnum.RegCenter;
			RefreshTime = refresh_time;
			Timeout = timeout;
			GatewayAddrs.AddRange (gateway_addrs);
		}

		/// <summary>
		///  设置自定义服务发现（比如DNS服务发现、环境变量等）
		/// </summary>
		/// <remarks>示例服务名称：“ExampleProject.IMyService:a385b0b48e760ed8e8167e24141fbbe4”、“ExampleProject.IMyService:”
		/// ExampleProject 为服务接口的命名空间
		/// IMyService 为服务接口名称
		/// a385b0b48e760ed8e8167e24141fbbe4 为服务hash。如果有增删改方法或参数、参数顺序有调整，那么这个值会改变，也可能没有这个值</remarks>
		/// <param name="get_service_addr">通过服务名称获取服务地址</param>
		public void SetCustomDiscovery (Func<string, (string, int)> get_service_addr) {
			if (DiscoveryType != DiscoveryTypeEnum.None)
				throw new MethodAccessException ("不能指定两次服务发现");
			DiscoveryType = DiscoveryTypeEnum.Custom;
			GetServiceAddr = get_service_addr;
		}

		/// <summary>
		/// 添加 swagger 文档
		/// </summary>
		/// <param name="url">接口地址路径，例如http://127.0.0.1:5000</param>
		public void AddSwagger (string url) {
			ServicesCollection.AddSwaggerGen ((c) => {
				// TODO: 补充完整
				//c.SwaggerDoc ("web", new OpenApiInfo { Title = "清玖后台服务接口 - Web后台", Version = "web" });
				//c.SwaggerDoc ("wx", new OpenApiInfo { Title = "清玖后台服务接口 - 微信后台", Version = "wx" });
				foreach (var _xml_file in PathMethods.GetAllAssemblyXmlFileNames ()) {
					c.IncludeXmlComments (_xml_file, true);
				}
				//c.IgnoreObsoleteActions ();
				c.AddSecurityDefinition ("Bearer", new OpenApiSecurityScheme {
					Description = "权限认证",
					Name = "Authorization",
					In = ParameterLocation.Header,
					Type = SecuritySchemeType.ApiKey,
					Scheme = JwtBearerDefaults.AuthenticationScheme,
				});
				c.OperationFilter<SecurityOperationFilter> ();
			});
		}
	}
}
