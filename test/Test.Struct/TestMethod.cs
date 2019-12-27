using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Test.Struct {
	public class TestMethod {
		public static object BaseFunc (Dictionary<string, object> _param, Type _ret_type) {
			return (object) "hello world";
		}
		public static object TestFunc (Dictionary<string, object> _param, Type _ret_type) {
			return Task.FromResult ((object) "hello world");
		}
		public static object TestFunc (Dictionary<string, object> _param, Func<Dictionary<string, object>, Type, object> _deg_func, Type _type) {
			return Task.FromResult ((object) "hello world");
		}
	}
}
