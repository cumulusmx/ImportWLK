using System.Globalization;
using System.Text;

namespace ImportWLK
{
	class DayFile
	{

		private static readonly string dayfilename = Program.Location + Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "dayfile.txt";
		public static readonly SortedList<DateTime, DayFileRec> Records = [];
		//private static string LineEnding = string.Empty;

		internal static void ReadDayFile()
		{
			if (File.Exists(dayfilename))
			{

				// determine dayfile line ending
				//if (Utils.TryDetectNewLine(dayfilename, out string lineend))
				//{
				//	LineEnding = lineend;
				//}

				var lines = File.ReadAllLines(dayfilename);

				foreach (var line in lines)
				{
					var rec = ParseDayFileRec(line);
					Records.Add(rec.Date, rec);
				}
			}
			else
			{
				Program.LogConsole("No day file found, a new one will be created", ConsoleColor.Cyan);
			}
		}

		internal static void WriteDayFile()
		{
			// backup old dayfile.txt
			if (File.Exists(dayfilename))
			{
				if (!File.Exists(dayfilename + ".sav"))
				{
					File.Move(dayfilename, dayfilename + ".sav");
				}
				else
				{
					var i = 1;
					do
					{
						if (!File.Exists(dayfilename + ".sav" + i))
						{
							File.Move(dayfilename, dayfilename + ".sav" + i);
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
				using FileStream fs = new FileStream(dayfilename, FileMode.Append, FileAccess.Write, FileShare.Read);
				using StreamWriter file = new StreamWriter(fs);
				Program.LogMessage("Dayfile.txt opened for writing");

				foreach (var rec in Records)
				{
					var line = RecToCsv(rec);
					if (null != line)
						file.WriteLine(line);
				}

				file.Close();
			}
			catch (Exception ex)
			{
				Program.LogMessage("Error writing to dayfile.txt: " + ex.Message);
			}
		}

		private static string RecToCsv(KeyValuePair<DateTime, DayFileRec> keyval)
		{
			// Writes an entry to the daily extreme log file. Fields are comma-separated.
			// 0   Date in the form dd/mm/yy (the slash may be replaced by a dash in some cases)
			// 1  Highest wind gust
			// 2  Bearing of highest wind gust
			// 3  Time of highest wind gust
			// 4  Minimum temperature
			// 5  Time of minimum temperature
			// 6  Maximum temperature
			// 7  Time of maximum temperature
			// 8  Minimum sea level pressure
			// 9  Time of minimum pressure
			// 10  Maximum sea level pressure
			// 11  Time of maximum pressure
			// 12  Maximum rainfall rate
			// 13  Time of maximum rainfall rate
			// 14  Total rainfall for the day
			// 15  Average temperature for the day
			// 16  Total wind run
			// 17  Highest average wind speed
			// 18  Time of highest average wind speed
			// 19  Lowest humidity
			// 20  Time of lowest humidity
			// 21  Highest humidity
			// 22  Time of highest humidity
			// 23  Total evapotranspiration
			// 24  Total hours of sunshine
			// 25  High heat index
			// 26  Time of high heat index
			// 27  High apparent temperature
			// 28  Time of high apparent temperature
			// 29  Low apparent temperature
			// 30  Time of low apparent temperature
			// 31  High hourly rain
			// 32  Time of high hourly rain
			// 33  Low wind chill
			// 34  Time of low wind chill
			// 35  High dew point
			// 36  Time of high dew point
			// 37  Low dew point
			// 38  Time of low dew point
			// 39  Dominant wind bearing
			// 40  Heating degree days
			// 41  Cooling degree days
			// 42  High solar radiation
			// 43  Time of high solar radiation
			// 44  High UV Index
			// 45  Time of high UV Index
			// 46  High Feels like
			// 47  Time of high feels like
			// 48  Low feels like
			// 49  Time of low feels like
			// 50  High Humidex
			// 51  Time of high Humidex

			// 52  Chill hours
			// 53  High 24 hr rain
			// 54  Time of high 24 hr rain

			var inv = CultureInfo.InvariantCulture;
			const char sep = ',';
			var rec = keyval.Value;

			// Write the date back using the same separator as the source file
			string datestring = rec.Date.ToString($"dd/MM/yy", inv);
			// NB this string is just for logging, the dayfile update code is further down
			var strb = new StringBuilder(300);
			strb.Append(datestring + sep);

			if (rec.HighGust < -9998)
				return null;
			//strb.Append("0.0" + listsep + "0" + listsep + "00:00" + listsep)
			else
			{
				strb.Append(rec.HighGust.ToString(Program.Cumulus.WindFormat, inv) + sep);
				strb.Append(rec.HighGustBearing.ToString() + sep);
				strb.Append(rec.HighGustTime.ToString("HH:mm", inv) + sep);
			}

			if (rec.LowTemp > 9998)
				return null;
			//strb.Append("0.0" + listsep + "00:00" + listsep)
			else
			{
				strb.Append(rec.LowTemp.ToString(Program.Cumulus.TempFormat, inv) + sep);
				strb.Append(rec.LowTempTime.ToString("HH:mm", inv) + sep);
			}

			if (rec.HighTemp < -9998)
				return null;
			//strb.Append("0.0" + listsep + "00:00" + listsep)
			else
			{
				strb.Append(rec.HighTemp.ToString(Program.Cumulus.TempFormat, inv) + sep);
				strb.Append(rec.HighTempTime.ToString("HH:mm", inv) + sep);
			}

			if (rec.LowPress > 9998)
				return null;
			//strb.Append("0.0" + listsep + "00:00" + listsep)
			else
			{
				strb.Append(rec.LowPress.ToString(Program.Cumulus.PressFormat, inv) + sep);
				strb.Append(rec.LowPressTime.ToString("HH:mm", inv) + sep);
			}

			if (rec.HighPress < -9998)
				return null;
			//strb.Append("0.0" + listsep + "00:00" + listsep)
			else
			{
				strb.Append(rec.HighPress.ToString(Program.Cumulus.PressFormat, inv) + sep);
				strb.Append(rec.HighPressTime.ToString("HH:mm", inv) + sep);
			}

			if (rec.HighRainRate < -9998)
				return null;
			//strb.Append("0.0" + listsep + "00:00" + listsep)
			else
			{
				strb.Append(rec.HighRainRate.ToString(Program.Cumulus.RainFormat, inv) + sep);
				strb.Append(rec.HighRainRateTime.ToString("HH:mm", inv) + sep);
			}

			if (rec.TotalRain < -9998)
				return null;
			//strb.Append("0.0" + listsep)
			else
				strb.Append(rec.TotalRain.ToString(Program.Cumulus.RainFormat, inv) + sep);

			if (rec.AvgTemp < -9998)
				strb.Append(sep);
			else
				strb.Append(rec.AvgTemp.ToString(Program.Cumulus.TempFormat, inv) + sep);


			strb.Append(rec.WindRun.ToString("F1", inv) + sep);

			if (rec.HighAvgWind < -9998)
				strb.Append(sep, 2);
			else
			{
				strb.Append(rec.HighAvgWind.ToString(Program.Cumulus.WindAvgFormat, inv) + sep);
				strb.Append(rec.HighAvgWindTime.ToString("HH:mm", inv) + sep);
			}

			if (rec.LowHumidity == 9999)
				strb.Append(sep, 2);
			else
			{
				strb.Append(rec.LowHumidity.ToString() + sep);
				strb.Append(rec.LowHumidityTime.ToString("HH:mm", inv) + sep);
			}

			if (rec.HighHumidity == -9999)
				strb.Append(sep, 2);
			else
			{
				strb.Append(rec.HighHumidity.ToString() + sep);
				strb.Append(rec.HighHumidityTime.ToString("HH:mm", inv) + sep);
			}

			if (rec.ET < -9998)
				strb.Append(sep);
			else
				strb.Append(rec.ET.ToString(Program.Cumulus.ETFormat, inv) + sep);

			if (rec.SunShineHours < -9998)
				strb.Append(sep);
			else
				strb.Append(rec.SunShineHours.ToString(Program.Cumulus.SunFormat, inv) + sep);

			if (rec.HighHeatIndex < -9998)
				strb.Append(sep, 2);
			else
			{
				strb.Append(rec.HighHeatIndex.ToString(Program.Cumulus.TempFormat, inv) + sep);
				strb.Append(rec.HighHeatIndexTime.ToString("HH:mm", inv) + sep);
			}

			if (rec.HighAppTemp < -9998)
				strb.Append(sep, 2);
			else
			{
				strb.Append(rec.HighAppTemp.ToString(Program.Cumulus.TempFormat, inv) + sep);
				strb.Append(rec.HighAppTempTime.ToString("HH:mm", inv) + sep);
			}

			if (rec.LowAppTemp > 9998)
				strb.Append(sep, 2);
			else
			{
				strb.Append(rec.LowAppTemp.ToString(Program.Cumulus.TempFormat, inv) + sep);
				strb.Append(rec.LowAppTempTime.ToString("HH:mm", inv) + sep);
			}

			if (rec.HighHourlyRain < 0)
				strb.Append(sep, 2);
			else
			{
				strb.Append(rec.HighHourlyRain.ToString(Program.Cumulus.RainFormat, inv) + sep);
				strb.Append(rec.HighHourlyRainTime.ToString("HH:mm", inv) + sep);
			}

			if (rec.LowWindChill > 9998)
				strb.Append(sep, 2);
			else
			{
				strb.Append(rec.LowWindChill.ToString(Program.Cumulus.TempFormat, inv) + sep);
				strb.Append(rec.LowWindChillTime.ToString("HH:mm", inv) + sep);
			}

			if (rec.HighDewPoint < -9998)
				strb.Append(sep, 2);
			else
			{
				strb.Append(rec.HighDewPoint.ToString(Program.Cumulus.TempFormat, inv) + sep);
				strb.Append(rec.HighDewPointTime.ToString("HH:mm", inv) + sep);
			}

			if (rec.LowDewPoint < -9998)
				strb.Append(sep, 2);
			else
			{
				strb.Append(rec.LowDewPoint.ToString(Program.Cumulus.TempFormat, inv) + sep);
				strb.Append(rec.LowDewPointTime.ToString("HH:mm", inv) + sep);
			}

			if (rec.DominantWindBearing < 0 || rec.DominantWindBearing == 9999)
				strb.Append(sep);
			else
				strb.Append(rec.DominantWindBearing.ToString() + sep);

			if (rec.HeatingDegreeDays < -9998)
				strb.Append(sep);
			else
				strb.Append(rec.HeatingDegreeDays.ToString("F1", inv) + sep);

			if (rec.CoolingDegreeDays < -9998)
				strb.Append(sep);
			else
				strb.Append(rec.CoolingDegreeDays.ToString("F1", inv) + sep);

			if (rec.HighSolar < 0 || rec.HighSolar > 9998)
				strb.Append(sep, 2);
			else
			{
				strb.Append(rec.HighSolar.ToString() + sep);
				strb.Append(rec.HighSolarTime.ToString("HH:mm", inv) + sep);
			}

			if (rec.HighUv < 0)
				strb.Append(sep, 2);
			else
			{
				strb.Append(rec.HighUv.ToString(Program.Cumulus.UVFormat, inv) + sep);
				strb.Append(rec.HighUvTime.ToString("HH:mm", inv) + sep);
			}

			if (rec.HighFeelsLike < -9998)
				strb.Append(sep, 2);
			else
			{
				strb.Append(rec.HighFeelsLike.ToString(Program.Cumulus.TempFormat, inv) + sep);
				strb.Append(rec.HighFeelsLikeTime.ToString("HH:mm", inv) + sep);
			}

			if (rec.LowFeelsLike > 9998 || rec.LowFeelsLike < -9998)
				strb.Append(sep, 2);
			else
			{
				strb.Append(rec.LowFeelsLike.ToString(Program.Cumulus.TempFormat, inv) + sep);
				strb.Append(rec.LowFeelsLikeTime.ToString("HH:mm", inv) + sep);
			}

			if (rec.HighHumidex < -9998)
				strb.Append(sep, 2);
			else
			{
				strb.Append(rec.HighHumidex.ToString(Program.Cumulus.TempFormat, inv) + sep);
				strb.Append(rec.HighHumidexTime.ToString("HH:mm", inv) + sep);
			}

			if (rec.ChillHours < 0)
				strb.Append(sep);
			else
				strb.Append(rec.ChillHours.ToString("F1", inv) + sep);

			if (rec.HighRain24h < 0)
				strb.Append(sep, 2);
			else
			{
				strb.Append(rec.HighRain24h.ToString(Program.Cumulus.TempFormat, inv));
				strb.Append(rec.HighRain24hTime.ToString("HH:mm", inv));
			}

			Program.LogMessage("Dayfile.txt Added: " + datestring);

			return strb.ToString();
		}

		internal static void AddRecord1(WlkDailySummary1 rec)
		{
			if (!Records.TryGetValue(rec.Date, out var value))
			{
				value = new DayFileRec() { Date = rec.Date };
				Records.Add(rec.Date, value);
			}

			var val = rec.OutsideTempHi / 10.0;
			var conv = Program.Cumulus.Calib.Temp.Calibrate(ConvertUnits.TempFToUser(val));
			if (val > -150 && val < 250 && conv > value.HighTemp)
			{
				value.HighTemp = conv;
				value.HighTempTime = rec.Date.AddMinutes(rec.TimeMins[0]);
			}

			val = rec.OutsideTempLow / 10.0;
			conv = Program.Cumulus.Calib.Temp.Calibrate(ConvertUnits.TempFToUser(val));
			if (val > -150 && val < 250 && conv < value.LowTemp)
			{
				value.LowTemp =conv;
				value.LowTempTime = rec.Date.AddMinutes(rec.TimeMins[1]);
			}

			val = rec.OutsideTempAvg / 10.0;
			conv = Program.Cumulus.Calib.Temp.Calibrate(ConvertUnits.TempFToUser(val));
			if (val > -150 && val < 250)
			{
				value.AvgTemp = conv;
			}

				// Not used
				// 2 = Hi Inside temp
				// 3 = Lo Inside temp
				// 4 = Hi Wind chill

			val = rec.WindChillLow / 10.0;
			conv = ConvertUnits.TempFToUser(val);
			if (val > -150 && val < 250 && conv < value.LowWindChill)
			{
				value.LowWindChill = conv;
				value.LowWindChillTime = rec.Date.AddMinutes(rec.TimeMins[5]);
			}

			val = rec.DewpointHi / 10.0;
			conv = ConvertUnits.TempFToUser(val);
			if (val > -150 && val < 250 && conv > value.HighDewPoint)
			{
				value.HighDewPoint = conv;
				value.HighDewPointTime = rec.Date.AddMinutes(rec.TimeMins[6]);
			}

			val = rec.DewpointLow / 10.0;
			conv = ConvertUnits.TempFToUser(val);
			if (val > -150 && val < 250 && conv < value.LowDewPoint)
			{
				value.LowDewPoint = conv;
				value.LowDewPointTime = rec.Date.AddMinutes(rec.TimeMins[7]);
			}

			val = Program.Cumulus.Calib.Hum.Calibrate(rec.OutsideHumidityHi / 10.0);
			if (val >= 0 && val <= 100 && val > value.HighHumidity)
			{
				value.HighHumidity = (int) val;
				value.HighHumidityTime = rec.Date.AddMinutes(rec.TimeMins[8]);
			}

			val = Program.Cumulus.Calib.Hum.Calibrate(rec.OutsideHumidityLow / 10.0);
			if (val >= 0 && val <= 100 && val < value.LowHumidity)
			{
				value.LowHumidity = (int) val;
				value.LowHumidityTime = rec.Date.AddMinutes(rec.TimeMins[9]);
			}

			// Not used
			// 10 = Hi Inside humidity
			// 11 = Lo Inside humidity

			val = rec.BaroHi / 1000.0;
			conv = Program.Cumulus.Calib.Press.Calibrate(ConvertUnits.PressINHGToUser(val));
			if (val> 25 && val < 32.5 && conv > value.HighPress)
			{
				value.HighPress = conv;
				value.HighPressTime = rec.Date.AddMinutes(rec.TimeMins[12]);
			}

			val = Program.Cumulus.Calib.Press.Calibrate(rec.BaroLow / 1000.0);
			conv = ConvertUnits.PressINHGToUser(val);
			if (val > 25 && val < 32.5 && conv < value.LowPress)
			{
				value.LowPress = conv;
				value.LowPressTime = rec.Date.AddMinutes(rec.TimeMins[13]);
			}

			val = rec.WindGustHi / 10.0;
			conv = Program.Cumulus.Calib.WindGust.Calibrate(ConvertUnits.WindMPHToUser(val));
			if (val >= 0 && val < 200 && conv > value.HighGust)
			{
				value.HighGust = conv;
				value.HighGustTime = rec.Date.AddMinutes(rec.TimeMins[14]);

				if (rec.WindDirGustHi != 255)
				{
					value.HighGustBearing = (rec.WindDirGustHi * 360) / 16;
				}
			}

			val = rec.WindAvgHi / 10.0;
			conv = Program.Cumulus.Calib.WindSpeed.Calibrate(ConvertUnits.WindMPHToUser(val));
			if (val >= 0 && val < 200 && conv > value.HighAvgWind)
			{
				value.HighAvgWind = conv;
				value.HighAvgWindTime = rec.Date.AddMinutes(rec.TimeMins[15]);
			}

			val = rec.RainRateHi / 1000.0;
			conv = Program.Cumulus.Calib.Rain.Calibrate(ConvertUnits.RainINToUser(val));
			if (val >= 0 && val < 300 && conv > value.HighRainRate)
			{
				value.HighRainRate = conv;
				value.HighRainRateTime = rec.Date.AddMinutes(rec.TimeMins[16]);
			}

			val = Program.Cumulus.Calib.UV.Calibrate(rec.UvHi / 10.0);
			if (val >= 0 && val < 20 && conv > value.HighUv)
			{
				value.HighUv = val;
				value.HighUvTime = rec.Date.AddMinutes(rec.TimeMins[17]);
			}

			val = Program.Cumulus.Calib.Rain.Calibrate(rec.DailyRainTotal / 1000.0);
			conv = ConvertUnits.RainINToUser(rec.DailyRainTotal / 1000.0);
			if (val >= 0 && val < 32 && conv > value.TotalRain)
			{
				value.TotalRain = conv;
			}

			val = Program.Cumulus.Calib.WindSpeed.Calibrate(rec.WindRun / 10.0);
			conv = ConvertUnits.MilesToUserUnits(rec.WindRun / 10.0);
			if (val >= 0 && val < 3200 && conv > value.WindRun)
			{
				value.WindRun = conv;
			}
		}

		internal static void AddRecord2(WlkDailySummary2 rec)
		{
			if (!Records.TryGetValue(rec.Date, out var value))
			{
				value = new DayFileRec();
				Records.Add(rec.Date, value);
			}

			if (rec.SolarHi != short.MaxValue)
			{
				value.HighSolar = rec.SolarHi;
				value.HighSolarTime = rec.Date.AddMinutes(rec.TimeMins[0]);
			}

			var val = rec.DailyET / 1000.0;
			var conv = ConvertUnits.RainINToUser(val);
			if (val >= 0 && val < 320)
			{
				value.ET = conv;
			}

			val = rec.HeatIndexHi / 10.0;
			conv = ConvertUnits.TempFToUser(val);
			if (val > -150 && val < 250)
			{
				value.HighHeatIndex = conv;
				value.HighHeatIndexTime = rec.Date.AddMinutes(rec.TimeMins[1]);
			}

			val = rec.HeatDegreeDays65 / 10.0;
			conv = ConvertUnits.DegreeDaysFtoUser(val);
			if (val > -320 && val < 320)
			{
				value.HeatingDegreeDays = conv;
			}

			val = rec.CoolDegreeDays65 / 10.0;
			conv = ConvertUnits.DegreeDaysFtoUser(val);
			if (val > -320 && val < 320)
			{
				value.CoolingDegreeDays = conv;
			}
		}


		internal static DayFileRec ParseDayFileRec(string data)
		{
			var inv = CultureInfo.InvariantCulture;
			var st = new List<string>(data.Split(','));
			double varDbl;
			int idx = 0;

			var rec = new DayFileRec();
			try
			{
				rec.Date = Utils.DdmmyyStrToDate(st[idx++]);
				rec.HighGust = Convert.ToDouble(st[idx++], inv);
				rec.HighGustBearing = Convert.ToInt32(st[idx++]);
				rec.HighGustTime = Utils.GetDateTime(rec.Date, st[idx++], Program.Cumulus.RolloverHour);
				rec.LowTemp = Convert.ToDouble(st[idx++], inv);
				rec.LowTempTime = Utils.GetDateTime(rec.Date, st[idx++], Program.Cumulus.RolloverHour);
				rec.HighTemp = Convert.ToDouble(st[idx++], inv);
				rec.HighTempTime = Utils.GetDateTime(rec.Date, st[idx++], Program.Cumulus.RolloverHour);
				rec.LowPress = Convert.ToDouble(st[idx++], inv);
				rec.LowPressTime = Utils.GetDateTime(rec.Date, st[idx++], Program.Cumulus.RolloverHour);
				rec.HighPress = Convert.ToDouble(st[idx++], inv);
				rec.HighPressTime = Utils.GetDateTime(rec.Date, st[idx++], Program.Cumulus.RolloverHour);
				rec.HighRainRate = Convert.ToDouble(st[idx++], inv);
				rec.HighRainRateTime = Utils.GetDateTime(rec.Date, st[idx++], Program.Cumulus.RolloverHour);
				rec.TotalRain = Convert.ToDouble(st[idx++], inv);
				rec.AvgTemp = Convert.ToDouble(st[idx++], inv);

				if (st.Count > idx++ && double.TryParse(st[16], inv, out varDbl))
					rec.WindRun = varDbl;

				if (st.Count > idx++ && double.TryParse(st[17], inv, out varDbl))
					rec.HighAvgWind = varDbl;

				if (st.Count > idx++ && st[18].Length == 5)
					rec.HighAvgWindTime = Utils.GetDateTime(rec.Date, st[18], Program.Cumulus.RolloverHour);

				if (st.Count > idx++ && double.TryParse(st[19], inv, out varDbl))
					rec.LowHumidity = Convert.ToInt32(varDbl);
				else
					rec.LowHumidity = int.MaxValue;

				if (st.Count > idx++ && st[20].Length == 5)
					rec.LowHumidityTime = Utils.GetDateTime(rec.Date, st[20], Program.Cumulus.RolloverHour);

				if (st.Count > idx++ && double.TryParse(st[21], inv, out varDbl))
					rec.HighHumidity = Convert.ToInt32(varDbl);
				else
					rec.HighHumidity = int.MinValue;

				if (st.Count > idx++ && st[22].Length == 5)
					rec.HighHumidityTime = Utils.GetDateTime(rec.Date, st[22], Program.Cumulus.RolloverHour);

				if (st.Count > idx++ && double.TryParse(st[23], inv, out varDbl))
					rec.ET = varDbl;
				else
					rec.ET = double.MaxValue;

				if (st.Count > idx++ && double.TryParse(st[24], inv, out varDbl))
					rec.SunShineHours = varDbl;

				if (st.Count > idx++ && double.TryParse(st[25], inv, out varDbl))
					rec.HighHeatIndex = varDbl;
				else
					rec.HighHeatIndex = double.MinValue;

				if (st.Count > idx++ && st[26].Length == 5)
					rec.HighHeatIndexTime = Utils.GetDateTime(rec.Date, st[26], Program.Cumulus.RolloverHour);

				if (st.Count > idx++ && double.TryParse(st[27], inv, out varDbl))
					rec.HighAppTemp = varDbl;
				else
					rec.HighAppTemp = double.MinValue;

				if (st.Count > idx++ && st[28].Length == 5)
					rec.HighAppTempTime = Utils.GetDateTime(rec.Date, st[28], Program.Cumulus.RolloverHour);

				if (st.Count > idx++ && double.TryParse(st[29], inv, out varDbl))
					rec.LowAppTemp = varDbl;
				else
					rec.LowAppTemp = double.MaxValue;

				if (st.Count > idx++ && st[30].Length == 5)
					rec.LowAppTempTime = Utils.GetDateTime(rec.Date, st[30], Program.Cumulus.RolloverHour);

				if (st.Count > idx++ && double.TryParse(st[31], inv, out varDbl))
					rec.HighHourlyRain = varDbl;
				else
					rec.HighHourlyRain = double.MinValue;

				if (st.Count > idx++ && st[32].Length == 5)
					rec.HighHourlyRainTime = Utils.GetDateTime(rec.Date, st[32], Program.Cumulus.RolloverHour);

				if (st.Count > idx++ && double.TryParse(st[33], inv, out varDbl))
					rec.LowWindChill = varDbl;
				else
					rec.LowWindChill = double.MaxValue;

				if (st.Count > idx++ && st[34].Length == 5)
					rec.LowWindChillTime = Utils.GetDateTime(rec.Date, st[34], Program.Cumulus.RolloverHour);

				if (st.Count > idx++ && double.TryParse(st[35], inv, out varDbl))
					rec.HighDewPoint = varDbl;
				else
					rec.HighDewPoint = double.MinValue;

				if (st.Count > idx++ && st[36].Length == 5)
					rec.HighDewPointTime = Utils.GetDateTime(rec.Date, st[36], Program.Cumulus.RolloverHour);

				if (st.Count > idx++ && double.TryParse(st[37], inv, out varDbl))
					rec.LowDewPoint = varDbl;
				else
					rec.LowDewPoint = double.MaxValue;

				if (st.Count > idx++ && st[38].Length == 5)
					rec.LowDewPointTime = Utils.GetDateTime(rec.Date, st[38], Program.Cumulus.RolloverHour);

				if (st.Count > idx++ && double.TryParse(st[39], inv, out varDbl))
					rec.DominantWindBearing = Convert.ToInt32(varDbl);
				else
					rec.DominantWindBearing = int.MinValue;

				if (st.Count > idx++ && double.TryParse(st[40], inv, out varDbl))
					rec.HeatingDegreeDays = varDbl;
				else
					rec.HeatingDegreeDays = double.MinValue;

				if (st.Count > idx++ && double.TryParse(st[41], inv, out varDbl))
					rec.CoolingDegreeDays = varDbl;
				else
					rec.CoolingDegreeDays = double.MinValue;

				if (st.Count > idx++ && double.TryParse(st[42], inv, out varDbl))
					rec.HighSolar = Convert.ToInt32(varDbl);
				else
					rec.HighSolar = int.MinValue;

				if (st.Count > idx++ && st[43].Length == 5)
					rec.HighSolarTime = Utils.GetDateTime(rec.Date, st[43], Program.Cumulus.RolloverHour);

				if (st.Count > idx++ && double.TryParse(st[44], inv, out varDbl))
					rec.HighUv = varDbl;
				else
					rec.HighUv = double.MinValue;

				if (st.Count > idx++ && st[45].Length == 5)
					rec.HighUvTime = Utils.GetDateTime(rec.Date, st[45], Program.Cumulus.RolloverHour);

				if (st.Count > idx++ && double.TryParse(st[46], inv, out varDbl))
					rec.HighFeelsLike = varDbl;
				else
					rec.HighFeelsLike = double.MinValue;

				if (st.Count > idx++ && st[47].Length == 5)
					rec.HighFeelsLikeTime = Utils.GetDateTime(rec.Date, st[47], Program.Cumulus.RolloverHour);

				if (st.Count > idx++ && double.TryParse(st[48], inv, out varDbl))
					rec.LowFeelsLike = varDbl;
				else
					rec.LowFeelsLike = double.MaxValue;

				if (st.Count > idx++ && st[49].Length == 5)
					rec.LowFeelsLikeTime = Utils.GetDateTime(rec.Date, st[49], Program.Cumulus.RolloverHour);

				if (st.Count > idx++ && double.TryParse(st[50], inv, out varDbl))
					rec.HighHumidex = varDbl;
				else
					rec.HighHumidex = double.MinValue;

				if (st.Count > idx++ && st[51].Length == 5)
					rec.HighHumidexTime = Utils.GetDateTime(rec.Date, st[51], Program.Cumulus.RolloverHour);

				if (st.Count > idx++ && double.TryParse(st[52], inv, out varDbl))
					rec.ChillHours = varDbl;
				else
					rec.ChillHours = double.MinValue;

				if (st.Count > idx++ && double.TryParse(st[53], inv, out varDbl))
					rec.HighRain24h = varDbl;
				else
					rec.HighRain24h = double.MinValue;

				if (st.Count > idx++ && st[54].Length == 5)
					rec.HighRain24hTime = Utils.GetDateTime(rec.Date, st[54], Program.Cumulus.RolloverHour);
			}
			catch (Exception ex)
			{
				Program.LogMessage($"ParseDayFileRec: Error at record {idx} - {ex.Message}");
				var e = new Exception($"Error at record {idx} = \"{st[idx - 1]}\" - {ex.Message}");
				throw e;
			}
			return rec;
		}


		internal class DayFileRec
		{
			public DateTime Date;
			public double HighGust;
			public int HighGustBearing;
			public DateTime HighGustTime;
			public double LowTemp;
			public DateTime LowTempTime;
			public double HighTemp;
			public DateTime HighTempTime;
			public double LowPress;
			public DateTime LowPressTime;
			public double HighPress;
			public DateTime HighPressTime;
			public double HighRainRate;
			public DateTime HighRainRateTime;
			public double TotalRain;
			public double AvgTemp;
			public double WindRun;
			public double HighAvgWind;
			public DateTime HighAvgWindTime;
			public int LowHumidity;
			public DateTime LowHumidityTime;
			public int HighHumidity;
			public DateTime HighHumidityTime;
			public double ET;
			public double SunShineHours;
			public double HighHeatIndex;
			public DateTime HighHeatIndexTime;
			public double HighAppTemp;
			public DateTime HighAppTempTime;
			public double LowAppTemp;
			public DateTime LowAppTempTime;
			public double HighHourlyRain;
			public DateTime HighHourlyRainTime;
			public double LowWindChill;
			public DateTime LowWindChillTime;
			public double HighDewPoint;
			public DateTime HighDewPointTime;
			public double LowDewPoint;
			public DateTime LowDewPointTime;
			public int DominantWindBearing;
			public double HeatingDegreeDays;
			public double CoolingDegreeDays;
			public int HighSolar;
			public DateTime HighSolarTime;
			public double HighUv;
			public DateTime HighUvTime;
			public double HighFeelsLike;
			public DateTime HighFeelsLikeTime;
			public double LowFeelsLike;
			public DateTime LowFeelsLikeTime;
			public double HighHumidex;
			public DateTime HighHumidexTime;
			public double ChillHours;
			public double HighRain24h;
			public DateTime HighRain24hTime;

			public DayFileRec()
			{
				HighGust = -9999;
				HighGustBearing = 0;
				LowTemp = 9999;
				HighTemp = -9999;
				LowPress = 9999;
				HighPress = -9999;
				HighRainRate = -9999;
				TotalRain = -9999;
				AvgTemp = -9999;
				WindRun = -9999;
				HighAvgWind = -9999;
				LowHumidity = 9999;
				HighHumidity = -9999;
				ET = -9999;
				SunShineHours = -9999;
				HighHeatIndex = -9999;
				HighAppTemp = -9999;
				LowAppTemp = 9999;
				HighHourlyRain = -9999;
				LowWindChill = 9999;
				HighDewPoint = -9999;
				LowDewPoint = 9999;
				DominantWindBearing = 9999;
				HeatingDegreeDays = -9999;
				CoolingDegreeDays = -9999;
				HighSolar = -9999;
				HighUv = -9999;
				HighFeelsLike = -9999;
				LowFeelsLike = 9999;
				HighHumidex = -9999;
				ChillHours = -9999;
				HighRain24h = -9999;
			}
		}
	}
}


