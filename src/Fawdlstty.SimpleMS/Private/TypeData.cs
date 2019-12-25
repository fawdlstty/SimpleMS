using Fawdlstty.SimpleMS.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Fawdlstty.SimpleMS.Private {
	public class TypeData {
		public static void InitInterfaces () {
			// 枚举所有接口
			foreach (var _type in Assembly.GetExecutingAssembly ().GetTypes ()) {
				var _type_attr = _type.GetCustomAttribute<ServiceMethodAttribute> ();
				if (_type_attr == null)
					continue;
				if (!_type.IsInterface)
					throw new TypeLoadException ("具有 [ServiceMethod] 标注的类必须为接口类型");

				// 降级处理函数列表
				var _degradations = new List<MethodInfo> ();

				// 创建类生成器
				var _type_builder = _create_type_builder (_type.FullName, false);

				// 循环新增新的函数处理
				_create_constructor (_type_builder, null);
				foreach (var _method_info in _type.GetMethods ()) {
					_add_transcall_method (_type_builder, _method_info);

					// 判断是否指定降级处理函数
					if (_method_info.GetCustomAttribute<ServiceDegradationAttribute> () != null) {
						_degradations.Add (_method_info);
					}
				}
				var _impl_type = _type_builder.CreateType ();

				// 如果类里面某个函数包含降级，那么需要创建子类
				TypeBuilder _child_type_builder = null;
				if (_degradations.Any ()) {
					// 创建降级处理类
					_child_type_builder = _create_type_builder (_type.FullName, true);

					// 创建降级处理方法
					_create_constructor (_child_type_builder, _impl_type);
					foreach (var _method_info_interface in _degradations) {
						// 找到实际需降级函数
						MethodInfo _method_info = null;
						var _methods = (from p in _impl_type.GetMethods () where p.Name == _method_info_interface.Name select p);
						if (_methods.Count () > 1) {
							// 存在同名函数，根据参数再次筛选
							_methods = (from p in _methods where p.GetParameters ()?.Length == _method_info_interface.GetParameters ()?.Length && p.ReturnType == _method_info_interface.ReturnType select p);
							if (_methods.Count () > 1) {
								// 存在参数数量及返回类型相同的函数，根据参数类型再次筛选
								foreach (var _tmp_method in _methods) {
									var _params1 = (from p in _method_info_interface.GetParameters () select p.ParameterType).ToArray ();
									var _params2 = (from p in _tmp_method.GetParameters () select p.ParameterType).ToArray ();
									bool _match = true;
									for (int i = 0; i < _params1.Length; ++i) {
										if (_params1 [i] != _params2 [i]) {
											_match = false;
											break;
										}
									}
									if (_match) {
										_method_info = _tmp_method;
										break;
									}
								}
							}
						}
						if (_method_info == null)
							_method_info = _methods.First ();

						// 创建降级处理函数
						var _deg_func = _method_info.GetCustomAttribute<ServiceDegradationAttribute> ().DegradationFunc;
						_create_deg_method (_child_type_builder, _method_info, _deg_func);
					}
				}

				object _impl_o = Activator.CreateInstance (_impl_type);
			}
		}

		// 创建类型生成器
		private static TypeBuilder _create_type_builder (string _full_name, bool _is_deg_class) {
			string _prefix = (_is_deg_class ? "child" : ""), _name = _full_name.Replace ('.', '_');
			var _assembly_name = new AssemblyName ($"_faw_{_prefix}_assembly__{_name}_");
			var _assembly_builder = AssemblyBuilder.DefineDynamicAssembly (_assembly_name, AssemblyBuilderAccess.Run);
			var _module_builder = _assembly_builder.DefineDynamicModule ($"_faw_{_prefix}_module__{_name}_");
			return _module_builder.DefineType ($"_faw_{_prefix}_type__{_name}_", TypeAttributes.Public | TypeAttributes.Class);
		}

		// 创建构造函数
		private static void _create_constructor (TypeBuilder _child_type_builder, Type _parent_impl_type) {
			ConstructorInfo _constructor_info = null;
			if (_parent_impl_type != null) {
				var _constructor_infos = (from p in _parent_impl_type.GetConstructors () where (p.GetParameters ()?.Length ?? 0) == 0 select p);
				if (!_constructor_infos.Any ())
					throw new MethodAccessException ("继承至 [ServiceMethod] 接口的类必须具有无参构造函数");
				_constructor_info = _constructor_infos.First ();
			}
			var _constructor_builder = _child_type_builder.DefineConstructor (MethodAttributes.Public, CallingConventions.Standard, Array.Empty<Type> ());
			var _il_generator = _constructor_builder.GetILGenerator();
			//_il_generator.Emit (OpCodes.Ldarg_0);
			//for (int i = 1; i <= baseConstructorInfo.GetParameters ().Length; i++) {
			//	_il_generator.Emit (OpCodes.Ldarg_S, i);
			//}
			if (_constructor_info != null)
				_il_generator.Emit (OpCodes.Call, _constructor_info);
			_il_generator.Emit (OpCodes.Ret);
		}

		// TODO:创建中转函数
		private static void _add_transcall_method (TypeBuilder _type_builder, MethodInfo _parent_method_info) {
			var _method_attr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
			var _param_types = (from p in _parent_method_info.GetParameters () select p.ParameterType).ToArray ();
			var _method_builder = _type_builder.DefineMethod (_parent_method_info.Name, _method_attr, CallingConventions.Standard, _parent_method_info.ReturnType, _param_types);
			_type_builder.DefineMethodOverride (_method_builder, _parent_method_info);
			var _il_generator = _method_builder.GetILGenerator ();
			// TODO:
			_il_generator.Emit (OpCodes.Ldstr, "hello");
			_il_generator.Emit (OpCodes.Ret);
		}

		// TODO:创建降级函数
		// try {
		//     return super.xxx ();
		// } catch (Exception) {
		//     return DegradationFunc ();
		//}
		private static void _create_deg_method (TypeBuilder _type_builder, MethodInfo _parent_method_info, object _func) {
			var _method_attr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
			var _param_types = (from p in _parent_method_info.GetParameters () select p.ParameterType).ToArray ();
			var _method_builder = _type_builder.DefineMethod (_parent_method_info.Name, _method_attr, CallingConventions.Standard, _parent_method_info.ReturnType, _param_types);
			_type_builder.DefineMethodOverride (_method_builder, _parent_method_info);
			var _il_generator = _method_builder.GetILGenerator ();
			// try {
			_il_generator.BeginExceptionBlock ();

			// } catch (Exception) {       ????
			_il_generator.BeginCatchBlock (typeof (Exception));
			var _exception = _il_generator.DeclareLocal (typeof (Exception));
			_il_generator.Emit (OpCodes.Stloc, _exception);

			// }
			_il_generator.EndExceptionBlock ();


			_il_generator.Emit (OpCodes.Ret);
		}

		// 创建只读属性
		private static void _add_readonly_property (TypeBuilder _type_builder, string _prop_name, string _prop_value) {
			var _method_attr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual;
			var _method_builder = _type_builder.DefineMethod ($"get_{_prop_name}" + _prop_name, _method_attr, typeof (string), Type.EmptyTypes);
			var _il_generator = _method_builder.GetILGenerator ();
			if (_prop_value == null) {
				_il_generator.Emit (OpCodes.Ldnull);
			} else {
				_il_generator.Emit (OpCodes.Ldstr, "hello");
			}
			_il_generator.Emit (OpCodes.Ret);
			//
			var _prop_builder = _type_builder.DefineProperty (_prop_name, PropertyAttributes.None, typeof (string), Type.EmptyTypes);
			_prop_builder.SetGetMethod (_method_builder);
		}
	}
}
