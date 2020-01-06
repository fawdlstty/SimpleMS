using Fawdlstty.SimpleMS.Attributes;
using System.Threading.Tasks;

namespace Example.Interface {
	/// <summary>
	/// 测试服务提供类
	/// </summary>
	[SimpleMSService]
	public interface IMyService {
		/// <summary>
		/// 测试方法，返回 hello
		/// </summary>
		/// <returns></returns>
		Task<string> Hello ();
	}
}
