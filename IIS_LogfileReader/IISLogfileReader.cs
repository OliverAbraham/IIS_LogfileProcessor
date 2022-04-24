using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Abraham.Web
{
	#region // #### MG: Enums
	public enum FileType
	{
		Data,
		Error
	}
	#endregion

	public class IISLogfileReader
	{
		#region ------------- Properties ----------------------------------------------------------

		public string LogfilesFolder { get; set; }
		public string StatusFile { get; set; }
		public List<string> ErrorMessages { get; set; }

		#endregion



		#region ------------- Fields --------------------------------------------------------------

		private Status _Status;

		#endregion



		#region ------------- Init ----------------------------------------------------------------

		public IISLogfileReader()
		{
			StatusFile = "IISLogfileReader.status";
		}

		#endregion



		#region ------------- Methods -------------------------------------------------------------

        public List<Entry> ReadNewEntries()
		{
			ErrorMessages = new List<string>();
			
			if (SourceFolderDoesntExist())
				return EmptyList();

			var files = EnumerateAllLogfiles(LogfilesFolder);

			if (NoFilesAreThere(files))
				return EmptyList();

			ReadStatusFromFile();

			var newLines = ReadAllNewLinesSinceLastCall(files);

			SaveStatusToFile();

			return ConvertLinesToEntries(newLines);
		}

        public void DeleteStatus_for_unit_tests_only()
		{
			File.Delete(StatusFile);
		}

        public void PatchStatus_for_unit_tests_only(string old, string @new)
		{
			ReadStatusFromFile();
			_Status.LastReadFile = _Status.LastReadFile.Replace(old, @new);
			SaveStatusToFile();
		}

		#endregion



		#region ------------- Implementation ------------------------------------------------------

		private bool SourceFolderDoesntExist()
		{
			if (!System.IO.Directory.Exists(LogfilesFolder))
			{
				ErrorMessages.Add("Source folder doesn't exist");
				return true;
			}
			else
			{
				return false;
			}
		}

		private bool NoFilesAreThere(List<string> filenames)
		{
			if (filenames.Count == 0)
			{
				ErrorMessages.Add("No files in source folder");
				return true;
			}
			else
			{
				return false;
			}
		}

		private List<Entry> EmptyList()
		{
			return new List<Entry>();
		}

		private List<string> ReadAllNewLinesSinceLastCall(List<string> AllFilenames)
		{
			bool FindStart = true;
			if (_Status.LastReadFile == null)
				FindStart = false;

			var NewLines = new List<string>();

			foreach (var filename in AllFilenames)
			{
				ReadNewLinesInFile(filename, ref FindStart, ref NewLines);
			}

			return NewLines;
		}

		private void ReadNewLinesInFile(string filename, ref bool FindStart, ref List<string> NewLines)
		{
			if (FindStart)
			{
				if (Path.GetFileNameWithoutExtension(filename) == Path.GetFileNameWithoutExtension(_Status.LastReadFile))
				{
					var Lines = ReadAllLinesWithErrorHandling(filename);
					// Only process this file if nobody has cut some lines!
					if (Lines.GetLength(0) > _Status.LastReadLineNumber)
					{
						for (int i = _Status.LastReadLineNumber + 1; i <= Lines.GetLength(0); i++)
							NewLines.Add(Lines[i - 1]);
						_Status.LastReadLineNumber = Lines.GetLength(0) - 1;
					}
					FindStart = false;
				}
			}
			else
			{
				var Lines2 = ReadAllLinesWithErrorHandling(filename);
				if (Lines2.GetLength(0) > 0)
				{
					NewLines.AddRange(Lines2);
					_Status.LastReadLineNumber = Lines2.GetLength(0);
					_Status.LastReadFile = filename;
				}
			}
		}

		private string[] ReadAllLinesWithErrorHandling(string filename)
		{
			try
			{
				Console.WriteLine($" - file {filename}");
				return File.ReadAllLines(filename);
			}
			catch (IOException ex)
			{
				Console.WriteLine($" - file {filename}: Error reading file: IO exception, file might be in use, Hresult = {ex.HResult}");
				return new string[0];
			}
			catch (Exception ex)
			{
				Console.WriteLine($" - file {filename}: Error reading file: {ex.ToString()}");
				return new string[0];
			}
		}

		private List<string> EnumerateAllLogfiles(string logfilesFolder)
		{
			var files = Directory.EnumerateFiles(LogfilesFolder, "*.log", SearchOption.TopDirectoryOnly).ToList();
			
			// sort files by filename --> on IIS same as sorting by date (what we want)
			files.Sort();

			return files;
		}

		private List<Entry> ConvertLinesToEntries(List<string> lines)
		{
			var entries = new List<Entry>();
			
			foreach (var line in lines)
			{
				if (Entry.LineContainsData(line))
					entries.Add(Entry.Deserialize(line));
			}
			
			return entries;
		}

		private void ReadStatusFromFile()
		{
			if (!System.IO.File.Exists(StatusFile))
			{
				_Status = new Status();
				return;
			}
			else
			{
				string Json = System.IO.File.ReadAllText(StatusFile);
				_Status = JsonConvert.DeserializeObject<Status>(Json);
			}
		}

		private void SaveStatusToFile()
		{
			var content = JsonConvert.SerializeObject(_Status);
			System.IO.File.WriteAllText(StatusFile, content);
		}

		#endregion
	}
}