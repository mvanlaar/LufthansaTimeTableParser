using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PDFReader;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace LufthansaTimeTableParser
{
    public class Program
    {
        [Serializable]
        public class CIFLight
        {
            // Auto-implemented properties. 
           
            public string FromIATA;
            public string ToIATA;
            public DateTime FromDate;
            public DateTime ToDate;
            public Boolean FlightMonday;
            public Boolean FlightTuesday;
            public Boolean FlightWednesday;
            public Boolean FlightThursday;
            public Boolean FlightFriday;
            public Boolean FlightSaterday;
            public Boolean FlightSunday;
            public DateTime DepartTime;
            public DateTime ArrivalTime;
            public String FlightNumber;
            public String FlightAirline;
            public String FlightOperator;
            public String FlightAircraft;
            public Boolean FlightCodeShare;
            public Boolean FlightNextDayArrival;
            public int FlightNextDays;
            public string FlightDuration;
        }

        public class AirportDef
        {
            // Auto-implemented properties. 
            public string Letter { get; set; }
            public string IATA { get; set; }
        }

        public static readonly List<string> _LufthansaAircraftCode = new List<string>() { "A319", "A320", "A321", "A330", "A340", "A380", "AR1", "AR8", "AT72", "B737", "B747", "CRJ7", "CRJ9", "DH8D", "E145", "E190", "E195", "F100", "BUS", "ICE", "B763", "B777", "B74H", "B733" };
        public static readonly List<string> _LufthansaAirlineCode = new List<string>() { "LH", "LX", "2L", "9L", "A3", "AC", "AF", "AI", "AV", "AX", "B6", "BE", "C3", "CA", "CL", "CO", "EN", "ET", "EV", "EW", "F7", "G7", "IQ", "JJ", "JP", "K2", "KM", "LG", "LO", "LY", "MS", "NH", "NI", "NZ", "OL", "OO", "OS", "OU", "OZ", "PS", "PT", "QI", "QR", "S5", "SA", "SK", "SN", "SQ", "TA", "TG", "TK", "TP", "UA", "US", "VO", "WK", "YV", "2A" };
        static List<AirportDef> Airports = new List<AirportDef>
        {
            new AirportDef { Letter = "M", IATA="MXP" },
            new AirportDef { Letter = "L", IATA="LIN" },
            new AirportDef { Letter = "H", IATA="LHR" },
            new AirportDef { Letter = "C", IATA="LCY" },
            new AirportDef { Letter = "C", IATA="LCY" }, 
            new AirportDef { Letter = "T", IATA="TXL" }, 
            new AirportDef { Letter = "B", IATA="BER" }, 
            new AirportDef { Letter = "S", IATA="STR" }, 
            new AirportDef { Letter = "Z", IATA="ZWS" }, 
            new AirportDef { Letter = "J", IATA="JFK" }, 
            new AirportDef { Letter = "W", IATA="EWR" },
            new AirportDef { Letter = "N", IATA="NRT" },
            new AirportDef { Letter = "H", IATA="HND" }            
        };

        static void Main(string[] args)
        {

            var text = new StringBuilder();
            CultureInfo ci = new CultureInfo("en-US");
            string path = AppDomain.CurrentDomain.BaseDirectory + "data\\Lufthansa.pdf";
            Regex rgxtime = new Regex(@"^([0-1]?[0-9]|[2][0-3]):([0-5][0-9])(\+)?([A-Z])?(\+)?");
            Regex rgxFlightNumber = new Regex(@"^([A-Z]{2}|[A-Z]\d|\d[A-Z])[0-9](\d{1,4})?(\([A-Z]{2}\))?$");
            Regex rgxFlightNumberPri = new Regex(@"^([A-Z]{2}|[A-Z]\d|\d[A-Z])[0-9](\d{1,4})?");
            Regex rgxFlightNumberCodeShare = new Regex(@"\([A-Z]{2}\)$");
            Regex rgxIATAAirport = new Regex(@"^[A-Z]{3}$");
            Regex rgxdate1 = new Regex(@"(([0-9])|([0-2][0-9])|([3][0-1])) (Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)");
            Regex rgxFlightDay = new Regex(@"^\d+$");
            Regex rgxFlightDayExclusion = new Regex(@"^X[1234567]+");            
            Regex rgxFlightTime = new Regex(@"^([0-9]|0[0-9]|1[0-9]|2[0-3])H([0-9]|0[0-9]|1[0-9]|2[0-9]|3[0-9]|4[0-9]|5[0-9])M$");
            Regex rgxTimeZone = new Regex(@"^(?:Z|[+-](?:2[0-3]|[01][0-9]):[0-5][0-9])$");
            List<CIFLight> CIFLights = new List<CIFLight> { };
            List<Rectangle> rectangles = new List<Rectangle>();

            //rectangles.Add(new Rectangle(x+(j*offset), (y+i*offset), offset, offset));
            float distanceInPixelsFromLeft = 0;
            float distanceInPixelsFromBottom = 30;
            float width = 140;//pdfReader.GetPageSize(page).Width / 2; // 306 deelt niet naar helft? 
            float height = 560; // pdfReader.GetPageSize(page).Height;
            // Formaat papaier 
            // Letter		 612x792
            // A4		     595x842
            var left = new Rectangle(
                        distanceInPixelsFromLeft,
                        distanceInPixelsFromBottom,
                        width,
                        height);

            var center = new Rectangle(
                       150,
                       distanceInPixelsFromBottom,
                       275,
                       height);

            var right = new Rectangle(
                       280,
                       distanceInPixelsFromBottom,
                       612,
                       height);
            

            rectangles.Add(left);
            rectangles.Add(center);
            rectangles.Add(right);

            // The PdfReader object implements IDisposable.Dispose, so you can
            // wrap it in the using keyword to automatically dispose of it
            Console.WriteLine("Opening PDF File...");

            //PdfReader reader = new PdfReader(path);


            using (var pdfReader = new PdfReader(path))
            {
                // Vaststellen valid from to date
                DateTime ValidFrom = new DateTime(2015, 6, 8);
                DateTime ValidTo = new DateTime(2015, 10, 24);

                // Vaststellen van Basics

                string TEMP_FromIATA = null;
                string TEMP_ToIATA = null;
                DateTime TEMP_ValidFrom = new DateTime();
                DateTime TEMP_ValidTo = new DateTime();
                int TEMP_Conversie = 0;
                Boolean TEMP_FlightMonday = false;
                Boolean TEMP_FlightTuesday = false;
                Boolean TEMP_FlightWednesday = false;
                Boolean TEMP_FlightThursday = false;
                Boolean TEMP_FlightFriday = false;
                Boolean TEMP_FlightSaterday = false;
                Boolean TEMP_FlightSunday = false;
                DateTime TEMP_DepartTime = new DateTime();
                DateTime TEMP_ArrivalTime = new DateTime();
                Boolean TEMP_FlightCodeShare = false;
                string TEMP_FlightNumber = null;
                string TEMP_Aircraftcode = null;
                TimeSpan TEMP_DurationTime = TimeSpan.MinValue;
                Boolean TEMP_FlightNextDayArrival = false;
                int TEMP_FlightNextDays = 0;
                string TEMP_FlightOperator = null;
                string TEMP_FromUTC = null;
                string TEMP_ToUTC = null;                
                // Loop through each page of the document
                for (var page = 6; page <= 6; page++)
                //for (var page = 3; page <= pdfReader.NumberOfPages; page++)
                {

                    Console.WriteLine("Parsing page {0}...", page);
                    float pageHeight = pdfReader.GetPageSize(page).Height;
                    float pageWidth = pdfReader.GetPageSize(page).Width;

                    //System.Diagnostics.Debug.WriteLine(currentText);


                    foreach (Rectangle rect in rectangles)
                    {
                        ITextExtractionStrategy its = new CSVTextExtractionStrategy();
                        var filters = new RenderFilter[1];
                        filters[0] = new RegionTextRenderFilter(rect);
                        //filters[1] = new RegionTextRenderFilter(rechts);

                        ITextExtractionStrategy strategy =
                            new FilteredTextRenderListener(
                                new CSVTextExtractionStrategy(), // new LocationTextExtractionStrategy()
                                filters);

                        var currentText = PdfTextExtractor.GetTextFromPage(
                            pdfReader,
                            page,
                            strategy);

                        currentText =
                            Encoding.UTF8.GetString(Encoding.Convert(
                                Encoding.Default,
                                Encoding.UTF8,
                                Encoding.Default.GetBytes(currentText)));

                        string[] lines = Regex.Split(currentText, "\r\n");
                        
                        foreach (string line in lines)
                        {
                            string[] values = line.SplitWithQualifier(',', '\"', true);

                            foreach (string value in values)
                            {
                                if (!String.IsNullOrEmpty(value.Trim()))
                                {
                                    // getrimde string temp value
                                    string temp_string = value.Trim();

                                    // assuming C#
                                    //if (temp_string == "OS376")
                                    //{
                                    //    System.Diagnostics.Debugger.Break();
                                    //}

                                    // New To:
                                    if (line.Replace("\"", "") == temp_string && rgxTimeZone.IsMatch(temp_string))
                                    {
                                        TEMP_FromIATA = null;
                                        TEMP_ToIATA = null;
                                        TEMP_ToUTC = null;
                                        TEMP_FromUTC = null;
                                    }

                                    // Time Zone support for calulation of flight duration
                                    //from Timezone
                                    if (rgxTimeZone.IsMatch(temp_string) && TEMP_FromIATA == null) {
                                        // timezone from airport
                                        TEMP_FromUTC = rgxTimeZone.Match(temp_string).Groups[0].Value;                                        
                                    }
                                    if (rgxTimeZone.IsMatch(temp_string) && TEMP_FromIATA != null & TEMP_ToIATA != null) {
                                        // timezone to airport
                                        TEMP_ToUTC = rgxTimeZone.Match(temp_string).Groups[0].Value;
                                    }                                    
                                    // From en To
                                    if (rgxIATAAirport.Matches(temp_string).Count > 0)
                                    {
                                        if (String.IsNullOrEmpty(TEMP_FromIATA))
                                        {
                                            TEMP_FromIATA = rgxIATAAirport.Match(temp_string).Groups[0].Value;                                            
                                        }
                                        else
                                        {
                                            if (String.IsNullOrEmpty(TEMP_ToIATA) && !String.IsNullOrEmpty(TEMP_FromIATA))
                                            {
                                                TEMP_ToIATA = rgxIATAAirport.Match(temp_string).Groups[0].Value;
                                            }
                                        }
                                    }
                                    if (temp_string == "®")
                                    {
                                        // New To airport.
                                        TEMP_ToIATA = null;
                                    }
                                    // Parsing flightdays
                                    if (temp_string == "X" || rgxFlightDay.Matches(temp_string).Count > 0 || rgxFlightDayExclusion.Matches(temp_string).Count > 0)
                                    {
                                        // Flight days found!
                                        if (temp_string == "X")
                                            // All flight days
                                        {
                                            TEMP_FlightSunday = true; 
                                            TEMP_FlightMonday = true; 
                                            TEMP_FlightTuesday = true; 
                                            TEMP_FlightWednesday = true; 
                                            TEMP_FlightThursday = true; 
                                            TEMP_FlightFriday = true; 
                                            TEMP_FlightSaterday = true;
                                        }
                                        if (rgxFlightDayExclusion.Matches(temp_string).Count > 0)
                                        {
                                            string y = null;
                                            foreach (Match ItemMatch in rgxFlightDayExclusion.Matches(temp_string))
                                            {
                                                y = y + ItemMatch.Value;
                                            }
                                            y = y.Replace("X", "");
                                            char[] arr;
                                            arr = y.ToCharArray();

                                            foreach (char c in arr)
                                            {
                                                int.TryParse(c.ToString(), out TEMP_Conversie);
                                                if (TEMP_Conversie == 1) { TEMP_FlightMonday = false; }
                                                if (TEMP_Conversie == 2) { TEMP_FlightTuesday = false; }
                                                if (TEMP_Conversie == 3) { TEMP_FlightWednesday = false; }
                                                if (TEMP_Conversie == 4) { TEMP_FlightThursday = false; }
                                                if (TEMP_Conversie == 5) { TEMP_FlightFriday = false; }
                                                if (TEMP_Conversie == 6) { TEMP_FlightSaterday = false; }
                                                if (TEMP_Conversie == 7) { TEMP_FlightSunday = false; }

                                            }
                                        }                                       
                                        if (rgxFlightDay.Matches(temp_string).Count > 0)
                                        {
                                            string y = null;
                                            foreach (Match ItemMatch in rgxFlightDay.Matches(temp_string))
                                            {
                                                y = y + ItemMatch.Value;
                                            }
                                            y = y.Replace(" ", "");                                        
                                            char[] arr;
                                            arr = y.ToCharArray();

                                            foreach (char c in arr)
                                            {
                                                int.TryParse(c.ToString(), out TEMP_Conversie);
                                                if (TEMP_Conversie == 1) { TEMP_FlightSunday = true; }
                                                if (TEMP_Conversie == 2) { TEMP_FlightMonday = true; }
                                                if (TEMP_Conversie == 3) { TEMP_FlightTuesday = true; }
                                                if (TEMP_Conversie == 4) { TEMP_FlightWednesday = true; }
                                                if (TEMP_Conversie == 5) { TEMP_FlightThursday = true; }
                                                if (TEMP_Conversie == 6) { TEMP_FlightFriday = true; }
                                                if (TEMP_Conversie == 7) { TEMP_FlightSaterday = true; }

                                            }
                                        }

                                    }
                                    // Vertrek en aankomst tijden
                                    if (rgxtime.Matches(temp_string).Count > 0)
                                    {

                                        if (TEMP_DepartTime == DateTime.MinValue)
                                        {
                                            // tijd parsing                                                
                                            DateTime.TryParse(temp_string.Trim(), out TEMP_DepartTime);
                                            //DateTime.TryParseExact(temp_string, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out TEMP_DepartTime);
                                        }
                                        else
                                        {
                                            // Er is al een waarde voor from dus dit is de to.
                                            string x = temp_string;
                                            if (x.Contains("+"))
                                            {
                                                // Next day arrival
                                                x = x.Replace("+", "");
                                                TEMP_FlightNextDays = 1;
                                                TEMP_FlightNextDayArrival = true;
                                            }
                                            // Multiple airport places.
                                            if (Regex.Matches(x,"[A-Z]").Count > 0) {
                                                //Lette found replace to with different airport
                                                string z = null;
                                                z = Regex.Match(x,"[A-Z]").Groups[0].Value;
                                                var item = Airports.Find(q => q.Letter == z);
                                                TEMP_ToIATA = item.IATA;
                                                x = x.Replace(z, "");
                                            }                                            
                                            DateTime.TryParse(x.Trim(), out TEMP_ArrivalTime);
                                        }
                                    }
                                    // FlightNumber Parsing
                                    if (rgxFlightNumber.IsMatch(temp_string) && !_LufthansaAircraftCode.Contains(temp_string, StringComparer.OrdinalIgnoreCase))
                                    {
                                        // Only Main FlightNumber
                                        TEMP_FlightNumber = rgxFlightNumberPri.Match(temp_string).Groups[0].Value;
                                        
                                        // Code Share flight
                                        if (rgxFlightNumberCodeShare.IsMatch(temp_string))
                                        {
                                            // Code Share Flight
                                            string x = null;
                                            TEMP_FlightCodeShare = true;
                                            x = rgxFlightNumberCodeShare.Match(temp_string).Groups[0].Value;
                                            x = x.Replace("(", "");
                                            x = x.Replace(")", "");
                                            TEMP_FlightOperator = x;                                          
                                        }                                        
                                    }
                                    // Vliegtuig parsing                                    
                                    if (_LufthansaAircraftCode.Contains(temp_string, StringComparer.OrdinalIgnoreCase))
                                    {
                                        if (TEMP_Aircraftcode == null)
                                        {
                                            TEMP_Aircraftcode = temp_string;
                                        }
                                    }
                                    
                                    if (TEMP_DepartTime != DateTime.MinValue && TEMP_ArrivalTime != DateTime.MinValue && TEMP_ToUTC != null & TEMP_FromUTC != null)
                                    {
                                        // Calculate flight times
                                        TimeSpan fromoffsettimespane = TimeSpan.Parse(TEMP_FromUTC.Replace("+", ""));
                                        TimeSpan tooffsettimespane = TimeSpan.Parse(TEMP_ToUTC.Replace("+", ""));                                        
                                        DateTimeOffset departtimeOffset = new DateTimeOffset(TEMP_DepartTime, fromoffsettimespane);
                                        DateTimeOffset arrivaltimeOffset = new DateTimeOffset(TEMP_ArrivalTime, tooffsettimespane);
                                        TEMP_DurationTime = arrivaltimeOffset - departtimeOffset;
                                    }
                                    
                                    if (TEMP_Aircraftcode != null && TEMP_DurationTime != TimeSpan.MinValue)
                                    {
                                        // Aircraft code is gevonden, dit moet nu de vlucht tijd zijn. En dus de laatste waarde in de reeks.                                        
                                        string TEMP_Airline = null;
                                        TEMP_Airline = TEMP_FlightNumber.Substring(0, 2);

                                        CIFLights.Add(new CIFLight
                                        {
                                            FromIATA = TEMP_FromIATA,
                                            ToIATA = TEMP_ToIATA,
                                            FromDate = ValidFrom,
                                            ToDate = ValidTo,
                                            ArrivalTime = TEMP_ArrivalTime,
                                            DepartTime = TEMP_DepartTime,
                                            FlightAircraft = TEMP_Aircraftcode,
                                            FlightAirline = TEMP_Airline,
                                            FlightMonday = TEMP_FlightMonday,
                                            FlightTuesday = TEMP_FlightTuesday,
                                            FlightWednesday = TEMP_FlightWednesday,
                                            FlightThursday = TEMP_FlightThursday,
                                            FlightFriday = TEMP_FlightFriday,
                                            FlightSaterday = TEMP_FlightSaterday,
                                            FlightSunday = TEMP_FlightSunday,
                                            FlightNumber = TEMP_FlightNumber,
                                            FlightOperator = TEMP_FlightOperator,
                                            FlightDuration = TEMP_DurationTime.ToString().Replace("-",""),
                                            FlightCodeShare = TEMP_FlightCodeShare,
                                            FlightNextDayArrival = TEMP_FlightNextDayArrival,
                                            FlightNextDays = TEMP_FlightNextDays
                                        });
                                        // Cleaning All but From and To 
                                        TEMP_ValidFrom = new DateTime();
                                        TEMP_ValidTo = new DateTime();
                                        TEMP_Conversie = 0;
                                        TEMP_FlightMonday = false;
                                        TEMP_FlightTuesday = false;
                                        TEMP_FlightWednesday = false;
                                        TEMP_FlightThursday = false;
                                        TEMP_FlightFriday = false;
                                        TEMP_FlightSaterday = false;
                                        TEMP_FlightSunday = false;
                                        TEMP_DepartTime = new DateTime();
                                        TEMP_ArrivalTime = new DateTime();
                                        TEMP_FlightNumber = null;
                                        TEMP_Aircraftcode = null;
                                        TEMP_DurationTime = TimeSpan.MinValue;
                                        TEMP_FlightCodeShare = false;
                                        TEMP_FlightNextDayArrival = false;
                                        TEMP_FlightNextDays = 0;
                                        TEMP_FlightOperator = null;
                                    }                                    
                                    Console.WriteLine(value);
                                }
                            }
                        }

                    }


                    //text.Append(currentText);                    
                }
            }

            // You'll do something else with it, here I write it to a console window
            // Console.WriteLine(text.ToString());

            // Write the list of objects to a file.
            System.Xml.Serialization.XmlSerializer writer =
            new System.Xml.Serialization.XmlSerializer(CIFLights.GetType());
            string myDir = AppDomain.CurrentDomain.BaseDirectory + "\\output";
            System.IO.Directory.CreateDirectory(myDir);
            System.IO.StreamWriter file =
               new System.IO.StreamWriter("output\\output.xml");

            writer.Serialize(file, CIFLights);
            file.Close();

            //Console.ReadKey();



        }




    }

}

