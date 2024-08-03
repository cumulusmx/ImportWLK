namespace ImportWLK
{
	struct DayIndex
	{
		public short RecordsInDay;     // includes any daily summary records
		public int StartPosition;     // The index (starting at 0) of the first daily summary record
	};


	internal class WlkFileHeader
	{
		public char[] IdCode;               // 16 bytes, Static header - should be: {'W', 'D', 'A', 'T', '5', '.', '0', 0, 0, 0, 0, 0, 0, 0, 5, 0}
		public int TotalRecords;           // 4 bytes, Total number of records in the file
		public DayIndex[] DayIndices;       // 32 * (2 + 4) = 192 bytes, index records for each day. Index 0 is not used
											// (i.e. the 1'st is at index 1, not index 0)

		public void ReadFile(BinaryReader reader)
		{
			IdCode = reader.ReadChars(16);
			TotalRecords = reader.ReadInt32();

			DayIndices = new DayIndex[32];
			for (int i = 0; i < 32; i++)
			{
				DayIndices[i] = new DayIndex
				{
					RecordsInDay = reader.ReadInt16(),
					StartPosition = reader.ReadInt32()
				};
			}
		}
	}
}
