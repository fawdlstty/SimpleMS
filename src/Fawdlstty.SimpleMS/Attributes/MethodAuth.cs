using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fawdlstty.SimpleMS.Attributes {
	public enum Role { none, logging, user, admin }

	public class MethodAuthAttribute: Attribute {
		internal Role [] m_roles { get; private set; } = Array.Empty<Role> ();

		public MethodAuthAttribute (params Role [] roles) {
			m_roles = roles;
		}

		public string Policy {
			get {
				if ((m_roles?.Length ?? 0) == 0) {
					return "【任意具有会话的用户均可调用】";
				} else {
					var _allow_users = (from p in m_roles select p switch
					{
						Role.admin => "管理员",
						Role.user => "普通用户",
						Role.logging => "待登录用户",
						_ => "未知用户",
					}).ToList ();
					return $"【仅允许{string.Join ("，", _allow_users)}调用】";
				}
			}
		}
	}
}
