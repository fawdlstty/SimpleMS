using Example.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Example.ServiceB.Services {
	public class MyService: IMyService {
		public Task<string> Hello () {
			return Task.FromResult ("hello ServiceB");
		}
	}
}
