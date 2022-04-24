using System;
using System.Collections.Generic;
using System.Text;

namespace Abraham.Web
{
	public class Entry
	{
		public DateTime Datetime    { get; set; }
		public string   DestIP	    { get; set; }
		public string   Method 	    { get; set; }
		public string   UriStem     { get; set; }
		public string   UriQuery    { get; set; }
		public int      Port 	    { get; set; }
		public string   Username    { get; set; }
		public string   SourceIP    { get; set; }
		public string   UserAgent   { get; set; }
		public string   Referer     { get; set; }
		public int      Status 	    { get; set; }
		public int      Substatus   { get; set; }
		public uint     Win32Status { get; set; }
		public int      TimeTaken   { get; set; }

		public static bool LineContainsData(string line)
		{
			return !line.StartsWith("#");
		}

		public static Entry Deserialize(string line)
		{
			var Entry = new Entry();

			int i=0;
			var Parts = line.Split(new char[] { ' ', '\t' });

		    DateTime date     = DateTime.ParseExact(Parts[i++], "yyyy-MM-dd" , System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
		    TimeSpan time     = TimeSpan.ParseExact(Parts[i++], @"hh\:mm\:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.TimeSpanStyles.None);
		    Entry.Datetime    = new DateTime(date.Year, date.Month, date.Day, time.Hours, time.Minutes, time.Seconds);
		    Entry.DestIP	  = Parts[i++];
		    Entry.Method 	  = Parts[i++];    
		    Entry.UriStem     = Parts[i++]; 
		    Entry.UriQuery    = Parts[i++]; 
		    Entry.Port 	      = Convert.ToInt32(Parts[i++]);
		    Entry.Username    = Parts[i++]; 
		    Entry.SourceIP    = Parts[i++]; 
		    Entry.UserAgent   = Parts[i++]; 
		    Entry.Referer     = Parts[i++]; 
		    Entry.Status 	  = Convert.ToInt32(Parts[i++]);    
		    Entry.Substatus   = Convert.ToInt32(Parts[i++]); 
		    Entry.Win32Status = Convert.ToUInt32(Parts[i++]); 
		    Entry.TimeTaken   = Convert.ToInt32(Parts[i++]); 
			
			return Entry;
		}

		public override string ToString()
		{
			return $"{Datetime} {DestIP} {Method}  {UriStem} {UriQuery} {Port} {Username} {SourceIP} {UserAgent} {Referer} {Status} {Substatus} {Win32Status} {TimeTaken}";
		}
	}
}
