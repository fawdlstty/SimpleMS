using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Example.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Example.Client.Controllers {
	[Route ("api/[controller]")]
	[ApiController]
	public class TestController: ControllerBase {
		private readonly IMyService _service;
		public TestController (IMyService service) {
			_service = service;
		}

		[HttpGet]
		public async Task<string> index () {
			try {
				return await _service.Hello ();
			} catch (Exception ex) {
				return ex.ToString ();
			}
		}
	}
}
