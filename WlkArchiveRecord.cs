namespace ImportWLK
{
	// Each record is 88 bytes long

	internal class WlkArchiveRecord
	{
		// Header
		public byte Datatype = 1;       //  0: Needs to be 1 for an archive record
		public byte ArchiveInterval;    //  1: Minutes in the record
		public byte Flags1;             //  2: Icon
		public byte Flags2;             //  3: Tx id
		public short PackedTime;        //  4: Minutes after midnight for the end of the record

		// Data
		public short OutsideTemp;       //  6: 0.1 F
		public short OutsideTempHi;     //  8: 0.1 F
		public short OutsideTempLow;    // 10: 0.1 F
		public short InsideTemp;        // 12: 0.1 F
		public short Baro;              // 14: 0.001 inHg
		public short OutsideHumidity;   // 16: 0.1 %
		public short InsideHumidity;    // 18: 0.1 %
		public ushort RainClicks;       // 20: clicks + collector type
		public short RainRateHi;        // 22: clicks/hr
		public short WindSpeed;         // 24: 0.1 mph
		public short WindSpeedGust;     // 26: 0.1 mph
		public byte WindDir;            // 28: Code 0-15, dash value = 255
		public byte WindDirGust;        // 29: Code 0-15, dash value = 255
		public short WindSamples;       // 30: Number of wind packets received in record interval
		public short Solar;             // 32: W/m²
		public short SolarHi;           // 34: W/m²
		public byte UV;                 // 36: 0.1 UV-I
		public byte UVHi;               // 37: 0.1 UV-I
		//public byte[] LeafTemp         // 38: 4 bytes, int F + 90 : NOT USED
		//public short RadiationExtra    // 42: used by THSW index? : NOT USED
		//public byte[] FutureUse        // 44: 12 bytes, reserved : NOT USED
		public byte Forecast;           // 56: VP2 Forecast code
		public byte ET;                 // 57: 0.001 inch
		public byte[] SoilTemp;         // 58: 6 bytes, int F + 90
		public byte[] SoilMoist;        // 64: 6 bytes, centibars
		public byte[] LeafWet;          // 70: 4 bytes, 0-15, dash value = 255
		public byte[] ExtraTemp;        // 76: 7 bytes, int F + 90
		public byte[] ExtraHum;         // 81: 7 bytes, int %
                                        // 88

		// additional fields

		public DateTime Timestamp; // Set externally

		/// <summary>
		/// Rain collector size
		/// 0: 0.1 in, 1: 0.01 in, 2: 0.2 mm, 3: 1.0 mm, 4: 0.1 mm
		/// </summary>
		public int RainCollectorType;
		public int WindTxId;


		public void ReadRecord(BinaryReader reader)
		{
			// The first byte (DataType) will have been read already to determine the record type to decode

			ArchiveInterval = reader.ReadByte();
			Flags1 = reader.ReadByte();
			Flags2 = reader.ReadByte();
			WindTxId = (Flags2 & 0x07);
			PackedTime = reader.ReadInt16();
			OutsideTemp = reader.ReadInt16();
			OutsideTempHi = reader.ReadInt16();
			OutsideTempLow = reader.ReadInt16();
			InsideTemp = reader.ReadInt16();
			Baro = reader.ReadInt16();
			OutsideHumidity = reader.ReadInt16();
			InsideHumidity = reader.ReadInt16();
			var rainCode = reader.ReadUInt16();
			RainCollectorType = (rainCode & 0xF000) / 0x1000;
			RainClicks = (ushort)(rainCode & 0x0FFF);
			RainRateHi = reader.ReadInt16();
			WindSpeed = reader.ReadInt16();
			WindSpeedGust = reader.ReadInt16();
			WindDir = reader.ReadByte();
			WindDirGust = reader.ReadByte();
			WindSamples = reader.ReadInt16();
			Solar = reader.ReadInt16();
			SolarHi = reader.ReadInt16();
			UV = reader.ReadByte();
			UVHi = reader.ReadByte();
			_ = reader.ReadBytes(4);           // discard leaf temps
			_ = reader.ReadInt16();            // discard radiation extra
			_ = reader.ReadBytes(12);          // discard the future sensors
			Forecast = reader.ReadByte();
			ET = reader.ReadByte();
			SoilTemp = reader.ReadBytes(6);
			SoilMoist = reader.ReadBytes(6);
			LeafWet = reader.ReadBytes(4);
			ExtraTemp = reader.ReadBytes(7);
			ExtraHum = reader.ReadBytes(7);
		}
	}
}
