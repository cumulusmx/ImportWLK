using System.Globalization;
using System.Text;


namespace ImportWLK
{
	static partial class LogFile
	{
		private static readonly SortedList<DateTime, LogFileRec> records = [];

		public static double HumidexHigh { get; set; }
		public static DateTime HumidexHighTime { get; set; }

		public static double ApparentHigh { get; set; }
		public static double ApparentLow { get; set; }
		public static DateTime ApparentHighTime { get; set; }
		public static DateTime ApparentLowTime { get; set; }

		public static double FeelsLikeHigh { get; set; }
		public static double FeelsLikeLow { get; set; }
		public static DateTime FeelsLikeHighTime { get; set; }
		public static DateTime FeelsLikeLowTime { get; set; }

		public static int ClicksToday { get; set; }
		public static int ClicksThisYear { get; set; }

		public static DateTime LastTimeStamp { get; set; }

		private static readonly Dictionary<int, (double bucketSize, string bucketUnit)> BucketTypes = new()
		{
			{ 0x0000, (0.1, "in") },
			{ 0x1000, (0.01, "in") },
			{ 0x2000, (0.2, "mm") },
			{ 0x3000, (1.0, "mm") },
			{ 0x6000, (0.1, "mm") },
		};


		internal static void Initialise()
		{
			records.Clear();
			LastTimeStamp = DateTime.MinValue;
		}

		internal static void AddRecord(WlkArchiveRecord rec)
		{
			LastTimeStamp = rec.Timestamp;

			if (!records.TryGetValue(rec.Timestamp, out var value))
			{
				value = new LogFileRec() { LogTime = rec.Timestamp};
				records.Add(rec.Timestamp, value);
			}

			var val = rec.OutsideTemp / 10.0;
			var conv = ConvertUnits.TempFToUser(val);
			value.Temperature = rec.OutsideTemp < -2000 ? 0 :conv;

			// not used
			// OutsideTempHi
			// OutsideTempLo

			val = rec.InsideTemp / 10.0;
			conv = ConvertUnits.TempFToUser(val);
			value.InsideTemp = rec.InsideTemp < -2000 ? 0 : conv;

			val = rec.Baro / 1000.0;
			conv = ConvertUnits.PressINHGToUser(val);
			value.Baro = rec.Baro < 0 ? 0 : conv;

			val = rec.OutsideHumidity / 10.0;
			value.Humidity = rec.OutsideHumidity < 0 ? 0 : (int) val;

			val = rec.InsideHumidity / 10.0;
			value.InsideHum = rec.InsideHumidity < 0 ? 0 : (int) val;

			var bucketType = rec.RainClicks & 0xF000;
			double bucketSize;
			string bucketUnit;

			if (BucketTypes.TryGetValue(bucketType, out var bucketInfo))
			{
				(bucketSize, bucketUnit) = bucketInfo;
			}
			else
			{
				// Handle default case
				bucketSize = 0.01;
				bucketUnit = "in";
			}

			if (rec.RainClicks >= 0)
			{
				ClicksThisYear += rec.RainClicks;
				ClicksToday += rec.RainClicks;
				value.RainfallCounter = ConvertUnits.RainINToUser(ClicksThisYear * bucketSize);

				if (rec.Timestamp.Hour == 0 && rec.Timestamp.Minute == 0)
					ClicksToday = 0;

				var rainToday = ClicksToday * bucketSize;


				if (bucketUnit == "in")
				{
					val = ConvertUnits.RainINToUser(rainToday);
				}
				else
				{
					val = ConvertUnits.RainMMToUser(rainToday);
				}

				value.RainfallToday = val;
				value.RainSinceMidnight = val;
			}

			if (rec.RainRateHi >= 0)
			{
				val = rec.RainRateHi * bucketSize;
				if (bucketUnit == "in")
				{
					conv = ConvertUnits.RainINToUser(val);
				}
				else
				{
					conv = ConvertUnits.RainMMToUser(val);
				}
			}
			else
			{
				conv = 0;
			}

			value.RainfallRate = conv;

			val = rec.WindSpeed / 10.0;
			conv = ConvertUnits.WindMPHToUser(val);
			value.WindSpeed = rec.WindSpeed < 0 ? 0 : conv;

			val = rec.WindDir == 255 ? 0 : (int) (rec.WindDir * 22.5);
			value.WindBearing = (int) val;
			value.SolarRad = rec.Solar < 0 ? 0 : rec.Solar;
			value.UVI = rec.UV > 200 ? 0 : rec.UV / 10;
			value.ET = ConvertUnits.RainINToUser(rec.ET > 200 ? 0 : rec.ET / 1000);

			if (rec.OutsideTemp > -2000 && rec.WindSpeed >= 0)
			{
				val = MeteoLib.WindChill(ConvertUnits.UserTempToC(value.Temperature), ConvertUnits.UserWindToKPH(value.WindSpeed));
				value.WindChill = val;
			}

			if (rec.OutsideTemp > -2000 && rec.OutsideHumidity >= 0 && rec.OutsideHumidity <= 1000)
			{
				val = MeteoLib.HeatIndex(ConvertUnits.UserTempToC(value.Temperature), value.Humidity);
				value.HeatIndex = val;

				val = MeteoLib.Humidex(ConvertUnits.UserTempToC(value.Temperature), value.Humidity);
				value.Humidex = val;

				if (val > HumidexHigh)
				{
					HumidexHigh = val;
					HumidexHighTime = rec.Timestamp;
				}

				val = MeteoLib.DewPoint(ConvertUnits.UserTempToC(value.Temperature), value.Humidity);
				value.Dewpoint = val;
			}

			if (rec.OutsideTemp > -2000 && rec.OutsideHumidity >= 0 && rec.OutsideHumidity <= 1000 && rec.WindSpeed >= 0)
			{
				val = MeteoLib.ApparentTemperature(ConvertUnits.UserTempToC(value.Temperature), ConvertUnits.UserWindToMS(value.WindSpeed), value.Humidity);
				value.ApparentTemp = val;

				if (val > ApparentHigh)
				{
					ApparentHigh = val;
					ApparentHighTime = rec.Timestamp;
				}
				if (val < ApparentLow)
				{
					ApparentLow = val;
					ApparentLowTime = rec.Timestamp;
				}


				val = MeteoLib.FeelsLike(ConvertUnits.UserTempToC(value.Temperature), ConvertUnits.UserWindToKPH(value.WindSpeed), value.Humidity);
				value.FeelsLike = val;

				if (val > FeelsLikeHigh)
				{
					FeelsLikeHigh = val;
					FeelsLikeHighTime = rec.Timestamp;
				}
				if (val < FeelsLikeLow)
				{
					FeelsLikeLow = val;
					FeelsLikeLowTime = rec.Timestamp;
				}
			}

			value.SolarMax = AstroLib.SolarMax(
					rec.Timestamp,
					(double) Program.Cumulus.Longitude,
					(double) Program.Cumulus.Latitude,
					Utils.AltitudeM(Program.Cumulus.Altitude),
					out _,
					Program.Cumulus.SolarOptions
				);
		}


		public static void WriteLogFile()
		{
			var logfilename = "data" + Path.DirectorySeparatorChar + GetLogFileName(records.First().Key);

			if (records.Count == 0)
			{
				Program.LogMessage($"No records to write to {logfilename}!");
				Program.LogConsole($"  No records to write to {logfilename}!", ConsoleColor.Red);
				return;
			}

			Program.LogMessage($"Writing {records.Count} to {logfilename}");
			Program.LogConsole($"  Writing to {logfilename}", ConsoleColor.Gray);

			// backup old dayfile.txt
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

		public static string RecToCsv(KeyValuePair<DateTime, LogFileRec> keyval)
		{
			// Writes an entry to the n-minute log file. Fields are comma-separated:
			// 0  Date in the form dd/mm/yy (the slash may be replaced by a dash in some cases)
			// 1  Current time - hh:mm
			// 2  Current temperature
			// 3  Current humidity
			// 4  Current dewpoint
			// 5  Current wind speed
			// 6  Recent (10-minute) high gust
			// 7  Average wind bearing
			// 8  Current rainfall rate
			// 9  Total rainfall today so far
			// 10  Current sea level pressure
			// 11  Total rainfall counter as held by the station
			// 12  Inside temperature
			// 13  Inside humidity
			// 14  Current gust (i.e. 'Latest')
			// 15  Wind chill
			// 16  Heat Index
			// 17  UV Index
			// 18  Solar Radiation
			// 19  Evapotranspiration
			// 20  Annual Evapotranspiration
			// 21  Apparent temperature
			// 22  Current theoretical max solar radiation
			// 23  Hours of sunshine so far today
			// 24  Current wind bearing
			// 25  RG-11 rain total
			// 26  Rain since midnight
			// 27  Feels like
			// 28  Humidex

			var rec = keyval.Value;

			// make sure solar max is calculated for those stations without a solar sensor
			Program.LogMessage("DoLogFile: Writing log entry for " + rec.LogTime);
			var CurrentSolarMax = AstroLib.SolarMax(rec.LogTime, (double) Program.Cumulus.Longitude, (double) Program.Cumulus.Latitude, Utils.AltitudeM(Program.Cumulus.Altitude), out _, Program.Cumulus.SolarOptions);
			var inv = CultureInfo.InvariantCulture;
			var sep = ",";

			var sb = new StringBuilder(256);
			sb.Append(rec.LogTime.ToString("dd/MM/yy", inv) + sep);
			sb.Append(rec.LogTime.ToString("HH:mm", inv) + sep);
			sb.Append(rec.Temperature.ToString(Program.Cumulus.TempFormat, inv) + sep);
			sb.Append(rec.Humidity + sep);
			sb.Append(rec.Dewpoint.ToString(Program.Cumulus.TempFormat, inv) + sep);
			sb.Append(rec.WindSpeed.ToString(Program.Cumulus.WindAvgFormat, inv) + sep);
			sb.Append(rec.WindGust.ToString(Program.Cumulus.WindFormat, inv) + sep);
			sb.Append(rec.WindBearing + sep);
			sb.Append(rec.RainfallRate.ToString(Program.Cumulus.RainFormat, inv) + sep);
			sb.Append(rec.RainfallToday.ToString(Program.Cumulus.RainFormat, inv) + sep);
			sb.Append(rec.Baro.ToString(Program.Cumulus.PressFormat, inv) + sep);
			sb.Append(rec.RainfallCounter.ToString(Program.Cumulus.RainFormat, inv) + sep);
			sb.Append(rec.InsideTemp.ToString(Program.Cumulus.TempFormat, inv) + sep);
			sb.Append(rec.InsideHum + sep);
			sb.Append(rec.CurrentGust.ToString(Program.Cumulus.WindFormat, inv) + sep);
			sb.Append(rec.WindChill.ToString(Program.Cumulus.TempFormat, inv) + sep);
			sb.Append(rec.HeatIndex.ToString(Program.Cumulus.TempFormat, inv) + sep);
			sb.Append(rec.UVI.ToString(Program.Cumulus.UVFormat, inv) + sep);
			sb.Append(rec.SolarRad + sep);
			sb.Append(rec.ET.ToString(Program.Cumulus.ETFormat, inv) + sep);
			sb.Append("0.0" + sep); // annual ET
			sb.Append(rec.ApparentTemp.ToString(Program.Cumulus.TempFormat, inv) + sep);
			sb.Append(rec.SolarMax + sep);
			sb.Append(rec.SunshineHours.ToString(Program.Cumulus.SunFormat, inv) + sep);
			sb.Append(rec.WindBearing + sep);
			sb.Append(rec.RG11Rain.ToString(Program.Cumulus.RainFormat, inv) + sep);
			sb.Append(rec.RainSinceMidnight.ToString(Program.Cumulus.RainFormat, inv) + sep);
			sb.Append(rec.FeelsLike.ToString(Program.Cumulus.TempFormat, inv) + sep);
			sb.Append(rec.Humidex.ToString(Program.Cumulus.TempFormat, inv));

			return sb.ToString();
		}

		private static string GetLogFileName(DateTime thedate)
		{
			return  thedate.ToString("yyyyMM") + "log.txt";
		}

	}

	internal class LogFileRec
	{
		public DateTime LogTime;
		public double Temperature;
		public int Humidity;
		public double Dewpoint;
		public double WindSpeed;
		public double WindGust;
		public int WindBearing;
		public double RainfallRate;
		public double RainfallToday;
		public double Baro;
		public double RainfallCounter;
		public double InsideTemp;
		public double InsideHum;
		public double CurrentGust;
		public double WindChill;
		public double HeatIndex;
		public double UVI;
		public int SolarRad;
		public double ET;
		public double ApparentTemp;
		public double SolarMax;
		public double SunshineHours = 0;
		public int CurrentBearing;
		public double RG11Rain = 0;
		public double RainSinceMidnight;
		public double FeelsLike;
		public double Humidex;
	}
}
