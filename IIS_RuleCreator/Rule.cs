using System;
using System.Collections.Generic;
using System.Text;

namespace Abraham.Web
{
	public class Rule
	{
		public string IP            { get; set; }
		public string SubnetMask    { get; set; }
		public bool   Allowed		{ get; set; }
		public string AllowedDescription { get { return Allowed ? "allow" : "deny"; } }

		public override string ToString()
		{
			return $"{AllowedDescription} {IP:15} {SubnetMask,15}";
		}
	}
}
