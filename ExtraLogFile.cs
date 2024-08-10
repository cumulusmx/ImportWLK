using System.Globalization;
using System.Security.Cryptography;
using System.Text;


namespace ImportWLK
{
	static partial class ExtraLogFile
	{
		private static readonly SortedList<DateTime, ExtraLogFileRec> records = [];

		public static DateTime LastTimeStamp { get; set; }

		internal static void Initialise()
		{
			records.Clear();
			LastTimeStamp = DateTime.MinValue;
		}

		internal static void AddRecord(WlkArchiveRecord rec)
		{
			LastTimeStamp = rec.Timestamp;

			if (rec.SoilTemp[0] < 255 || rec.SoilTemp[1] < 255 || rec.SoilTemp[2] < 255 || rec.SoilTemp[3] < 255 || rec.SoilTemp[4] < 255 || rec.SoilTemp[5] < 255 ||
				rec.SoilMoist[0] < 255 || rec.SoilMoist[1] < 255 || rec.SoilMoist[2] < 255 || rec.SoilMoist[3] < 255 || rec.SoilMoist[4] < 255 || rec.SoilMoist[5] < 255 ||
				rec.LeafWet[0] < 255 || rec.LeafWet[1] < 255 || rec.LeafWet[2] < 255 || rec.LeafWet[3] < 255 ||
				rec.ExtraTemp[0] < 255 || rec.ExtraTemp[1] < 255 || rec.ExtraTemp[2] < 255 || rec.ExtraTemp[3] < 255 || rec.ExtraTemp[4] < 255 || rec.ExtraTemp[5] < 255 || rec.ExtraTemp[6] < 255 ||
				rec.ExtraHum[0] < 255 || rec.ExtraHum[1] < 255 || rec.ExtraHum[2] < 255 || rec.ExtraHum[3] < 255 || rec.ExtraHum[4] < 255 || rec.ExtraHum[5] < 255 || rec.ExtraHum[6] < 255)
			{
				// we have data
				Program.LogDebugMessage("  Extra log entry for " + rec.Timestamp);
			}
			else
			{
				Program.LogDebugMessage("  Skipping extra log entry for " + rec.Timestamp);
				return;
			}

			if (!records.TryGetValue(rec.Timestamp, out var value))
			{
				value = new ExtraLogFileRec() { LogTime = rec.Timestamp};
				records.Add(rec.Timestamp, value);
			}

			// Soil Temp
			for (var i = 0; i < 6; i++)
			{
				if (rec.SoilTemp[i] < 255)
				{
					var val = rec.SoilTemp[i] - 90;
					var conv = ConvertUnits.TempFToUser(val);
					value.SoilTemp[i] = conv;
				}
			}

			// Soil Moisture
			for (var i = 0; i < 6; i++)
			{
				if (rec.SoilMoist[i] < 255)
				{
					value.SoilMoisture[i] = rec.SoilMoist[i];
				}
			}

			// Leaf Wetness
			for (var i = 0; i < 4; i++)
			{
				if (rec.LeafWet[i] < 255)
				{
					value.LeafWetness[i] = rec.LeafWet[i];
				}
			}

			// Extra Temp
			for (var i = 0; i < 7; i++)
			{
				if (rec.ExtraTemp[i] < 255)
				{
					var val = rec.ExtraTemp[i] - 90;
					var conv = ConvertUnits.TempFToUser(val);
					value.Temperature[i] = conv;
				}
			}

			// Extra Hum
			for (var i = 0; i < 7; i++)
			{
				if (rec.ExtraHum[i] < 255)
				{
					value.Humidity[i] = rec.ExtraHum[i];
				}
			}

			// Dewpoint
			for (var i = 0; i < 7; i++)
			{
				if (rec.ExtraTemp[i] < 255 && rec.ExtraHum[i] < 255)
				{

					var val = MeteoLib.DewPoint(ConvertUnits.UserTempToC(rec.ExtraTemp[i]), rec.ExtraHum[i]);
					var conv = ConvertUnits.TempCToUser(val);
					value.Dewpoint[i] = conv;
				}
			}
		}


		public static void WriteLogFile()
		{

			if (records.Count == 0)
			{
				Program.LogMessage("No records to write to Extra Log file!");
				return;
			}

			var logfilename = "data" + Path.DirectorySeparatorChar + GetExtraLogFileName(records.First().Key);
			Program.LogMessage($"Writing {records.Count} to {logfilename}");
			Program.LogConsole($"  Writing to {logfilename}", ConsoleColor.Gray);

			// backup old logfile
			if (File.Exists(logfilename))
			{
				if (!File.Exists(logfilename + ".sav"))
				{
					File.Move(logfilename, logfilename + ".sav");
				}
				else
				{
					var i = 1;
					do
					{
						if (!File.Exists(logfilename + ".sav" + i))
						{
							File.Move(logfilename, logfilename + ".sav" + i);
							break;
						}
						else
						{
							i++;
						}
					} while (true);
				}
			}


			try
			{
				using FileStream fs = new FileStream(logfilename, FileMode.Append, FileAccess.Write, FileShare.Read);
				using StreamWriter file = new StreamWriter(fs);
				Program.LogMessage($"{logfilename} opened for writing {records.Count} records");

				foreach (var rec in records)
				{
					var line = RecToCsv(rec);
					if (null != line)
						file.WriteLine(line);
				}

				file.Close();
				Program.LogMessage($"{logfilename} write complete");
			}
			catch (Exception ex)
			{
				Program.LogMessage($"Error writing to {logfilename}: {ex.Message}");
			}
		}

		public static string RecToCsv(KeyValuePair<DateTime, ExtraLogFileRec> keyval)
		{
			// Writes an entry to the n-minute extralogfile. Fields are comma-separated:
			// 0  Date in the form dd/mm/yy (the slash may be replaced by a dash in some cases)
			// 1  Current time - hh:mm
			// 2-11  Temperature 1-10
			// 12-21 Humidity 1-10
			// 22-31 Dew point 1-10
			// 32-35 Soil temp 1-4
			// 36-39 Soil moisture 1-4
			// 40-41 Leaf temp 1-2
			// 42-43 Leaf wetness 1-2
			// 44-55 Soil temp 5-16
			// 56-67 Soil moisture 5-16
			// 68-71 Air quality 1-4
			// 72-75 Air quality avg 1-4
			// 76-83 User temperature 1-8
			// 84  CO2
			// 85  CO2 avg
			// 86  CO2 pm2.5
			// 87  CO2 pm2.5 avg
			// 88  CO2 pm10
			// 89  CO2 pm10 avg
			// 90  CO2 temp
			// 91  CO2 hum

			var rec = keyval.Value;

			// make sure solar max is calculated for those stations without a solar sensor
			Program.LogDebugMessage("DoExtraLogFile: Writing log entry for " + rec.LogTime);
			var inv = CultureInfo.InvariantCulture;
			var sep = ',';

			var sb = new StringBuilder(256);
			sb.Append(rec.LogTime.ToString("dd/MM/yy", inv) + sep);
			sb.Append(rec.LogTime.ToString("HH:mm", inv) + sep);
			// Extra Temp 1-10
			for (int i = 0; i < 10; i++)
			{
				sb.Append(rec.Temperature[i].ToString(Program.Cumulus.TempFormat, inv) + sep);
			}
			// Extra Hum 1-10
			for (int i = 0; i < 10; i++)
			{
				sb.Append(rec.Humidity[i].ToString() + sep);
			}
			// Extra Dewpoint 1-10
			for (int i = 0; i < 10; i++)
			{
				sb.Append(rec.Dewpoint[i].ToString(Program.Cumulus.TempFormat, inv) + sep);
			}
			// Extra Soil Temp 1-4
			for (int i = 0; i < 4; i++)
			{
				sb.Append(rec.SoilTemp[i].ToString(Program.Cumulus.TempFormat, inv) + sep);
			}
			// Extra Soil Moisture 1-4
			for (int i = 0; i < 4; i++)
			{
				sb.Append(rec.SoilMoisture[i].ToString() + sep);
			}
			// Leaf temp - not used
			sb.Append("0,0,0,0,");
			// Extra Leaf wetness 1-2
			sb.Append(rec.LeafWetness[0].ToString() + sep);
			sb.Append(rec.LeafWetness[1].ToString() + sep);
			// Soil Temp 5-16
			for (int i = 4; i < 16; i++)
			{
				sb.Append(rec.SoilTemp[i].ToString(Program.Cumulus.TempFormat, inv) + sep);
			}
			// Soil Moisture 5-16
			for (int i = 4; i < 16; i++)
			{
				sb.Append(rec.SoilMoisture[i].ToString() + sep);
			}
			// Air quality 1-4
			for (int i = 0; i < 4; i++)
			{
				sb.Append("0" + sep);
			}
			// Air quality avg 1-4
			for (int i = 0; i < 4; i++)
			{
				sb.Append("0" + sep);
			}
			// User temp 1-8
			for (int i = 0; i < 8; i++)
			{
				sb.Append("0" + sep);
			}
			// CO2
			sb.Append("0,0,0,0,0,0,0,0");

			return sb.ToString();
		}

		private static string GetExtraLogFileName(DateTime thedate)
		{
			return "ExtraLog" + thedate.ToString("yyyyMM") + "log.txt";
		}
	}

	internal class ExtraLogFileRec
	{
		public DateTime LogTime;
		public double[] Temperature = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
		public int[] Humidity = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
		public double[] Dewpoint = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
		public double[] SoilTemp = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
		public int[] SoilMoisture = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
		public double[] LeafTemp = [0, 0];
		public int[] LeafWetness = [0, 0];
	}
}
