using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Example.ServiceA {
	public class Program {
		public static void Main (string [] args) {
			//var _type = typeof (Services.MyService);
			//var _method = _type.GetMethods () [1];
			//var _args = _method.GetGenericArguments ();
			//var _pars = _method.GetParameters ();
			//var _ret_type = _method.ReturnType;
			CreateHostBuilder (args).Build ().Run ();
		}

		public static IHostBuilder CreateHostBuilder (string [] args) =>
			Host.CreateDefaultBuilder (args)
				.ConfigureWebHostDefaults (webBuilder => {
					webBuilder.UseStartup<Startup> ();
				});
	}
}
