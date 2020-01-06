using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fawdlstty.SimpleMS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Example.ServiceA {
	public class Startup {
		public Startup (IConfiguration configuration) {
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices (IServiceCollection services) {
			services.AddSimpleMS ((_option) => {
				_option.LocalPort = 5000;
				_option.SetRegCenterDiscovery (TimeSpan.FromSeconds (10), TimeSpan.FromSeconds (1), ("127.0.0.1", 4455));
				//_option.AddSwagger ("http://127.0.0.1:5000");
			});
			//services.AddSwaggerGen (c => {
			//	c.SwaggerDoc ("web", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "清玖后台服务接口 - Web后台", Version = "web" });
			//	c.SwaggerDoc ("wx", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "清玖后台服务接口 - 微信后台", Version = "wx" });
			//	c.IncludeXmlComments (Path.Combine (Directory.GetCurrentDirectory (), "QingjiuServer3.xml"), true);
			//	//c.IgnoreObsoleteActions ();
			//	c.AddSecurityDefinition ("Bearer", new OpenApiSecurityScheme {
			//		Description = "权限认证",
			//		Name = "Authorization",
			//		In = ParameterLocation.Header,
			//		Type = SecuritySchemeType.ApiKey,
			//		Scheme = JwtBearerDefaults.AuthenticationScheme,
			//	});
			//	c.OperationFilter<SecurityRequirementsOperationFilter> ();
			//});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure (IApplicationBuilder app, IWebHostEnvironment env) {
			app.UseSimpleMSService ();
			//app.UseAuthorization ();
			//app.UseAuthentication ();
			//app.UseSwagger ();
			//app.UseSwaggerUI (c => {
			//	c.RoutePrefix = "doc";
			//	c.SwaggerEndpoint ("/swagger/web/swagger.json", "Web后台接口");
			//	c.SwaggerEndpoint ("/swagger/wx/swagger.json", "微信后台接口");
			//});
		}
	}
}
