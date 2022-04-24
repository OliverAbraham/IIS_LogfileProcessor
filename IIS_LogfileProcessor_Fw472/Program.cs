using Abraham.Web;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using IIS_LogfileProcessor;
using System.IO;
using System.Net;
using CommandLine;
using CommandLine.Text;
using System.Globalization;

namespace IIS_LogfileProcessor_Fw472
{
    class Program
	{
		private const string VERSION = "2021-09-29";

		#region ------------- Command line options ------------------------------------------------
		class Options
		{
			[Option("help", Required = false, HelpText = "Display info")]
			public bool Help { get; set; }

			[Option("list", Required = false, HelpText = "List all existing rules in IIS")]
			public bool List { get; set; }

			[Option("deleteall", Required = false, HelpText = "Delete all IP rules is IIS")]
			public bool DeleteAll { get; set; }

			[Option("save", Required = false, HelpText = "Saveall deny rules to file")]
			public bool SaveToFile { get; set; }

			[Option("load", Required = false, HelpText = "Load deny rules from file into IIS (with filename)")]
			public string LoadFromFileFilename { get; set; }

			[Option("reset", Required = false, HelpText = "Reset saved logfile position and start from the beginning of all logs")]
			public bool ResetLogPosition { get; set; }

			[Option("analyse", Required = false, HelpText = "Analyse files and create new deny rules")]
			public bool Analyse { get; set; }

			[Option("config", Required = false, HelpText = "Display configuration")]
			public bool DisplayConfiguration { get; set; }

			[Option("unattended", Required = false, HelpText = "Don't prompt for keypress, for unattended use")]
			public bool Unattended { get; set; }

			public bool LoadFromFile 
			{ 
				get { return !string.IsNullOrWhiteSpace(LoadFromFileFilename); } 
			}

			public bool NoOptionIsSet()
			{
				return	!Help                 &&
						!List                 &&
						!DeleteAll            &&
						!SaveToFile           &&
						!LoadFromFile         &&
						!ResetLogPosition     &&
						!DisplayConfiguration &&
						!Analyse;
			}
		}

		#endregion



		#region ------------- Fields --------------------------------------------------------------

		// Command line
		private static Options _CommandLineOptions;
		private static string _Heading;
		private static string _Copyright;

		// Configuration file:
		private static string _ConfigFile = "appsettings.json";
		private static Configuration _Configuration;
		private static bool _DisplayWhiteListEntries = false;
		private static bool _DisplayBlackListEntries = false;

		#endregion



		#region ------------- Implementation ------------------------------------------------------

		static int Main(string[] args)
		{
			DisplayGreeting();
			if (!ParseCommandLineArguments(args))
				return 1;

			if (_CommandLineOptions.List)
				return ListExistingRules();

			if (_CommandLineOptions.DeleteAll)
				return DeleteAllRules();

			if (_CommandLineOptions.SaveToFile)
				return SaveAllRulesToFile();

			if (_CommandLineOptions.LoadFromFile)
				return LoadAllRulesFromFile();

			if (_CommandLineOptions.ResetLogPosition)
				return ResetLogPosition();

			if (_CommandLineOptions.DisplayConfiguration)
				return DisplayConfiguration();

			if (_CommandLineOptions.Analyse)
				return AnalyseLogfiles();

			return 1;
		}

		#endregion



		#region ------------- User interface ------------------------------------------------------

		private static void DisplayGreeting()
		{
			_Heading   = $"IIS logfile processor {VERSION}";
			_Copyright = "Oliver Abraham 2019-2021 - mail@oliver-abraham.de";

			Console.WriteLine($"-------------------------------------------------------------------------");
			Console.WriteLine($"{_Heading} - {_Copyright}");
			Console.WriteLine($"-------------------------------------------------------------------------");
			Console.WriteLine($"");
		}

		private static bool ParseCommandLineArguments(string[] args)
		{
			var result = Parser.Default.ParseArguments<Options>(args);
			result.WithParsed<Options>(o => { _CommandLineOptions = o; });

			if (_CommandLineOptions == null)
				return false;

			if (_CommandLineOptions.NoOptionIsSet())
			{
				var helpText = new HelpText().AddOptions(result);
				//= HelpText.AutoBuild<Options>();
				//result, h =>
				//{
				//	h.AdditionalNewLineAfterOption = false;
				//	h.AddNewLineBetweenHelpSections = false;
				//	h.Heading = _Heading;
				//	h.Copyright = _Copyright;
				//	h.AutoVersion = false;
				//	return h;
				//}, e => e);
				Console.WriteLine(helpText);
				return false;
			}
			return true;
		}

		private static int ListExistingRules()
		{
			try
			{
				Console.WriteLine("Reading all existing IP deny rules from our IIS...");
				var rules = ReadAllExistingIPBanRulesFromIIS();
				DisplayRules(rules);
				return 0;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return 1;
			}
		}

		private static void DisplayRules(List<Rule> rules)
		{
			if (rules.Count > 0)
			{
				Console.WriteLine("Existing rules:");
				foreach (var rule in rules)
					Console.WriteLine(rule.ToString());
			}
			else
			{
				Console.WriteLine("THERE ARE CURRENTLY NO RULES SET IN IIS");
			}
			Console.WriteLine();
		}

		private static int DeleteAllRules()
		{
			try
			{
				Console.WriteLine("Deleting all existing IP allow/deny rules from our IIS...");
				DeleteAllExistingRulesInIIS();
				return 0;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return 1;
			}
		}

		private static int SaveAllRulesToFile()
		{
			try
			{
				var filename = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", new CultureInfo("DE-DE"))}.rules";
				Console.WriteLine($"Saving all existing IP allow/deny rules to file '{filename}'...");
				var rules = ReadAllExistingIPBanRulesFromIIS();
				SaveRulesToFile(rules, filename);
				Console.WriteLine($"{rules.Count} rules saved.");
				return 0;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return 1;
			}
		}

		private static int LoadAllRulesFromFile()
		{
			try
			{
				var filename = _CommandLineOptions.LoadFromFileFilename;
				Console.WriteLine($"Loading deny rules from file '{filename}'...");
				var rules = ReadAllExistingIPBanRulesFromIIS();
				Console.WriteLine("Adding rules to IIS...");
				AddRulesToIIS(rules);
				Console.WriteLine("Rules added.");
				return 0;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return 1;
			}
		}

		private static void DisplayParameters()
		{
			Console.WriteLine($"");
			Console.WriteLine($"Configuration:");
			Console.WriteLine($"----------------");
			Console.WriteLine($"IIS_Logfile_Directory: {_Configuration.IIS_Logfile_Directory}");
			Console.WriteLine($"NetworkMaskForDenyEntry: {_Configuration.NetworkMaskForBanEntry}");
			Console.WriteLine($"");
			Console.WriteLine($"");
			Console.WriteLine($"Whitelist:");
			Console.WriteLine($"Items on this list will never produce a deny rule for the requesting IP");
			Console.WriteLine($"----------------");
			foreach (var item in _Configuration.WhiteList)
				Console.WriteLine(item);
			Console.WriteLine($"");
			Console.WriteLine($"");
			Console.WriteLine($"Blacklist:");
			Console.WriteLine($"Items on this list will immediately cause a deny rule for the requesting IP:");
			Console.WriteLine($"----------------");
			foreach (var item in _Configuration.BlackList)
				Console.WriteLine(item);
			Console.WriteLine($"");
			Console.WriteLine($"IPs that are never blocked:");
			foreach (var item in _Configuration.IPsNeverBlocked)
				Console.WriteLine(item);
			Console.WriteLine($"");
			Console.WriteLine($"");
		}

		private static int ResetLogPosition()
		{
			try
			{
				File.Delete("IISLogfileReader.status");
				Console.WriteLine($"Log position deleted. Next analyse will start the logs from the oldest logfile.");
				return 0;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return 1;
			}
		}

		private static int DisplayConfiguration()
		{
			try
			{
				ReadConfiguration();
				DisplayParameters();
				return 0;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return 1;
			}
		}

		private static int AnalyseLogfiles()
		{
			try
			{
				ReadConfiguration();
				DisplayParameters();

				Console.WriteLine($"processing the new log entries now:");
				if (!_DisplayWhiteListEntries)
					Console.WriteLine($"note: WhiteList calls are omitted.");
				if (!_DisplayBlackListEntries)
					Console.WriteLine($"note: BlackList calls are omitted.");
				Console.WriteLine($"");
				Console.WriteLine($"");
			
				var existingBans = ReadAllExistingIPBanRulesFromIIS();
				DisplayRules(existingBans);
			

				Console.WriteLine("Press any key to read IIS logs and process new entries...");
				if (!_CommandLineOptions.Unattended)
					Console.ReadKey();
				Console.WriteLine();
				Console.WriteLine();


				var entries = ReadNewLogfileEntriesSinceLastRun();
				var newBans = ProcessEntries(entries, existingBans);
				DisplayRules(newBans);


				Console.WriteLine();
				Console.WriteLine();
				Console.WriteLine("Press any key to add newbans to IIS...");
				if (!_CommandLineOptions.Unattended)
					Console.ReadKey();


				Console.WriteLine();
				Console.WriteLine($"adding rule to IIS to ban the new 'bad' IPs:");
				AddRulesToIIS(newBans);
				DisplayFinalMessage();
				return 0;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return 1;
			}
		}

		#endregion



		#region ------------- Configuration file --------------------------------------------------

		private static void ReadConfiguration()
		{
			Console.WriteLine($"Reading configuration from file '{_ConfigFile}'");
			
			if (!System.IO.File.Exists(_ConfigFile))
			{
				_Configuration = new Configuration();
				return;
			}
			else
			{
				string Json = System.IO.File.ReadAllText(_ConfigFile);
				_Configuration = JsonConvert.DeserializeObject<Configuration>(Json);
			}
		}

		#endregion



		#region ------------- Logic ---------------------------------------------------------------

		private static List<Rule> ReadAllExistingIPBanRulesFromIIS()
		{
			var creator = new IISRuleCreator();
			return creator.ReadAllRules();
		}

		private static void DeleteAllExistingRulesInIIS()
		{
			var creator = new IISRuleCreator();
			creator.DeleteAllRules();
		}

		private static void SaveRulesToFile(List<Rule> rules, string filename)
		{
			if (rules.Count == 0)
				return;

			string contents = "";
			foreach (var rule in rules)
				contents += rule.ToString() + "\n";
			
			File.WriteAllText(filename, contents);
		}

		private static List<Entry> ReadNewLogfileEntriesSinceLastRun()
		{
			Console.WriteLine();
			Console.WriteLine($"Reading all new logfiles and entries since last run:");
			Console.WriteLine($"Reading '{_Configuration.IIS_Logfile_Directory}'");
			var reader = new IISLogfileReader();
			reader.LogfilesFolder = _Configuration.IIS_Logfile_Directory;
			var Entries = reader.ReadNewEntries();

			if (reader.ErrorMessages.Count > 0)
			{
				Console.WriteLine($"Errors reading the source directory:");
				foreach (var message in reader.ErrorMessages)
					Console.WriteLine(message);
				throw new Exception("problem reading files from the source directory");
			}
			Console.WriteLine($"Read {Entries.Count} new log entries");
			return Entries;
		}

		private static void DisplayFinalMessage()
		{
			Console.WriteLine($"processing log entries finished");
		}

		private static bool ItemIsOnList(string text, List<string> list)
		{
			foreach(var item in list)
			{
				if (item.EndsWith("*"))
				{
					string substring = item.Substring(0, item.Length-1);
					if (text.StartsWith(substring))
						return true;
				}
				if (text == item)
					return true;
			}
			return false;
		}

		private static List<Rule> ProcessEntries(List<Entry> entries, List<Rule> existingBans)
		{
			//DisplaySomeInformation();

			var IPAdressesToBan = new List<string>();
			string lastIP = "";
			int clientAccessCounter = 0;
			foreach (var entry in entries)
			{
				ProcessEntry(entry, IPAdressesToBan, lastIP, existingBans, ref clientAccessCounter);
				lastIP = entry.SourceIP;
			}

			IPAdressesToBan.Sort();


			var rules = new List<Rule>();
			foreach (var ip in IPAdressesToBan)
			{
				rules.Add(new Rule() { IP = ip });
			}
			return rules;
		}

		private static void ProcessEntry(Entry entry, List<string> BannedIPs, string lastIP, List<Rule> existingBans, ref int counter)
		{
			string Result = "SUSPECT";
			if (ItemIsOnList(entry.UriStem, _Configuration.WhiteList))
				Result = "WHITELIST";
			else if (ItemIsOnList(entry.UriStem, _Configuration.BlackList))
				Result = "BLACKLIST";


			bool ThisIPWillNeverBeBlocked = _Configuration.IPsNeverBlocked.Contains(entry.SourceIP);
			if (ThisIPWillNeverBeBlocked)
			{
				counter = 0;
				Result = "NEVERBAN";
				DisplayEntry(BannedIPs, entry, Result, counter);
				return;
			}

			//if (entry.SourceIP != lastIP)
			//{
			//	counter = 0;
			//	DisplayEntry(BannedIPs, entry, Result, counter);
			//	return;
			//}

			if (EntryIsAlreadyBanned(entry.SourceIP, existingBans) || BannedIPs.Contains(entry.SourceIP))
            {
				counter = 0;
				Result = "BANNED";
				DisplayEntry(BannedIPs, entry, Result, counter);
				return;
            }

			// 3 or more subsequent calls from the same IP come on the banned list!
			// we don't look on the time, it can also be that these 10 calls are within some hours
			// (which is very unlikely)
			// If the traffic is high this algorithmn won't work, because different clients intersect
			if (Result == "SUSPECT" || Result == "BLACKLIST")
				counter++;

			DisplayEntry(BannedIPs, entry, Result, counter);

			if ((Result == "BLACKLIST" && counter >= 1) ||
				(counter >= 3))
			{
				//if (!BannedIPs.Contains(entry.SourceIP))
				//{
					if (EntryIsAlreadyBanned(entry.SourceIP, existingBans) || BannedIPs.Contains(entry.SourceIP))
					{
						Console.WriteLine($"------------------------------ THIS IP IS ALREADY BANNED!  -------------------------");
					}
					else
					{
						Console.WriteLine($"------------------------------ THIS IP WILL BE BANNED NOW! ------------------------- banning {entry.SourceIP}");
						if (!BannedIPs.Contains(entry.SourceIP))
							BannedIPs.Add(entry.SourceIP);
					}
				//}
				counter = 0;
			}
		}

		private static bool EntryIsAlreadyBanned(string sourceIP, List<Rule> existingRules)
		{
			foreach(var rule in existingRules)
			{
				if (ThisRuleCoversTheIP(rule, sourceIP))
					return true;
			}
			return false;
		}

		private static bool ThisRuleCoversTheIP(Rule rule, string sourceIP)
		{
			if (rule.Allowed)
				return false;
			return MaskIP(rule.IP, rule.SubnetMask) == MaskIP(sourceIP, rule.SubnetMask);
		}

		private static string MaskIP(string ip, string subnetMask)
		{
			IPAddress adr = IPAddress.Parse(ip);
			IPAddress mask = IPAddress.Parse(subnetMask);
			
			long result = adr.Address & mask.Address;

			var resultAsString = new IPAddress(result).ToString();
			return resultAsString;
		}

		private static void DisplayEntry(List<string> BannedIPs, Entry entry, string Result, int counter)
		{
			bool display = true;
			if (!_DisplayWhiteListEntries && Result == "WHITELIST")
				display = false;
			//if (!_DisplayBlackListEntries && Result == "BLACKLIST")
			//	display = false;
			//if (BannedIPs.Contains(entry.SourceIP))
			//	display = false;

			if (display)
				Console.WriteLine($"{Result,-9}  {counter}  {entry.Datetime.ToString(@"yyyy-MM-dd hh\:mm\:ss")}    {entry.Method,-5} {entry.UriStem,-40} from {entry.SourceIP,-15} ");
		}

		private static void AddRulesToIIS(List<Rule> rules)
		{
			try
			{
				var creator = new IISRuleCreator();
				CreateNewDenyRules(creator, rules);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		private static void CreateNewDenyRules(IISRuleCreator creator, List<Rule> rules)
		{
			foreach(var rule in rules)
			{
				CreateNewDenyRule(creator, rule);
			}
		}

		private static void CreateNewDenyRule(IISRuleCreator creator, Rule rule)
		{
			string maskedIP = MaskIP(rule.IP, _Configuration.NetworkMaskForBanEntry);
			
			if (!_Configuration.SimulationOnly)
			{
				try
				{
					creator.AddRule(maskedIP, _Configuration.NetworkMaskForBanEntry, allowed:false);
					Console.WriteLine($"Added a deny rule to IIS for {maskedIP}, mask {_Configuration.NetworkMaskForBanEntry} (original IP {rule.IP})");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Problem adding a rule to IIS: {ex.ToString()}");
					Console.WriteLine($"I was trying to add a deny rule to IIS for {maskedIP}, mask {_Configuration.NetworkMaskForBanEntry} (original IP {rule.IP})");
				}
			}
			else
			{
				Console.WriteLine($"SIMULATION: Adding a deny rule to IIS for {maskedIP}, mask {_Configuration.NetworkMaskForBanEntry} (original IP {rule.IP})");
			}
		}

		#endregion
	}
}
