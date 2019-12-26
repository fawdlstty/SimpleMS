using Fawdlstty.SimpleMS.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Fawdlstty.SimpleMS.Private {
	public class ImplTypeBuilder {
		// TODO:
		public static void InitInterfaces () {
			// 枚举所有接口
			foreach (var _type in Assembly.GetExecutingAssembly ().GetTypes ()) {
				var _type_attr = _type.GetCustomAttribute<ServiceMethodAttribute> ();
				if (_type_attr == null)
					continue;
				if (!_type.IsInterface)
					throw new TypeLoadException ("具有 [ServiceMethod] 标注的类必须为接口类型");

				// 创建类生成器
				string _name = _type.FullName.Replace ('.', '_');
				var _assembly_name = new AssemblyName ($"_faw_assembly__{_name}_");
				var _assembly_builder = AssemblyBuilder.DefineDynamicAssembly (_assembly_name, AssemblyBuilderAccess.Run);
				var _module_builder = _assembly_builder.DefineDynamicModule ($"_faw_module__{_name}_");
				var _type_builder = _module_builder.DefineType ($"_faw_type__{_name}_", TypeAttributes.Public | TypeAttributes.Class);

				// 创建构造函数

				// 循环新增新的函数处理
				_create_constructor (_type_builder, null);
				_type_builder.AddInterfaceImplementation (_type);
				foreach (var _method_info in _type.GetMethods ()) {
					var _deg_func = _method_info.GetCustomAttribute<ServiceDegradationAttribute> ()?.DegradationFunc;
					_add_transcall_method (_type_builder, _method_info, _deg_func);
				}
				var _impl_type = _type_builder.CreateType ();

				object _impl_o = Activator.CreateInstance (_impl_type);
			}
		}

		// 创建构造函数
		private static void _create_constructor (TypeBuilder _type_builder, Type _parent_impl_type) {
			ConstructorInfo _constructor_info = null;
			if (_parent_impl_type != null) {
				var _constructor_infos = (from p in _parent_impl_type.GetConstructors () where (p.GetParameters ()?.Length ?? 0) == 0 select p);
				if (!_constructor_infos.Any ())
					throw new MethodAccessException ("继承至 [ServiceMethod] 接口的类必须具有无参构造函数");
				_constructor_info = _constructor_infos.First ();
			}
			var _constructor_builder = _type_builder.DefineConstructor (MethodAttributes.Public, CallingConventions.Standard, Array.Empty<Type> ());
			var _code = _constructor_builder.GetILGenerator ();
			if (_constructor_info != null)
				_code.Emit (OpCodes.Call, _constructor_info);
			_code.Emit (OpCodes.Ret);
		}

		// TODO:创建中转函数
		private static void _add_transcall_method (TypeBuilder _type_builder, MethodInfo _parent_method_info, Func<Dictionary<string, object>, Type, object> _deg_func) {
			var _method_attr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
			var _param_types = (from p in _parent_method_info.GetParameters () select p.ParameterType).ToArray ();
			var _method_builder = _type_builder.DefineMethod (_parent_method_info.Name, _method_attr, CallingConventions.Standard, _parent_method_info.ReturnType, _param_types);
			_type_builder.DefineMethodOverride (_method_builder, _parent_method_info);
			var _code = _method_builder.GetILGenerator ();
			//var _bytes = typeof (TypeData).GetMethod ("_add_transcall_method").GetMethodBody ().GetILAsByteArray ();
			// TODO:

			// 参数0：参数列表
			var _localvar_param_values = _code.DeclareLocal (typeof (Dictionary<string, object>));
			_code.Emit (OpCodes.Newobj, typeof (Dictionary<string, object>).GetConstructor (Type.EmptyTypes));
			_code.Emit (OpCodes.Stloc, _localvar_param_values);
			var _param_add = typeof(IDictionary<string, object>).GetMethod("Add", new Type[] { typeof(string), typeof(object) });
			foreach (var _param_info in _parent_method_info.GetParameters ()) {
				var _field = _type_builder.DefineField ($"_", _param_info.ParameterType, FieldAttributes.Private);
				_code.Emit (OpCodes.Ldarg_0);
				_code.Emit (OpCodes.Ldloc, _localvar_param_values);
				_code.Emit (OpCodes.Ldstr, _param_info.Name);
				_code.Emit (OpCodes.Ldfld, _field);
				if (_field.FieldType.IsValueType)
					_code.Emit (OpCodes.Box, _field.FieldType);
				_code.Emit (OpCodes.Callvirt, _param_add);
			}

			// 转发请求并返回
			_code.Emit (OpCodes.Ldarg_0);
			_code.Emit (OpCodes.Ldloc, _localvar_param_values);
			_code.Emit (OpCodes.Ret);
		}

		// 创建只读属性
		private static void _add_readonly_property (TypeBuilder _type_builder, string _prop_name, string _prop_value) {
			var _method_attr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual;
			var _method_builder = _type_builder.DefineMethod ($"get_{_prop_name}" + _prop_name, _method_attr, typeof (string), Type.EmptyTypes);
			var _code = _method_builder.GetILGenerator ();
			if (_prop_value == null) {
				_code.Emit (OpCodes.Ldnull);
			} else {
				_code.Emit (OpCodes.Ldstr, "hello");
			}
			_code.Emit (OpCodes.Ret);
			//
			var _prop_builder = _type_builder.DefineProperty (_prop_name, PropertyAttributes.None, typeof (string), Type.EmptyTypes);
			_prop_builder.SetGetMethod (_method_builder);
		}
	}
}
