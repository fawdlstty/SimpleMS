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

namespace Example.Client {
	public class Startup {
		public Startup (IConfiguration configuration) {
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices (IServiceCollection services) {
			services.AddControllers ();
			services.AddSimpleMS ((_option) => {
				_option.SetRegCenterDiscovery (TimeSpan.FromSeconds (10), TimeSpan.FromSeconds (1), ("127.0.0.1", 4455));
				//_option.SetCustomDiscovery ((_service_name) => {
				//	_service_name = _service_name.Substring (0, _service_name.IndexOf (':')).ToUpper ().Replace (".", "_");
				//	string _ret = Environment.GetEnvironmentVariable (_service_name) ?? ":0";
				//	int _split = _ret.IndexOf (':');
				//	return (_ret.Substring (0, _split), int.Parse (_ret.Substring (_split + 1)));
				//});
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure (IApplicationBuilder app, IWebHostEnvironment env) {
			if (env.IsDevelopment ()) {
				app.UseDeveloperExceptionPage ();
			}

			app.UseRouting ();

			app.UseAuthorization ();

			app.UseEndpoints (endpoints => {
				endpoints.MapControllers ();
			});
		}
	}
}
