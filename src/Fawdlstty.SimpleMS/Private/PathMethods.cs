using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Fawdlstty.SimpleMS.Private {
	internal class PathMethods {
		public static List<string> GetAllAssemblyFileNames () {
			if (m_asm_files == null) {
				var _ignores_pattern = string.Format ("^Microsoft\\.\\w*|^System\\.\\w*|^Newtonsoft\\.\\w*");
				Regex _ignores = new Regex (_ignores_pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
				m_asm_files = Directory.GetFiles (LocalPath, "*.dll").Select (Path.GetFullPath).Where (a => !_ignores.IsMatch (Path.GetFileName (a))).ToList ();
			}
			return m_asm_files;
		}

		public static List<Assembly> GetAllAssemblys () {
			if (s_asms == null) {
				s_asms = new List<Assembly> ();
				var _files = PathMethods.GetAllAssemblyFileNames ();
				foreach (var _file in _files) {
					try {
						var _asm = Assembly.LoadFrom (_file);
						if (!s_asms.Contains (_asm)) {
							s_asms.Add (_asm);
						}
					} catch (Exception) {
					}
				}
			}
			return s_asms;
		}

		public static List<Type> GetAllTypes () {
			if (s_types == null) {
				s_types = new List<Type> ();
				var _asms = GetAllAssemblys ();
				foreach (var _asm in _asms)
					s_types.AddRange (_asm.GetTypes ());
			}
			return s_types;
		}

		public static string LocalPath { get { return Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location); } }
		private static List<string> m_asm_files = null;
		private static List<Assembly> s_asms = null;
		private static List<Type> s_types = null;
	}
}
