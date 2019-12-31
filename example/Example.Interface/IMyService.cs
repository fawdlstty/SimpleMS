using Fawdlstty.SimpleMS.Attributes;
using System.Threading.Tasks;

namespace Example.Interface {
	[ServiceMethod]
	public interface IMyService {
		Task<string> Hello ();
	}
}
