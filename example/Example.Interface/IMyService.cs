using Fawdlstty.SimpleMS.Attributes;
using System.Threading.Tasks;

namespace Example.Interface {
	[SimpleMSService]
	public interface IMyService {
		Task<string> Hello ();
	}
}
