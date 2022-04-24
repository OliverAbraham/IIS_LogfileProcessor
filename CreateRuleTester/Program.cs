using Abraham.Web;
using System;

namespace CreateRuleTester_Fw472
{
	class Program
	{
		static void Main(string[] args)
		{
			ParseArguments(args);
		}

		private static void ParseArguments(string[] args)
		{
			if (args.GetLength(0) == 0)
			{
				Usage();
			}
			else
			{
				string command = args[0];

				if (command == "-list")
				{
					ListAllRules();
				}
				else if (command == "-add")
				{
					if (args.GetLength(0) != 4)
						Console.WriteLine($"Error: not the right arguments. Give three arguments after -add");
					else
						AddRule(args);
				}
				else if (command == "-deleteall")
				{
					DeleteAllRules();
				}
			}
		}

		private static void ListAllRules()
		{
			Console.WriteLine("Listing all existing rules:");
			var creator = new IISRuleCreator();
			var rules = creator.ReadAllRules();
			foreach (var rule in rules)
				Console.WriteLine($"Rule: {rule.IP} {rule.SubnetMask} {rule.Allowed}");
		}

		private static void AddRule(string[] args)
		{
			string ip          = args[1];
			string mask        = args[2];
			bool   allowed     = Convert.ToBoolean(args[3]);
			string allowedDesc = (allowed) ? "Allow" : "Deny";

			Console.WriteLine($"Adding a {allowedDesc} rule for IP {ip} and subnet mask {mask}");

			var creator = new IISRuleCreator();
			creator.AddRule(ip, mask, allowed);
			Console.WriteLine($"success!");
		}

		private static void DeleteAllRules()
		{
			Console.WriteLine($"Deleting all existing rules");
			var creator = new IISRuleCreator();
			creator.DeleteAllRules();
			Console.WriteLine($"success!");
		}

		private static void Usage()
		{
			Console.WriteLine("Create rule test program   -   Oliver Abraham 2019 - mail@oliver-abraham.de");
			Console.WriteLine("");
			Console.WriteLine("Usage:");
			Console.WriteLine("CreateRuleTester_Fw472 -list");
			Console.WriteLine("CreateRuleTester_Fw472 -add [ip] [mask] [true|false]");
			Console.WriteLine("CreateRuleTester_Fw472 -deleteall");
			Console.WriteLine("");
			Console.WriteLine("Examples:");
			Console.WriteLine("CreateRuleTester_Fw472 -list");
			Console.WriteLine("CreateRuleTester_Fw472 -add  200.0.0.0  255.0.0.0  false");
			Console.WriteLine("CreateRuleTester_Fw472 -deleteall");
			Console.WriteLine("");
		}
	}
}
