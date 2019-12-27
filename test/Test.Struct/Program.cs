using System;

namespace Test.Struct {
	class Program {
		static void Main (string [] args) {
			var _type = typeof (System.Collections.Generic.List<int>);
			var _methods = _type.GetMethods ();
			Console.WriteLine ("Hello World!");
		}
	}
}
