using System.Diagnostics;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ImportWLK
{
	static class Program
	{
		public static Cumulus Cumulus { get; set; }
		public static string Location { get; set; }

		private static ConsoleColor defConsoleColour;
		private const int headerBytes = 212; // 16 + 4 + 32 * (2 + 4)

		static void Main()
		{

			// Tell the user what is happening

			TextWriterTraceListener myTextListener = new TextWriterTraceListener($"MXdiags{Path.DirectorySeparatorChar}ImportWLK-{DateTime.Now:yyyyMMdd-HHmmss}.txt", "WLKlog");
			Trace.Listeners.Add(myTextListener);
			Trace.AutoFlush = true;

			defConsoleColour = Console.ForegroundColor;

			var fullVer = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
			var version = $"{fullVer.Major}.{fullVer.Minor}.{fullVer.Build}";
			LogMessage("ImportWLK v." + version);
			Console.WriteLine("ImportWLK v." + version);

			LogMessage("Processing started");
			Console.WriteLine();
			Console.WriteLine($"Processing started: {DateTime.Now:U}");
			Console.WriteLine();

			// get the location of the exe - we will assume this is in the Cumulus root folder
			Location = AppDomain.CurrentDomain.BaseDirectory;

			// Read the Cumulus.ini file
			Cumulus = new Cumulus();

			// Check meteo day
			if (Cumulus.RolloverHour != 0)
			{
				LogMessage("Cumulus is not configured for a midnight rollover, so Import cannot create any day file entries");
				LogConsole("Cumulus is not configured for a midnight rollover, so no day file entries will be created", ConsoleColor.DarkYellow);
				LogConsole("You must run CreateMissing after this Import to create the day file entries", ConsoleColor.DarkYellow);
			}
			else
			{
				LogMessage("Cumulus is configured for a midnight rollover, Import will create day file entries");
				LogConsole("Cumulus is configured for a midnight rollover, so day file entries will be created", ConsoleColor.Cyan);
				LogConsole("You must still run CreateMissing after this Import to add missing details to those day file entries", ConsoleColor.Cyan);
			}
			Console.WriteLine();


			// Load the existing day file if it exists
			DayFile.ReadDayFile();


			// Find all the wlk files
			// naming convention YYYY-MM.wlk, eg 2024-05.wlk
			LogMessage("Searching for wlk files");
			Console.WriteLine("Searching for wlk log files...");

			var dirsep = Path.DirectorySeparatorChar;
			if (!Directory.Exists(Location + dirsep + "wlk"))
			{
				LogMessage($"The source directory '{Location}{dirsep}wlk' does not exist, aborting");
				LogConsole($"The source directory '{Location}{dirsep}wlk' does not exist, aborting", ConsoleColor.Red);
				Environment.Exit(1);
			}

			var dirInfo = new DirectoryInfo(Location + dirsep + "wlk");
			var wlkFiles = dirInfo.GetFiles("????-??.wlk");

			LogMessage($"Found {wlkFiles.Length} wlk log files");
			LogConsole($"Found {wlkFiles.Length} wlk log files", defConsoleColour);

			// sort the file list
			var wlkList = wlkFiles.OrderBy(f => f.Name).ToList();

			//	foreach file
			//		get header
			//			foreach day
			//				daily summaries to dayfile entry
			//				archive entries to log file entries
			//				archive entries to extra log file entries


			var lastYear = 0;

			foreach (var wlk in wlkList)
			{
				FileStream file;
				try
				{
					file = File.Open(wlk.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
				}
				catch (Exception ex)
				{
					LogMessage($"Error opening file {wlk.Name} - {ex.Message}");
					LogConsole($"Error opening file {wlk.Name} - {ex.Message}", ConsoleColor.Red);
					LogConsole("Skipping to next file", defConsoleColour);
					// abort this file
					continue;
				}

				// get the year/month from the file name
				var arr = wlk.Name.Split('-');
				var year = int.Parse(arr[0]);
				var month = int.Parse(arr[1].Split('.')[0]);

				using var binReader = new BinaryReader(file);
				var header = new WlkFileHeader();
				header.ReadFile(binReader);
				LogConsole($"Processing {wlk.Name}...", ConsoleColor.Gray);
				LogMessage($"File {wlk.Name} contains {header.TotalRecords} records covering {header.DayIndices.Length} days");

				var logFileWritten = false;
				var date = DateTime.MinValue;

				for (var day = 1; day < header.DayIndices.Length; day++)
				{
					if (header.DayIndices[day].RecordsInDay == 0)
					{
						LogMessage($"Day {day} contains no records");
						LogConsole($"  Day {day} contains no records", ConsoleColor.Gray);
						continue;
					}

					LogFile.HumidexHigh = -9999;
					LogFile.ApparentHigh = -9999;
					LogFile.ApparentLow = 9999;
					LogFile.FeelsLikeHigh = -9999;
					LogFile.FeelsLikeLow = 9999;

					LogFile.ClicksToday = 0;

					if (lastYear != year)
					{
						lastYear = year;
						LogFile.ClicksThisYear = 0;
					}

					LogMessage($"{year:D4}/{month:D2}/{day:D2} contains {header.DayIndices[day].RecordsInDay} records");
					LogConsole($"  {year:D4}/{month:D2}/{day:D2} contains {header.DayIndices[day].RecordsInDay} records", ConsoleColor.Gray);

					date = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Local);

					binReader.BaseStream.Seek(headerBytes + header.DayIndices[day].StartPosition * 88, SeekOrigin.Begin);

					for (var i = 0; i < header.DayIndices[day].RecordsInDay; i++)
					{
						// get the first record type
						byte type;
						try
						{
							type = binReader.ReadByte();

							if (type == 1)
							{
								// archive record
								LogMessage("Processing archive record type 1");

								var rec = new WlkArchiveRecord();
								rec.ReadRecord(binReader);

								rec.Timestamp = date.AddMinutes(rec.PackedTime);

								LogMessage("  Log entry time: " + rec.Timestamp.ToUniversalTime());

								// The wlk file contains the midnight entry for the following month, Cumulus starts the day at midnight rather than ending it midnight
								if (rec.Timestamp.Month != month)
								{
									// set the daily records for derived values
									if (LogFile.HumidexHigh > -9999)
									{
										DayFile.Records[date].HighHumidex = LogFile.HumidexHigh;
										DayFile.Records[date].HighHumidexTime = LogFile.HumidexHighTime;
									}
									if (LogFile.ApparentHigh > -9999)
									{
										DayFile.Records[date].HighAppTemp = LogFile.ApparentHigh;
										DayFile.Records[date].HighAppTempTime = LogFile.ApparentHighTime;
									}
									if (LogFile.ApparentLow < 9999)
									{
										DayFile.Records[date].LowAppTemp = LogFile.ApparentLow;
										DayFile.Records[date].LowAppTempTime = LogFile.ApparentLowTime;
									}
									if (LogFile.FeelsLikeHigh > -9999)
									{
										DayFile.Records[date].HighFeelsLike = LogFile.FeelsLikeHigh;
										DayFile.Records[date].HighFeelsLikeTime = LogFile.FeelsLikeHighTime;
									}
									if (LogFile.FeelsLikeLow < 9999)
									{
										DayFile.Records[date].LowFeelsLike = LogFile.FeelsLikeLow;
										DayFile.Records[date].LowFeelsLikeTime = LogFile.FeelsLikeLowTime;
									}

									// save the log file
									LogFile.WriteLogFile();
									logFileWritten = true;

									LogFile.Initialise();

									// save the extra log file
									ExtraLogFile.WriteLogFile();
									ExtraLogFile.Initialise();
								}

								if (LogFile.LastTimeStamp != DateTime.MinValue && LogFile.LastTimeStamp.Month != rec.Timestamp.Month)
								{
									// looks like the last entry from the previous month does not match the current month (have we skipped a month or two?) - so discard it
									LogFile.Initialise();
									ExtraLogFile.Initialise();
								}

								LogFile.AddRecord(rec);

								ExtraLogFile.AddRecord(rec);
							}
							else if (type == 2)
							{
								// daily summary1
								LogMessage("Processing summary record type 2");

								var rec = new WlkDailySummary1();
								rec.ReadRecord(binReader);
								rec.Date = date;

								DayFile.AddRecord1(rec);
							}
							else if (type == 3)
							{
								// daily summary2
								LogMessage("Processing summary record type 3");

								var rec = new WlkDailySummary2();
								rec.ReadRecord(binReader);
								rec.Date = date;

								DayFile.AddRecord2(rec);
							}
							else
							{
								// unknown
								LogMessage($"Unknown record type found - {type}. Aborting processing of this file");
								LogConsole($"Unknown record type found - {type}. Aborting processing of this file", ConsoleColor.Red);
							}
						}
						catch (EndOfStreamException)
						{
							binReader.Close();
						}
						catch (Exception ex)
						{
							LogMessage("Error - " + ex.Message);
							LogConsole(" Error - " + ex.Message, ConsoleColor.Red);
						}
					}


				}

				// save the log file if not done already
				if (!logFileWritten)
				{
					// set the daily records for derived values
					if (LogFile.HumidexHigh > -9999)
					{
						DayFile.Records[date].HighHumidex = LogFile.HumidexHigh;
						DayFile.Records[date].HighHumidexTime = LogFile.HumidexHighTime;
					}
					if (LogFile.ApparentHigh > -9999)
					{
						DayFile.Records[date].HighAppTemp = LogFile.ApparentHigh;
						DayFile.Records[date].HighAppTempTime = LogFile.ApparentHighTime;
					}
					if (LogFile.ApparentLow < 9999)
					{
						DayFile.Records[date].LowAppTemp = LogFile.ApparentLow;
						DayFile.Records[date].LowAppTempTime = LogFile.ApparentLowTime;
					}
					if (LogFile.FeelsLikeHigh > -9999)
					{
						DayFile.Records[date].HighFeelsLike = LogFile.FeelsLikeHigh;
						DayFile.Records[date].HighFeelsLikeTime = LogFile.FeelsLikeHighTime;
					}
					if (LogFile.FeelsLikeLow < 9999)
					{
						DayFile.Records[date].LowFeelsLike = LogFile.FeelsLikeLow;
						DayFile.Records[date].LowFeelsLikeTime = LogFile.FeelsLikeLowTime;
					}

					LogFile.WriteLogFile();
					LogFile.Initialise();

					// save the extra log file
					ExtraLogFile.WriteLogFile();
					ExtraLogFile.Initialise();
				}

			}

			// save dayfile
			DayFile.WriteDayFile();

			// summary
			LogConsole($"Wrote {DayFile.Records.Count} records to the day file", ConsoleColor.Green);

			LogConsole("Finished", ConsoleColor.Green);
			LogMessage("Finished");
		}

		public static void LogMessage(string message)
		{
			Trace.TraceInformation(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff ") + message);
		}

		public static void LogConsole(string msg, ConsoleColor colour, bool newLine = true)
		{
			Console.ForegroundColor = colour;

			if (newLine)
			{
				Console.WriteLine(msg);
			}
			else
			{
				Console.Write(msg);
			}

			Console.ForegroundColor = defConsoleColour;
		}
	}
}
