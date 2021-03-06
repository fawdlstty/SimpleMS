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

namespace Example.ServiceB {
	public class Startup {
		public Startup (IConfiguration configuration) {
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices (IServiceCollection services) {
			services.AddSimpleMS ((_option) => {
				_option.LocalPort = 5001;
				_option.SetRegCenterDiscovery (TimeSpan.FromSeconds (10), TimeSpan.FromSeconds (1), ("127.0.0.1", 4455));
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure (IApplicationBuilder app, IWebHostEnvironment env) {
			app.UseSimpleMSService ();
		}
	}
}
