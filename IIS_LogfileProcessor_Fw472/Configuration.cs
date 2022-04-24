using System.Collections.Generic;

namespace IIS_LogfileProcessor
{
	internal class Configuration
	{
		public string IIS_Logfile_Directory { get; set; }
		public List<string> WhiteList { get; set; }
		public List<string> BlackList { get; set; }
		public List<string> IPsNeverBlocked { get; set; }
		public string NetworkMaskForBanEntry { get; set; }
		public bool SimulationOnly { get; set; }
	}
}