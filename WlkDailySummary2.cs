namespace ImportWLK
{
	internal class WlkDailySummary2
	{
		// Header
		public byte DataType = 3;           //  0: Needs to be 3 for a Daily Summary type 1 record
		//private byte reserved              //  1: padding to make rest of fields start on even addresses

		// Data
		//public ushort TodaysWeather            //  2: Not used
		public short NumWindPackets;            //  4: # of valid packets containing wind data
		public short SolarHi;                   //  6: W/m²
		//public short DailySolarEnergy          //  8: 0.1 Ly - Not used
		public short SunLightMins;              // 10: number of accumulated minutes where the avg solar rad > 150
		public short DailyET;                   // 12: 0.001 in
		public short HeatIndexHi;               // 14: 0.1 F
		public short HeatIndexLow;              // 16: 0.1 F
		public short HeatIndexAvg;              // 18: 0.1 F
		public short ThswHi;                    // 20: 0.1 F
		public short ThswLow;                   // 22: 0.1 F
		public short ThwHi;                     // 24: 0.1 F
		public short ThwLow;                    // 26: 0.1 F
		public short HeatDegreeDays65;          // 28: 0.1 F, integrated Heating Degree Days (65F threshold)

		// wet bulb values are not calculated
		//public short WetBulbHi                 // 30: 0.1 F
		//public short WetBulbLow                // 32: 0.1 F
		//public short WetBulbAvg                // 34: 0.1 F
		public byte[] DirBins;                  // 36: 24 bytes, 16 direction bins (1.5 bytes each)
		public byte[] TimeValues;               // 60: 15 bytes, 10 time values
		public short CoolDegreeDays65;          // 75: 0.1 F, integrated Cooling Degree Days (65F threshold)
		//public byte reserved2                  // 77: 11 bytes
												// 88

		// added for convenience
		public DateTime Date;
		public int[] TimeMins = new int[10];

		public void ReadRecord(BinaryReader reader)
		{
			// The first byte (DataType) will have been read already to determine the record type to decode

			_ = reader.ReadByte();              // discard reserved
			_ = reader.ReadUInt16();            // discard TodaysWeather
			NumWindPackets = reader.ReadInt16();
			SolarHi = reader.ReadInt16();
			_ = reader.ReadInt16();             // discard DailySolarEnergy
			SunLightMins = reader.ReadInt16();
			DailyET = reader.ReadInt16();
			HeatIndexHi = reader.ReadInt16();
			HeatIndexLow = reader.ReadInt16();
			HeatIndexAvg = reader.ReadInt16();
			ThswHi = reader.ReadInt16();
			ThswLow = reader.ReadInt16();
			ThwHi = reader.ReadInt16();
			ThwLow = reader.ReadInt16();
			HeatDegreeDays65 = reader.ReadInt16();
			_ = reader.ReadInt16();             // discard WetBulbHi
			_ = reader.ReadInt16();             // discard WetBulbLow
			_ = reader.ReadInt16();             // discard WetBulbAvg
			DirBins = reader.ReadBytes(24);
			TimeValues = reader.ReadBytes(15);
			CoolDegreeDays65 = reader.ReadInt16();
			_ = reader.ReadBytes(11);

			for (var i = 0; i < 10; i++)
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
