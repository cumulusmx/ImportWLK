Data File Structure

What follows is a technical description of the .WLK weather database files.
This is of interest mostly to programmers who want to write their own programs to read the data files.

The data filename has the following format:
    YYYY-MM.wlk
    where YYYY is the four digit year and MM is the two digit month of the data contained in the file.

The structures defined below assume that no bytes are added to the structures to make the fields are on the "correct" address boundaries.
With the Microsoft C++ compiler, you can use the directive "#pragma pack (1)" to enforce this and use "#pragma pack ()" to return the compiler to its default behavior.


// Data is stored in monthly files. Each file has the following header.
struct DayIndex
{
    short recordsInDay;  // includes any daily summary records
    long startPos;       // The index (starting at 0) of the first daily summary record
};

// Header for each monthly file.
// The first 16 bytes are used to identify a weather database file and to identify
// different file formats. (Used for converting older database files.)
class HeaderBlock
{
    char idCode [16];           // = {'W', 'D', 'A', 'T', '5', '.', '0', 0, 0, 0, 0, 0, 0, 0, 5, 0}
    long totalRecords;
    DayIndex dayIndex [32];     // index records for each day. Index 0 is not used
                                // (i.e. the 1'st is at index 1, not index 0)
};


// After the Header are a series of 88 byte data records with one of the following
// formats. Note that each day will begin with 2 daily summary records


// Daily Summary Record 1
struct DailySummary1
{
    BYTE dataType = 2;
    BYTE reserved;                  // this will cause the rest of the fields to start on an even address

    short dataSpan;                 // total # of minutes accounted for by physical records for this day
    short hiOutTemp, lowOutTemp;    // tenths of a degree F
    short hiInTemp, lowInTemp;      // tenths of a degree F
    short avgOutTemp, avgInTemp;    // tenths of a degree F (integrated over the day)
    short hiChill, lowChill;        // tenths of a degree F
    short hiDew, lowDew;            // tenths of a degree F
    short avgChill, avgDew;         // tenths of a degree F
    short hiOutHum, lowOutHum;      // tenths of a percent
    short hiInHum, lowInHum;        // tenths of a percent
    short avgOutHum;                // tenths of a percent
    short hiBar, lowBar;            // thousandths of an inch Hg
    short avgBar;                   // thousandths of an inch Hg
    short hiSpeed, avgSpeed;        // tenths of an MPH
    short dailyWindRunTotal;        // 1/10'th of an mile
    short hi10MinSpeed;             // the highest average wind speed record
    BYTE dirHiSpeed, hi10MinDir;    // direction code (0-15, 255)
    short dailyRainTotal;           // 1/1000'th of an inch
    short hiRainRate;               // 1/100'th inch/hr ???
    short dailyUVDose;              // 1/10'th of a standard MED
    BYTE hiUV;                      // tenth of a UV Index
    BYTE timeValues[27];            // space for 18 time values (see below)
};


// Daily Summary Record 2
struct DailySummary2
{
    BYTE dataType = 3;
    BYTE reserved;                  // this will cause the rest of the fields to start on an even address

    // this field is not used now.
    unsigned short todaysWeather;   // bitmapped weather conditions (Fog, T-Storm, hurricane, etc)

    short numWindPackets;           // # of valid packets containing wind data,
                                    // this is used to indicate reception quality
    short hiSolar;                  // Watts per meter squared
    short dailySolarEnergy;         // 1/10'th Ly
    short minSunlight;              // number of accumulated minutes where the avg solar rad > 150
    short dailyETTotal;             // 1/1000'th of an inch
    short hiHeat, lowHeat;          // tenths of a degree F
    short avgHeat;                  // tenths of a degree F
    short hiTHSW, lowTHSW;          // tenths of a degree F
    short hiTHW, lowTHW;            // tenths of a degree F

    short integratedHeatDD65;       // integrated Heating Degree Days (65F threshold)
                                    // tenths of a degree F - Day

    // Wet bulb values are not calculated
    short hiWetBulb, lowWetBulb;    // tenths of a degree F
    short avgWetBulb;               // tenths of a degree F

    BYTE dirBins[24];               // space for 16 direction bins // (Used to calculate monthly dominant Dir)

    BYTE timeValues[15];            // space for 10 time values (see below)

    short integratedCoolDD65;       // integrated Cooling Degree Days (65F threshold)
                                    // tenths of a degree F - Day
    BYTE reserved2[11];
};


// standard archive record

struct WeatherDataRecord
{
    BYTE dataType = 1;
    BYTE archiveInterval;       // number of minutes in the archive

    // see below for more details about these next two fields)
    BYTE iconFlags;             // Icon associated with this record, plus Edit flags
    BYTE moreFlags;             // Tx Id, etc.

    short packedTime;           // minutes past midnight of the end of the archive period
    short outsideTemp;          // tenths of a degree F
    short hiOutsideTemp;        // tenths of a degree F
    short lowOutsideTemp;       // tenths of a degree F
    short insideTemp;           // tenths of a degree F
    short barometer;            // thousandths of an inch Hg
    short outsideHum;           // tenths of a percent
    short insideHum;            // tenths of a percent
    unsigned short rain;        // number of clicks + rain collector type code
    short hiRainRate;           // clicks per hour
    short windSpeed;            // tenths of an MPH
    short hiWindSpeed;          // tenths of an MPH
    BYTE windDirection;         // direction code (0-15, 255)
    BYTE hiWindDirection;       // direction code (0-15, 255)
    short numWindSamples;       // number of valid ISS packets containing wind data
                                // this is a good indication of reception
    short solarRad, hisolarRad; // Watts per meter squared
    BYTE UV, hiUV;              // tenth of a UV Index
    BYTE leafTemp[4];           // (whole degrees F) + 90
    short extraRad;             // used to calculate extra heating effects of the sun in THSW index
    short newSensors[6];        // reserved for future use
    BYTE forecast;              // forecast code during the archive interval
    BYTE ET;                    // in thousandths of an inch
    BYTE soilTemp[6];           // (whole degrees F) + 90
    BYTE soilMoisture[6];       // centibars of dryness
    BYTE leafWetness[4];        // Leaf Wetness code (0-15, 255)
    BYTE extraTemp[7];          // (whole degrees F) + 90
    BYTE extraHum[7];           // whole percent
};

Notes:

Always check the dataType field to make sure you are reading the correct record type

There are extra fields that are not used by the current software.
For example, there is space for 7 extra temperatures and Hums, but current Vantage stations only log data for 3 extra temps and 2 extra hums.

Extra/Soil/Leaf temperatures are in whole degrees with a 90 degree offset. A database value of 0 = -90 F, 100 = 10 F, etc.

The rain collector type is encoded in the most significant nibble of the rain field.
    rainCollectorType = (rainCode & 0xF000);
    rainClicks = (rainCode & 0x0FFF);

Type rainCollectorType
    0.1 inch 0x0000
    0.01 inch 0x1000
    0.2 mm 0x2000
    1.0 mm 0x3000
    0.1 mm 0x6000 (not fully supported)

Use the rainCollectorType to interpret the hiRainRate field.
For example, if you have a 0.01 in rain collector, a rain rate value of 19 = 0.19 in/hr = 4.8 mm/hr, but if you have a 0.2 mm rain collector, a rain rate value of 19 = 3.8 mm/hr = 0.15 in/hr.

Format for the iconFlags field The lower nibble will hold a value that will represent an Icon to associate with this data record (i.e. snow, rain, sun, lightning, etc.). This field is not used.

Bit (0x10) is set if the user has used the edit record function to change a data value. This allows tracking of edited data.

Bit (0x20) is set if there is a data note associated with the archive record.
If there is, it will be found in a text file named YYYYMMDDmmmm.NOTE.
YYYY is the four digit year, MM is the two digit month (i.e. Jan = 01), DD is the two digit day, and mmmm is the number of minutes past midnight (i.e. the packedTime field).
This file is found in the DATANOTE subdirectory of the station directory.

Format for the moreFlags field The lowest 3 bits contain the transmitter ID that is the source of the wind speed packets recorded in the numWindSamples field.
This value is between 0 and 7. If your ISS is on ID 1, zero will be stored in this field.

WindTxID = (moreFlags & 0x07);

Time values and Wind direction values in Daily Summary records
These values are between 0 and 1440 and therefore will fit in 1 1/2 bytes, and 2 values fit in 3 bytes.
Use this code to extract the i'th time or direction value. See below for the list of i values.

fieldIndex = (i/2) * 3;    // note this is integer division (rounded down)

if (i is even)
    value = field[fieldIndex] + (field[fieldIndex+2] & 0x0F)<<8;

if (i is odd)
    value = field[fieldIndex+1] + (field[fieldIndex+2] & 0xF0)<<4;

A value of 0x0FFF or 0x07FF indicates no data available (i.e. invalid data)

The time value represents the number of minutes after midnight that the specified event took place (actually the time of the archive record).

The wind direction bins represent the number of minutes that that direction was the dominant wind direction for the day.

Index values for Daily Summary Record 1 time values
Time of High Outside Temperature 0
Time of Low Outside Temperature 1
Time of High Inside Temperature 2
Time of Low Inside Temperature 3
Time of High Wind Chill 4
Time of Low Wind Chill 5
Time of High Dew Point 6
Time of Low Dew Point 7
Time of High Outside Humidity 8
Time of Low Outside Humidity 9
Time of High Inside Humidity 10
Time of Low Inside Humidity 11
Time of High Barometer 12
Time of Low Barometer 13
Time of High Wind Speed 14
Time of High Average Wind Speed 15
Time of High Rain Rate 16
Time of High UV 17

Index values for Daily Summary Record 2 time values
Time of High Solar Rad 0
Time of High Outside Heat Index 1
Time of Low Outside Heat Index 2
Time of High Outside THSW Index 3
Time of Low Outside THSW Index 4
Time of High Outside THW Index 5
Time of Low Outside THW Index 6
Time of High Outside Wet Bulb Temp 7
Time of Low Outside Wet Bulb Temp 8
(Time value 9 is not used)

Index values for Dominant Wind direction bins in Daily Summary Record 2
N 0
NNE 1
NE 2
...
NW 14
NNW 15



COPYRIGHT STATEMENT FOLLOWS THIS LINE
    Portions copyright 1994, 1995, 1996, 1997, 1998, by Cold Spring Harbor Laboratory. Funded under Grant P41-RR02188 by the National Institutes of Health.
    Portions copyright 1996, 1997, 1998, by Boutell.Com, Inc.
    GIF decompression code copyright 1990, 1991, 1993, by David Koblas (koblas@netcom.com).
    Non-LZW-based GIF compression code copyright 1998, by Hutchison Avenue Software Corporation (<http://www.hasc.com>, info@hasc.com).
    Permission has been granted to copy and distribute gd in any context, including a commercial application, provided that this notice is present in user-accessible supporting documentation.
    This does not affect your ownership of the derived work itself, and the intent is to assure proper credit for the authors of gd, not to interfere with your productive use of gd. If you have questions, ask. "Derived works" includes all programs that utilize the library. Credit must be given in user-accessible documentation.
    Permission to use, copy, modify, and distribute this software and its documentation for any purpose and without fee is hereby granted, provided that the above copyright notice appear in all copies and that both that copyright notice and this permission notice appear in supporting documentation. This software is provided "as is" without express or implied warranty.
END OF COPYRIGHT STATEMENT
