using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Test.Struct {
	public class TestCaller {
		private List<Func<Dictionary<string, object>, Type, object>> _deg_func = new List<Func<Dictionary<string, object>, Type, object>> ();

		public string trans_paramN (int n) {
			var _param = new Dictionary<string, object> ();
			_param.Add ("n", n);
			Func<Dictionary<string, object>, Type, object> _deg = _deg_func [n];
			return (string) TestMethod.TestFunc (_param, _deg, typeof (string));
		}
	}
}
