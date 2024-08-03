namespace ImportWLK
{
	internal class WlkDailySummary1
	{
		// Header
		public byte DataType = 2;           //  0: Needs to be 2 for a Daily Summary type 1 record
		//private byte reserved              //  1: padding to make rest of fields start on even addresses

		// Data
		public short DataSpan;              //  2: total # of minutes accounted for by physical records for this day
		public short OutsideTempHi;         //  4: 0.1 F
		public short OutsideTempLow;        //  6: 0.1 F
		public short InsideTempHi;          //  8: 0.1 F
		public short InsideTempLow;         // 10: 0.1 F
		public short OutsideTempAvg;        // 12: 0.1 F
		public short InsideTempAvg;         // 14: 0.1 F
		public short WindChillHi;           // 16: 0.1 F
		public short WindChillLow;          // 18: 0.1 F
		public short DewpointHi;            // 20: 0.1 F
		public short DewpointLow;           // 22: 0.1 F
		public short WindChillAvg;          // 24: 0.1 F
		public short DewpointAvg;           // 26: 0.1 F
		public short OutsideHumidityHi;     // 28: 0.1 %
		public short OutsideHumidityLow;    // 30: 0.1 %
		public short InsideHumidityHi;      // 32: 0.1 %
		public short InsideHumidityLow;     // 34: 0.1 %
		public short OutsideHumidityAvg;    // 36: 0.1 %
		public short BaroHi;                // 38: 0.001 inHg
		public short BaroLow;               // 40; 0.001 inHg
		public short BaroAvg;               // 42: 0.001 inHg
		public short WindGustHi;            // 44: 0.1 mph
		public short WindAvg;           // 46: 0.1 mph
		public short WindRun;               // 48: 0.1 mile
		public short WindAvgHi;           // 50: 0.1 mph
		public byte WindDirGustHi;          // 52: direction code (0-15, dash = 255)
		public byte WindDirAvgHi;           // 53: direction code (0-15, dash = 255)
		public short DailyRainTotal;        // 54: 0.001 in
		public short RainRateHi;            // 56: 0.01 in/hr
		//public short UvDose                // 58: 0.1 MED
		public byte UvHi;                   // 60: 0.1 UV-I
		public byte[] TimeValues;           // 61: 27 bytes, 18 time values
											// 88

		// added for convenience
		public DateTime Date;
		public int[] TimeMins = new int[18];

		public void ReadRecord(BinaryReader reader)
		{
			// The first byte (DataType) will have been read already to determine the record type to decode

			_ = reader.ReadByte();                  // discard padding byte
			DataSpan = reader.ReadInt16();
			OutsideTempHi = reader.ReadInt16();
			OutsideTempLow = reader.ReadInt16();
			InsideTempHi = reader.ReadInt16();
			InsideTempLow = reader.ReadInt16();
			OutsideTempAvg = reader.ReadInt16();
			InsideTempAvg = reader.ReadInt16();
			WindChillHi = reader.ReadInt16();
			WindChillLow = reader.ReadInt16();
			DewpointHi = reader.ReadInt16();
			DewpointLow = reader.ReadInt16();
			WindChillAvg = reader.ReadInt16();
			DewpointAvg = reader.ReadInt16();
			OutsideHumidityHi = reader.ReadInt16();
			OutsideHumidityLow = reader.ReadInt16();
			InsideHumidityHi = reader.ReadInt16();
			InsideHumidityLow = reader.ReadInt16();
			OutsideHumidityAvg = reader.ReadInt16();
			BaroHi = reader.ReadInt16();
			BaroLow = reader.ReadInt16();
			BaroAvg = reader.ReadInt16();
			WindGustHi = reader.ReadInt16();
			WindAvg = reader.ReadInt16();
			WindRun = reader.ReadInt16();
			WindAvgHi = reader.ReadInt16();
			WindDirGustHi = reader.ReadByte();
			WindDirAvgHi = reader.ReadByte();
			DailyRainTotal = reader.ReadInt16();
			RainRateHi = reader.ReadInt16();
			_ = reader.ReadInt16();              // discard UvDose
			UvHi = reader.ReadByte();
			TimeValues = reader.ReadBytes(27);

			for (var i = 0; i < 18; i++)
			{
				var fieldIndex = (i / 2) * 3;
				if (i % 2 == 0)
				{
					TimeMins[i] = TimeValues[fieldIndex] + ((TimeValues[fieldIndex + 2] & 0x0F) << 8);
				}
				else
				{
					TimeMins[i] = TimeValues[fieldIndex + 1] + ((TimeValues[fieldIndex + 2] & 0xF0) << 4);
				}
			}
		}
	}
}
