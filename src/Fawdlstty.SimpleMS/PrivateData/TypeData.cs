using Fawdlstty.SimpleMS.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Fawdlstty.SimpleMS.PrivateData {
	public class TypeData {
		public static void InitInterfaces () {
			// 枚举所有接口
			foreach (var _type in Assembly.GetExecutingAssembly ().GetTypes ()) {
				var _type_attr = _type.GetCustomAttribute<ServiceMethodAttribute> ();
				if (_type_attr == null)
					continue;
				if (!_type.IsInterface)
					throw new TypeLoadException ("具有 [ServiceMethod] 标注的类必须为接口类型");

				// 创建访问对象
				var _name = _type.FullName.Replace ('.', '_');
				var _assembly_name = new AssemblyName ($"_faw_assembly__{_name}_");
				var _assembly_builder = AssemblyBuilder.DefineDynamicAssembly (_assembly_name, AssemblyBuilderAccess.Run);
				var _module_builder = _assembly_builder.DefineDynamicModule ($"_faw_module__{_name}_");
				var _type_builder = _module_builder.DefineType ($"_type__{_name}_", TypeAttributes.Public | TypeAttributes.Class);
				foreach (var _method_info in _type.GetMethods ()) {
					var _param_types = (from p in _method_info.GetParameters () select p.ParameterType).ToArray ();
					var _method_builder = _type_builder.DefineMethod (_method_info.Name, MethodAttributes.Public | MethodAttributes.Virtual, _method_info.ReturnType, _param_types);
					var _il_generator = _method_builder.GetILGenerator ();
					_il_generator.Emit (OpCodes.Ldstr, "hello");
					_il_generator.Emit (OpCodes.Ret);
				}
				var _impl_type = _type_builder.CreateType ();
				object _impl_o = Activator.CreateInstance (_impl_type);
			}
		}
		//public static Type [] AllTypes { get; set; } = Array.Empty<Type> ();
	}
}
