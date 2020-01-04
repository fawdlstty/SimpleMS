using Fawdlstty.SimpleMS.Attributes;
using Fawdlstty.SimpleMS.Datum;
using Fawdlstty.SimpleMS.Options;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fawdlstty.SimpleMS.Private {
	internal class ImplTypeBuilder {
		// 初始化接口信息
		public static (List<string>, List<string>) InitInterfaces (IServiceCollection services) {
			List<string> _local = new List<string> (), _remote = new List<string> ();

			// 枚举所有接口
			var _types = _get_all_types ();
			foreach (var _type in _types) {
				var _type_attr = _type.GetCustomAttribute<SimpleMSServiceAttribute> ();
				if (_type_attr == null)
					continue;
				if (!_type.IsInterface)
					throw new TypeLoadException ("具有 [SimpleMSService] 标注的类必须为接口类型");

				// 获取接口服务名称
				string _name = _type.FullName.Replace ('.', '_');
				string _service_name = _type.GetServiceName ();

				// 如果需要，那么搜索本地模块
				var _type_impls = (from p in _types where p.GetInterface (_type.Name) == _type select p);
				if (_type_impls.Count () > 1)
					throw new TypeLoadException ("实现 [SimpleMSService] 标注的接口的类最多只能有一个");
				object _impl_o = null;
				if (_type_impls.Any ()) {
					// 从本地模块加载
					_local.Add (_service_name);

					// 创建实例
					_impl_o = Activator.CreateInstance (_type_impls.First ());
				} else {
					// 从外部模块加载
					_remote.Add (_service_name);

					// 创建类生成器
					var _assembly_name = new AssemblyName ($"_faw_assembly__{_name}_");
					var _assembly_builder = AssemblyBuilder.DefineDynamicAssembly (_assembly_name, AssemblyBuilderAccess.Run);
					var _module_builder = _assembly_builder.DefineDynamicModule ($"_faw_module__{_name}_");
					var _type_builder = _module_builder.DefineType ($"_faw_type__{_name}_", TypeAttributes.Public | TypeAttributes.Class);

					// 创建构造函数
					var _constructor_builder = _type_builder.DefineConstructor (MethodAttributes.Public, CallingConventions.Standard, Array.Empty<Type> ());
					_constructor_builder.GetILGenerator ().Emit (OpCodes.Ret);

					// 定义存储降级函数的字段
					var _deg_funcs = new List<Func<Dictionary<string, object>, Type, Exception, Task>> ();
					var _deg_field = _type_builder.DefineField ("_faw_field__deg_funcs_", typeof (List<Func<Dictionary<string, object>, Type, Exception, Task>>), FieldAttributes.Public);

					// 定义存储返回类型的字段
					var _return_types = new List<Type> ();
					var _return_field = _type_builder.DefineField ("_faw_field__return_types_", typeof (List<Type>), FieldAttributes.Public);

					// 循环新增新的函数处理
					_type_builder.AddInterfaceImplementation (_type);
					var _method_infos = _type.GetMethods ();
					for (int i = 0; i < _method_infos.Length; ++i) {
						// 不允许出现getter和setter
						if (_method_infos [i].Name.StartsWith ("get_") || _method_infos [i].Name.StartsWith ("set_")) {
							throw new TypeLoadException ("具有 [SimpleMSService] 标注的接口不允许出现 getter/setter 函数 （函数名不允许 get_/set_ 开头）");
						}

						// 只提供异步函数
						if (_method_infos [i].ReturnType != typeof (Task) && _method_infos [i].ReturnType.BaseType != typeof (Task)) {
							throw new TypeLoadException ($"具有 [SimpleMSService] 标注的接口函数 {_method_infos [i].Name} 返回类型非 Task");
						}

						// 将回调函数与返回类型函数
						var _deg_func = _method_infos [i].GetCustomAttribute<ServiceDegradationAttribute> ()?.DegradationFunc;
						_deg_funcs.Add (_deg_func);
						_return_types.Add (_method_infos [i].ReturnType);
						_add_transcall_method (_service_name, _type_builder, _method_infos [i], _deg_field, _return_field, i);
					}

					// 创建实例
					var _impl_type = _type_builder.CreateType ();
					_impl_o = Activator.CreateInstance (_impl_type);
					_impl_type.InvokeMember ("_faw_field__deg_funcs_", BindingFlags.SetField, null, _impl_o, new [] { _deg_funcs });
					_impl_type.InvokeMember ("_faw_field__return_types_", BindingFlags.SetField, null, _impl_o, new [] { _return_types });
				}

				// 添加进处理对象
				services.AddSingleton (_type, _impl_o);
				Singletons.CallerMap.Add ((_service_name, _type), _impl_o);
			}

			return (_local, _remote);
		}

		// 获取所有接口
		private static List<Type> _get_all_types () {
			var _path = Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location);
			var _ignores_pattern = string.Format ("^Microsoft\\.\\w*|^System\\.\\w*|^Newtonsoft\\.\\w*");
			Regex _ignores = new Regex (_ignores_pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
			var _files = Directory.GetFiles (_path, "*.dll").Select (Path.GetFullPath).Where (a => !_ignores.IsMatch (Path.GetFileName (a))).ToList ();
			bool _change = false;
			foreach (var _file in _files) {
				try {
					var _asm = Assembly.LoadFrom (_file);
					if (!s_asms.Contains (_asm)) {
						_change = true;
						s_asms.Add (_asm);
						s_types.AddRange (_asm.GetTypes ());
					}
				} catch (Exception) {
				}
			}
			if (_change)
				s_types = s_types.Distinct ().ToList ();
			return s_types;
		}

		private static List<Assembly> s_asms = new List<Assembly> ();
		private static List<Type> s_types = new List<Type> ();

		// 创建中转函数
		private static void _add_transcall_method (string _service_name, TypeBuilder _type_builder, MethodInfo _method_info, FieldBuilder _field_deg_funcs, FieldBuilder _field_return_types, int _index) {
			var _method_attr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
			var _param_types = (from p in _method_info.GetParameters () select p.ParameterType).ToArray ();
			var _method_builder = _type_builder.DefineMethod (_method_info.Name, _method_attr, CallingConventions.Standard, _method_info.ReturnType, _param_types);
			_type_builder.DefineMethodOverride (_method_builder, _method_info);
			var _code = _method_builder.GetILGenerator ();

			// 参数2：参数列表
			var _map = _code.DeclareLocal (typeof (Dictionary<string, object>));
			_code.Emit (OpCodes.Newobj, typeof (Dictionary<string, object>).GetConstructor (Type.EmptyTypes));
			_code.Emit (OpCodes.Stloc, _map);
			var _param_add = typeof (IDictionary<string, object>).GetMethod ("Add", new Type [] { typeof (string), typeof (object) });
			var _param_infos = _method_info.GetParameters ();
			int _param_hash = _method_info.GetHashCode ();
			for (int i = 0; i < _param_infos.Length; ++i) {
				_code.Emit (OpCodes.Ldloc, _map);
				_code.Emit (OpCodes.Ldstr, _param_infos [i].Name);
				if (i == 0) {
					_code.Emit (OpCodes.Ldarg_1);
				} else if (i == 1) {
					_code.Emit (OpCodes.Ldarg_2);
				} else if (i == 2) {
					_code.Emit (OpCodes.Ldarg_3);
				} else if (i <= 126) {
					_code.Emit (OpCodes.Ldarg_S, (byte) i + 1);
				} else {
					_code.Emit (OpCodes.Ldarg, i + 1);
				}
				if (_param_infos [i].ParameterType.IsValueType)
					_code.Emit (OpCodes.Box, _param_infos [i].ParameterType);
				_code.EmitCall (OpCodes.Callvirt, _param_add, null);
				_param_hash ^= _param_infos [i].ParameterType.GetHashCode ();
			}

			// 参数3：降级函数
			var _deg_func = _code.DeclareLocal (typeof (Func<Dictionary<string, object>, Type, object>));
			_code.Emit (OpCodes.Ldarg_0);
			_code.Emit (OpCodes.Ldfld, _field_deg_funcs);
			_emit_fast_int (_code, _index);
			var _list_get_Item_deg_func = typeof (List<Func<Dictionary<string, object>, Type, Exception, object>>).GetMethod ("get_Item");
			_code.EmitCall (OpCodes.Callvirt, _list_get_Item_deg_func, null);
			_code.Emit (OpCodes.Stloc, _deg_func);

			// 参数4：返回类型
			var _return_type = _code.DeclareLocal (typeof (Type));
			_code.Emit (OpCodes.Ldarg_0);
			_code.Emit (OpCodes.Ldfld, _field_return_types);
			_emit_fast_int (_code, _index);
			var _list_get_Item_return_type = typeof (List<Type>).GetMethod ("get_Item");
			_code.EmitCall (OpCodes.Callvirt, _list_get_Item_return_type, null);
			_code.Emit (OpCodes.Stloc, _return_type);

			// 转发请求并返回
			_code.Emit (OpCodes.Ldstr, _service_name);
			_code.Emit (OpCodes.Ldstr, _method_info.Name);
			_code.Emit (OpCodes.Ldloc, _map);
			_code.Emit (OpCodes.Ldloc, _deg_func);
			_code.Emit (OpCodes.Ldloc, _return_type);
			var _impl_invoke_method = typeof (ImplCaller).GetMethod ("invoke_method");
			_code.EmitCall (OpCodes.Call, _impl_invoke_method, null);
			_code.Emit (OpCodes.Ret);
		}

		// 发送整型
		private static void _emit_fast_int (ILGenerator _code, int _value) {
			if (_value >= -1 && _value <= 8) {
				switch (_value) {
					case -1:	_code.Emit (OpCodes.Ldc_I4_M1);		break;
					case 0:		_code.Emit (OpCodes.Ldc_I4_0);		break;
					case 1:		_code.Emit (OpCodes.Ldc_I4_1);		break;
					case 2:		_code.Emit (OpCodes.Ldc_I4_2);		break;
					case 3:		_code.Emit (OpCodes.Ldc_I4_3);		break;
					case 4:		_code.Emit (OpCodes.Ldc_I4_4);		break;
					case 5:		_code.Emit (OpCodes.Ldc_I4_5);		break;
					case 6:		_code.Emit (OpCodes.Ldc_I4_6);		break;
					case 7:		_code.Emit (OpCodes.Ldc_I4_7);		break;
					case 8:		_code.Emit (OpCodes.Ldc_I4_8);		break;
				};
			} else if (_value >= -128 && _value <= 127) {
				_code.Emit (OpCodes.Ldc_I4_S, (sbyte) _value);
			} else {
				_code.Emit (OpCodes.Ldc_I4, _value);
			}
		}
	}
}
