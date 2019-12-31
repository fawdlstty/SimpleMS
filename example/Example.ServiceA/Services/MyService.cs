using Example.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Example.ServiceA.Services {
	public class MyService: IMyService {
		public Task<string> Hello () {
			return Task.FromResult ("hello ServiceA");
		}
		//public void Hello1 (string hello) {
		//}
	}
}
