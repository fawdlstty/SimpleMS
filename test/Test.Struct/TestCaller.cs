using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Test.Struct {
	public class TestCaller {
		private List<Func<Dictionary<string, object>, Type, object>> _deg_func = new List<Func<Dictionary<string, object>, Type, object>> ();

		private List<string> m_items { get; set; }

		public string trans_paramN (int n) {
			return (string) TestMethod.TestFunc ();
		}
	}
}
