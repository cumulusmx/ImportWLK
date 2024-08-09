namespace ImportWLK
{
	public class Calibrations
	{
		public Calibrations()
		{
			Temp = new Settings();
			InTemp = new Settings();
			Hum = new Settings();
			InHum = new Settings();
			Press = new Settings();
			Rain = new Settings();
			WindSpeed = new Settings();
			WindGust = new Settings();
			WindDir = new Settings();
			Solar = new Settings();
			UV = new Settings();
			WetBulb = new Settings();
		}
		public Settings Temp { get; set; }
		public Settings InTemp { get; set; }
		public Settings Hum { get; set; }
		public Settings InHum { get; set; }
		public Settings Press { get; set; }
		public Settings Rain { get; set; }
		public Settings WindSpeed { get; set; }
		public Settings WindGust { get; set; }
		public Settings WindDir { get; set; }
		public Settings Solar { get; set; }
		public Settings UV { get; set; }
		public Settings WetBulb { get; set; }
	}
	public class Settings
	{
		public double Offset { get; set; } = 0;
		public double Mult { get; set; } = 1;
		public double Mult2 { get; set; } = 0;

		public double Calibrate(double value)
		{
			return value * value * Mult2 + value * Mult + Offset;
		}
	}
}
