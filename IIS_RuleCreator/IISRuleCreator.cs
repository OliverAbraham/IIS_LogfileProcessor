using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abraham.Web
{
	public class IISRuleCreator
	{
		#region ------------- Properties ----------------------------------------------------------

		public string WebSiteName { get; set; }

		#endregion



		#region ------------- Fields --------------------------------------------------------------
		#endregion



		#region ------------- Init ----------------------------------------------------------------

		public IISRuleCreator()
		{
			WebSiteName = "Default Web Site";
		}

		#endregion



		#region ------------- Methods -------------------------------------------------------------

		public List<Rule> ReadAllRules()
		{
			var rules = new	List<Rule>();

			using (var manager = new ServerManager())
			{
				var collection = GetSecurityCollection(manager);

				foreach (ConfigurationElement element in collection)
				{
					var rule        = new Rule();
					rule.IP         = Convert.ToString ( element["ipAddress" ]);
					rule.SubnetMask = Convert.ToString ( element["subnetMask"]);
					rule.Allowed    = Convert.ToBoolean( element["allowed"   ]);
					rules.Add(rule);
				}
			}

			return rules;
		}

		public void AddRule(string ip, string subnetMask, bool allowed)
		{
			using (var manager = new ServerManager())
			{
				var collection = GetSecurityCollection(manager);

				ConfigurationElement addElement = collection.CreateElement("add");
				addElement["ipAddress"]  = ip;
				addElement["subnetMask"] = subnetMask;
				addElement["allowed"]    = allowed;
				collection.Add(addElement);

				manager.CommitChanges();
			}
		}

		public void DeleteAllRules()
		{
			using (var manager = new ServerManager())
			{
				var collection = GetSecurityCollection(manager);
				collection.Clear();
				manager.CommitChanges();
			}
		}

		#endregion



		#region ------------- Implementation ------------------------------------------------------

		private ConfigurationElementCollection GetSecurityCollection(ServerManager serverManager)
		{
			Configuration config = serverManager.GetApplicationHostConfiguration();
			ConfigurationSection ipSecuritySection = config.GetSection("system.webServer/security/ipSecurity", WebSiteName);
			ConfigurationElementCollection ipSecurityCollection = ipSecuritySection.GetCollection();
			return ipSecurityCollection;
		}

		#endregion
	}
}
