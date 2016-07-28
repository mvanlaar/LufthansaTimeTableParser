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
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Net;
using Newtonsoft;
using CsvHelper;
using Newtonsoft.Json.Linq;
using System.IO.Compression;

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

        public class IATAAirport
        {
            public string stop_iata;
            public string stop_icao;
            public string stop_name;
            public string stop_city;
            public string stop_country;
            public string stop_lat;
            public string stop_lon;
            public string stop_timezone;
        }

        public class AirlinesDef
        {
            // Auto-implemented properties.  
            public string Name { get; set; }
            public string IATA { get; set; }
            public string DisplayName { get; set; }
            public string WebsiteUrl { get; set; }
        }
        static List<AirlinesDef> _Airlines = new List<AirlinesDef> 
        {
            new AirlinesDef { IATA = "DA", Name="AEROLINEA DE ANTIOQUIA S.A.", DisplayName="ADA",WebsiteUrl="https://www.ada-aero.com/" }, 
            new AirlinesDef { IATA = "EF", Name="EASYFLY S.A", DisplayName="Easyfly",WebsiteUrl="http://www.easyfly.com.co" }, 
            new AirlinesDef { IATA = "2K", Name="AEROGAL", DisplayName="Avianca Ecuador",WebsiteUrl="http://www.avianca.com" }, 
            new AirlinesDef { IATA = "9H", Name="DUTCH ANTILLES EXPRESS SUCURSAL COLOMBIA", DisplayName="Dutch Antilles Express",WebsiteUrl="https://nl.wikipedia.org/wiki/Dutch_Antilles_Express" }, 
            new AirlinesDef { IATA = "AR", Name="AEROLINEAS ARGENTINAS", DisplayName="Aerolíneas Argentinas",WebsiteUrl="http://www.aerolineas.com.ar/" }, 
            new AirlinesDef { IATA = "AM", Name="AEROMEXICO SUCURSAL COLOMBIA", DisplayName="Aeroméxico",WebsiteUrl="http://www.aeromexico.com/" }, 
            new AirlinesDef { IATA = "P5", Name="AEROREPUBLICA", DisplayName="Copa Airlines",WebsiteUrl="http://www.copa.com" }, 
            new AirlinesDef { IATA = "AC", Name="AIR CANADA", DisplayName="AirCanada",WebsiteUrl="http://www.aircanada.com" }, 
            new AirlinesDef { IATA = "AF", Name="AIR FRANCE", DisplayName="Air France",WebsiteUrl="http://www.airfrance.com" }, 
            new AirlinesDef { IATA = "4C", Name="AIRES", DisplayName="LATAM Colombia",WebsiteUrl="http://www.latam.com/" }, 
            new AirlinesDef { IATA = "AA", Name="AMERICAN", DisplayName="American Airlines",WebsiteUrl="http://www.aa.com" }, 
            new AirlinesDef { IATA = "AV", Name="AVIANCA", DisplayName="Avianca",WebsiteUrl="http://www.avianca.com" }, 
            new AirlinesDef { IATA = "V0", Name="CONVIASA", DisplayName="Conviasa",WebsiteUrl="http://www.conviasa.aero/" }, 
            new AirlinesDef { IATA = "CM", Name="COPA", DisplayName="Copa Airlines",WebsiteUrl="http://www.copaair.com/" }, 
            new AirlinesDef { IATA = "CU", Name="CUBANA", DisplayName="Cubana de Aviación",WebsiteUrl="http://www.cubana.cu/home/?lang=en" }, 
            new AirlinesDef { IATA = "DL", Name="DELTA", DisplayName="Delta",WebsiteUrl="http://www.delta.com" }, 
            new AirlinesDef { IATA = "4O", Name="INTERJET", DisplayName="Interjet",WebsiteUrl="http://www.interjet.com/" }, 
            new AirlinesDef { IATA = "5Z", Name="FAST COLOMBIA SAS", DisplayName="ViVaColombia",WebsiteUrl="http://www.vivacolombia.co/" }, 
            new AirlinesDef { IATA = "IB", Name="IBERIA", DisplayName="Iberia",WebsiteUrl="http://www.iberia.com" }, 
            new AirlinesDef { IATA = "B6", Name="JETBLUE AIRWAYS CORPORATION", DisplayName="Jetblue",WebsiteUrl="http://www.jetblue.com" }, 
            new AirlinesDef { IATA = "LR", Name="LACSA", DisplayName="Avianca Costa Rica",WebsiteUrl="http://www.avianca.com" }, 
            new AirlinesDef { IATA = "LA", Name="LAN AIRLINES S.A.", DisplayName="LAN Airlines",WebsiteUrl="http://www.lan.com/" }, 
            new AirlinesDef { IATA = "LP", Name="LAN PERU", DisplayName="LAN Airlines",WebsiteUrl="http://www.lan.com/" }, 
            new AirlinesDef { IATA = "LH", Name="LUFTHANSA", DisplayName="Lufthansa",WebsiteUrl="http://www.lufthansa.com" }, 
            new AirlinesDef { IATA = "9R", Name="SERVICIO AEREO A TERRITORIOS NACIONALES SATENA", DisplayName="Satena",WebsiteUrl="http://www.satena.com/" }, 
            new AirlinesDef { IATA = "NK", Name="SPIRIT AIRLINES", DisplayName="Spirit",WebsiteUrl="http://www.spirit.com" }, 
            new AirlinesDef { IATA = "TA", Name="TACA INTERNATIONAL", DisplayName="TACA Airlines",WebsiteUrl="http://www.taca.com/" }, 
            new AirlinesDef { IATA = "EQ", Name="TAME", DisplayName="TAME",WebsiteUrl="http://www.tame.com.ec/" }, 
            new AirlinesDef { IATA = "3P", Name="TIARA", DisplayName="Tiara Air Aruba",WebsiteUrl="http://www.tiara-air.com/" }, 
            new AirlinesDef { IATA = "T0", Name="TRANS AMERICAN AIR LINES S.A. SUCURSAL COL.", DisplayName="Trans American Airlines",WebsiteUrl="http://www.avianca.com/" }, 
            new AirlinesDef { IATA = "UA", Name="UNITED AIR LINES INC", DisplayName="United",WebsiteUrl="http://www.united.com" }, 
            new AirlinesDef { IATA = "4C", Name="LATAM AIRLINES GROUP S.A SUCURSAL COLOMBIA", DisplayName="LATAM",WebsiteUrl="http://www.latam.com/" }, 
            new AirlinesDef { IATA = "TP", Name="TAP PORTUGAL SUCURSAL COLOMBIA", DisplayName="TAP",WebsiteUrl="http://www.flytap.com" }, 
            new AirlinesDef { IATA = "7P", Name="AIR PANAMA", DisplayName="Air Panama",WebsiteUrl="http://www.airpanama.com/" }, 
            new AirlinesDef { IATA = "O6", Name="OCEANAIR", DisplayName="Avianca Brazil",WebsiteUrl="http://www.avianca.com" },
            new AirlinesDef { IATA = "8I", Name="INSELAIR ARUBA", DisplayName="Insel Air Aruba",WebsiteUrl="http://www.fly-inselair.com/"},
            new AirlinesDef { IATA = "7I", Name="INSEL AIR", DisplayName="Insel Air",WebsiteUrl="http://www.fly-inselair.com/"},
            new AirlinesDef { IATA = "TK", Name="TURK HAVA YOLLARI (TURKISH AIRKINES CO.)", DisplayName="Turkish Airlines",WebsiteUrl="http://www.turkishairlines.com"},
            new AirlinesDef { IATA = "UX", Name="AIR EUROPA", DisplayName="Air Europe",WebsiteUrl="http://www.aireurope.com"},
            new AirlinesDef { IATA = "9V", Name="AVIOR AIRLINES,C.A.", DisplayName="Avior Airlines",WebsiteUrl="http://www.avior.com.ve/"},
            new AirlinesDef { IATA = "KL", Name="KLM", DisplayName="KLM",WebsiteUrl="http://www.klm.nl"},
            new AirlinesDef { IATA = "JJ", Name="TAM", DisplayName="TAM Linhas Aéreas",WebsiteUrl="http://www.latam.com/"},
            new AirlinesDef { IATA = "OS", Name="TAM", DisplayName="Austrian Airlines",WebsiteUrl="http://www.latam.com/"},
            new AirlinesDef { IATA = "LX", Name="TAM", DisplayName="Swiss International Air Lines",WebsiteUrl="http://www.swiss.com/"},
            new AirlinesDef { IATA = "4U", Name="TAM", DisplayName="Germanwings",WebsiteUrl="http://www.latam.com/"},
            new AirlinesDef { IATA = "EW", Name="TAM", DisplayName="Eurowings",WebsiteUrl="http://www.latam.com/"},
            new AirlinesDef { IATA = "X6", Name="TAM", DisplayName="Khors Aircompany Ltd",WebsiteUrl="http://www.latam.com/"},
            new AirlinesDef { IATA = "X2", Name="TAM", DisplayName="Baikal Airlines",WebsiteUrl="http://www.latam.com/"},
            new AirlinesDef { IATA = "X1", Name="TAM", DisplayName="TAM Linhas Aéreas",WebsiteUrl="http://www.latam.com/"}

            
        };

        public class AirportDef
        {
            // Auto-implemented properties. 
            public string Letter { get; set; }
            public string IATA { get; set; }
        }

        public static readonly List<string> _LufthansaAircraftCode = new List<string>() { "A319", "A320", "A321", "A330", "A340", "A380", "AR1", "AR8", "AT72", "B737", "B747", "CRJ7", "CRJ9", "DH8D", "E145", "E190", "E195", "F100", "BUS", "ICE", "B763", "B777", "B74H", "B733", "TRN" };
        public static readonly List<string> _LufthansaAirlineCode = new List<string>() { "LH", "LX", "2L", "9L", "A3", "AC", "AF", "AI", "AV", "AX", "B6", "BE", "C3", "CA", "CL", "CO", "EN", "ET", "EV", "EW", "F7", "G7", "IQ", "JJ", "JP", "K2", "KM", "LG", "LO", "LY", "MS", "NH", "NI", "NZ", "OL", "OO", "OS", "OU", "OZ", "PS", "PT", "QI", "QR", "S5", "SA", "SK", "SN", "SQ", "TA", "TG", "TK", "TP", "UA", "US", "VO", "WK", "YV", "2A" };
        static List<AirportDef> Airports = new List<AirportDef>
        {
            new AirportDef { Letter = "M", IATA="MXP" },
            new AirportDef { Letter = "L", IATA="LIN" },
            new AirportDef { Letter = "H", IATA="LHR" },
            new AirportDef { Letter = "Y", IATA="LCY" },
            new AirportDef { Letter = "T", IATA="TXL" }, 
            new AirportDef { Letter = "B", IATA="BER" }, 
            new AirportDef { Letter = "S", IATA="STR" }, 
            new AirportDef { Letter = "Z", IATA="ZWS" }, 
            new AirportDef { Letter = "J", IATA="JFK" }, 
            new AirportDef { Letter = "W", IATA="EWR" },
            new AirportDef { Letter = "N", IATA="NRT" },
            new AirportDef { Letter = "H", IATA="HND" },
            new AirportDef { Letter = "D", IATA="DMM" },
            new AirportDef { Letter = "B", IATA="DMS" }
        };

        static void Main(string[] args)
        {

            // Downlaoding latest pdf from skyteam website
            string path = AppDomain.CurrentDomain.BaseDirectory + "data\\lufthansa.pdf";
            string myDirdata = AppDomain.CurrentDomain.BaseDirectory + "\\data";
            Directory.CreateDirectory(myDirdata);

            Uri url = new Uri("http://dl-oim.de/download/LH_Timetable_en.pdf");
            const string ua = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)";
            const string referer = "http://www.lufthansa.com/nl/en/Timetable-to-download";
            if (!File.Exists(path))
            {
                WebRequest.DefaultWebProxy = null;
                using (System.Net.WebClient wc = new WebClient())
                {
                    wc.Headers.Add("user-agent", ua);
                    wc.Headers.Add("Referer", referer);
                    wc.Proxy = null;
                    Console.WriteLine("Downloading latest lufthansa timetable pdf file...");
                    wc.DownloadFile(url, path);
                    Console.WriteLine("Download ready...");
                }
            }

            var text = new StringBuilder();
            CultureInfo ci = new CultureInfo("en-US");            
            Regex rgxtime = new Regex(@"^([0-1]?[0-9]|[2][0-3]):([0-5][0-9])(\+)?([A-Z0-9])?(\+)?");
            Regex rgxFlightNumber = new Regex(@"^([A-Z]{2}|[A-Z]\d|\d[A-Z])[0-9](\d{1,4})?(\([A-Z0-9]{2}\))?$");
            Regex rgxFlightNumberPri = new Regex(@"^([A-Z]{2}|[A-Z]\d|\d[A-Z])[0-9](\d{1,4})?");
            Regex rgxFlightNumberCodeShare = new Regex(@"\([A-Z0-9]{2}\)$");
            Regex rgxIATAAirport = new Regex(@"^[A-Z]{3}$");
            Regex rgxdate1 = new Regex(@"(([0-9])|([0-2][0-9])|([3][0-1])) (Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)");
            Regex rgxdate2 = new Regex(@"(3[0-1]|2[0-9]|1[0-9]|0[1-9]) (Jan|JAN|Feb|FEB|Mar|MAR|Apr|APR|May|MAY|Jun|JUN|Jul|JUL|Aug|AUG|Sep|SEP|Oct|OCT|Nov|NOV|Dec|DEC) (\d{2})");
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

            var firstpage = new Rectangle(
                        distanceInPixelsFromLeft,
                        distanceInPixelsFromBottom,
                        595,
                        height);


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
                ITextExtractionStrategy fpstrategy = new SimpleTextExtractionStrategy();

                var fpcurrentText = PdfTextExtractor.GetTextFromPage(
                    pdfReader,
                    1,
                    fpstrategy);

                fpcurrentText =
                    Encoding.UTF8.GetString(Encoding.Convert(
                        Encoding.Default,
                        Encoding.UTF8,
                        Encoding.Default.GetBytes(fpcurrentText)));

                MatchCollection matches = rgxdate2.Matches(fpcurrentText);

                string validfrom = matches[0].Value;
                string validto = matches[1].Value;

                string TEMP_PageFromIATA = null;
                string TEMP_PageToIATA = null;

                DateTime ValidFrom = DateTime.ParseExact(validfrom, "dd MMM yy", ci);
                DateTime ValidTo = DateTime.ParseExact(validto, "dd MMM yy", ci);
                
                // Vaststellen valid from to date
                //DateTime ValidFrom = new DateTime(2015, 6, 8);
                //DateTime ValidTo = new DateTime(2015, 10, 24);

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
                for (var page = 6; page <= 50; page++)
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
                                    //if (temp_string == "Turin")
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
                                            // Hack for TRN Aircraft and city
                                            TEMP_Aircraftcode = null;
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

                                        if (!String.IsNullOrEmpty(TEMP_FromIATA) && !String.IsNullOrEmpty(TEMP_ToIATA) && TEMP_Aircraftcode != "BUS" && TEMP_Aircraftcode != "ICE")
                                        {
                                            if (TEMP_Airline == "LH") {
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
                                                    FlightDuration = TEMP_DurationTime.ToString().Replace("-", ""),
                                                    FlightCodeShare = TEMP_FlightCodeShare,
                                                    FlightNextDayArrival = TEMP_FlightNextDayArrival,
                                                    FlightNextDays = TEMP_FlightNextDays
                                                });
                                            }
                                        }
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
            Console.WriteLine("Create XML File...");
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

            // Generate GTFS

            // Read The Airports Json
            Console.WriteLine("Reading IATA Airports....");
            string IATAAirportsFile = AppDomain.CurrentDomain.BaseDirectory + "IATAAirports.json";
            JArray o1 = JArray.Parse(File.ReadAllText(IATAAirportsFile));
            IList<IATAAirport> TempIATAAirports = o1.ToObject<IList<IATAAirport>>();
            var IATAAirports = TempIATAAirports as List<IATAAirport>;

            Console.WriteLine("Creating GTFS Directory...");
            string gtfsDir = AppDomain.CurrentDomain.BaseDirectory + "\\gtfs";
            System.IO.Directory.CreateDirectory(gtfsDir);
            Console.WriteLine("Creating GTFS Files...");


            string gtfsDirfull = AppDomain.CurrentDomain.BaseDirectory + "\\gtfs";
            System.IO.Directory.CreateDirectory(gtfsDirfull);
            Console.WriteLine("Creating GTFS File agency.txt...");
            using (var gtfsagency = new StreamWriter(@"gtfs\\agency.txt"))
            {
                var csv = new CsvWriter(gtfsagency);
                csv.Configuration.Delimiter = ",";
                csv.Configuration.Encoding = Encoding.UTF8;
                csv.Configuration.TrimFields = true;
                // header 
                csv.WriteField("agency_id");
                csv.WriteField("agency_name");
                csv.WriteField("agency_url");
                csv.WriteField("agency_timezone");
                csv.WriteField("agency_lang");
                csv.WriteField("agency_phone");
                csv.WriteField("agency_fare_url");
                csv.WriteField("agency_email");
                csv.NextRecord();

                var airlines = CIFLights.Select(m => new { m.FlightAirline }).Distinct().ToList();

                for (int i = 0; i < airlines.Count; i++) // Loop through List with for)
                {
                    csv.WriteField(airlines[i].FlightAirline);
                    var item4 = _Airlines.Find(q => q.IATA == airlines[i].FlightAirline);
                    string TEMP_Name = item4.DisplayName;
                    string TEMP_Url = item4.WebsiteUrl;
                    csv.WriteField(TEMP_Name);
                    csv.WriteField(TEMP_Url);
                    csv.WriteField("America/Bogota");
                    csv.WriteField("ES");
                    csv.WriteField("");
                    csv.WriteField("");
                    csv.WriteField("");
                    csv.NextRecord();
                }
            }

            Console.WriteLine("Creating GTFS File routes.txt ...");

            using (var gtfsroutes = new StreamWriter(@"gtfs\\routes.txt"))
            {
                // Route record
                var csvroutes = new CsvWriter(gtfsroutes);
                csvroutes.Configuration.Delimiter = ",";
                csvroutes.Configuration.Encoding = Encoding.UTF8;
                csvroutes.Configuration.TrimFields = true;
                // header 
                csvroutes.WriteField("route_id");
                csvroutes.WriteField("agency_id");
                csvroutes.WriteField("route_short_name");
                csvroutes.WriteField("route_long_name");
                csvroutes.WriteField("route_desc");
                csvroutes.WriteField("route_type");
                csvroutes.WriteField("route_url");
                csvroutes.WriteField("route_color");
                csvroutes.WriteField("route_text_color");
                csvroutes.NextRecord();

                var routes = CIFLights.Select(m => new { m.FromIATA, m.ToIATA, m.FlightAirline }).Distinct().ToList();

                for (int i = 0; i < routes.Count; i++) // Loop through List with for)
                {
                    //var item4 = _Airlines.Find(q => q.Name == routes[i].FlightAirline);
                    //string TEMP_Name = item4.DisplayName;
                    //string TEMP_Url = item4.WebsiteUrl;
                    //string TEMP_IATA = item4.IATA;

                    var FromAirportInfo = IATAAirports.Find(q => q.stop_iata == routes[i].FromIATA);
                    var ToAirportInfo = IATAAirports.Find(q => q.stop_iata == routes[i].ToIATA);


                    csvroutes.WriteField(routes[i].FromIATA + routes[i].ToIATA + routes[i].FlightAirline);
                    csvroutes.WriteField(routes[i].FlightAirline);
                    csvroutes.WriteField(routes[i].FromIATA + routes[i].ToIATA + routes[i].FlightAirline);
                    csvroutes.WriteField(FromAirportInfo.stop_city + " - " + ToAirportInfo.stop_city);
                    csvroutes.WriteField(""); // routes[i].FlightAircraft + ";" + CIFLights[i].FlightAirline + ";" + CIFLights[i].FlightOperator + ";" + CIFLights[i].FlightCodeShare
                    csvroutes.WriteField(1102);
                    csvroutes.WriteField("");
                    csvroutes.WriteField("");
                    csvroutes.WriteField("");
                    csvroutes.NextRecord();
                }
            }

            // stops.txt

            List<string> agencyairportsiata =
             CIFLights.SelectMany(m => new string[] { m.FromIATA, m.ToIATA })
                     .Distinct()
                     .ToList();

            using (var gtfsstops = new StreamWriter(@"gtfs\\stops.txt"))
            {
                // Route record
                var csvstops = new CsvWriter(gtfsstops);
                csvstops.Configuration.Delimiter = ",";
                csvstops.Configuration.Encoding = Encoding.UTF8;
                csvstops.Configuration.TrimFields = true;
                // header                                 
                csvstops.WriteField("stop_id");
                csvstops.WriteField("stop_name");
                csvstops.WriteField("stop_desc");
                csvstops.WriteField("stop_lat");
                csvstops.WriteField("stop_lon");
                csvstops.WriteField("zone_id");
                csvstops.WriteField("stop_url");
                csvstops.NextRecord();

                for (int i = 0; i < agencyairportsiata.Count; i++) // Loop through List with for)
                {


                    //int result1 = IATAAirports.FindIndex(T => T.stop_id == 9458)
                    var airportinfo = IATAAirports.Find(q => q.stop_iata == agencyairportsiata[i]);
                    csvstops.WriteField(airportinfo.stop_iata);
                    csvstops.WriteField(airportinfo.stop_name);
                    csvstops.WriteField(airportinfo.stop_city + " - " + airportinfo.stop_country);
                    csvstops.WriteField(airportinfo.stop_lat);
                    csvstops.WriteField(airportinfo.stop_lon);
                    csvstops.WriteField("");
                    csvstops.WriteField("");
                    csvstops.NextRecord();
                }
            }

            Console.WriteLine("Creating GTFS File trips.txt and stop_times.txt...");
            using (var gtfscalendar = new StreamWriter(@"gtfs\\calendar.txt"))
            {
                using (var gtfstrips = new StreamWriter(@"gtfs\\trips.txt"))
                {
                    using (var gtfsstoptimes = new StreamWriter(@"gtfs\\stop_times.txt"))
                    {
                        // Headers 
                        var csvstoptimes = new CsvWriter(gtfsstoptimes);
                        csvstoptimes.Configuration.Delimiter = ",";
                        csvstoptimes.Configuration.Encoding = Encoding.UTF8;
                        csvstoptimes.Configuration.TrimFields = true;
                        // header 
                        csvstoptimes.WriteField("trip_id");
                        csvstoptimes.WriteField("arrival_time");
                        csvstoptimes.WriteField("departure_time");
                        csvstoptimes.WriteField("stop_id");
                        csvstoptimes.WriteField("stop_sequence");
                        csvstoptimes.WriteField("stop_headsign");
                        csvstoptimes.WriteField("pickup_type");
                        csvstoptimes.WriteField("drop_off_type");
                        csvstoptimes.WriteField("shape_dist_traveled");
                        csvstoptimes.WriteField("timepoint");
                        csvstoptimes.NextRecord();

                        var csvtrips = new CsvWriter(gtfstrips);
                        csvtrips.Configuration.Delimiter = ",";
                        csvtrips.Configuration.Encoding = Encoding.UTF8;
                        csvtrips.Configuration.TrimFields = true;
                        // header 
                        csvtrips.WriteField("route_id");
                        csvtrips.WriteField("service_id");
                        csvtrips.WriteField("trip_id");
                        csvtrips.WriteField("trip_headsign");
                        csvtrips.WriteField("trip_short_name");
                        csvtrips.WriteField("direction_id");
                        csvtrips.WriteField("block_id");
                        csvtrips.WriteField("shape_id");
                        csvtrips.WriteField("wheelchair_accessible");
                        csvtrips.WriteField("bikes_allowed ");
                        csvtrips.NextRecord();

                        var csvcalendar = new CsvWriter(gtfscalendar);
                        csvcalendar.Configuration.Delimiter = ",";
                        csvcalendar.Configuration.Encoding = Encoding.UTF8;
                        csvcalendar.Configuration.TrimFields = true;
                        // header 
                        csvcalendar.WriteField("service_id");
                        csvcalendar.WriteField("monday");
                        csvcalendar.WriteField("tuesday");
                        csvcalendar.WriteField("wednesday");
                        csvcalendar.WriteField("thursday");
                        csvcalendar.WriteField("friday");
                        csvcalendar.WriteField("saturday");
                        csvcalendar.WriteField("sunday");
                        csvcalendar.WriteField("start_date");
                        csvcalendar.WriteField("end_date");
                        csvcalendar.NextRecord();

                        //1101 International Air Service
                        //1102 Domestic Air Service
                        //1103 Intercontinental Air Service
                        //1104 Domestic Scheduled Air Service

                        for (int i = 0; i < CIFLights.Count; i++) // Loop through List with for)
                        {

                            // Calender

                            csvcalendar.WriteField(CIFLights[i].FromIATA + CIFLights[i].ToIATA + CIFLights[i].FlightAirline + CIFLights[i].FlightNumber.Replace(" ", "") + String.Format("{0:yyyyMMdd}", CIFLights[i].FromDate) + String.Format("{0:yyyyMMdd}", CIFLights[i].ToDate) + Convert.ToInt32(CIFLights[i].FlightMonday) + Convert.ToInt32(CIFLights[i].FlightTuesday) + Convert.ToInt32(CIFLights[i].FlightWednesday) + Convert.ToInt32(CIFLights[i].FlightThursday) + Convert.ToInt32(CIFLights[i].FlightFriday) + Convert.ToInt32(CIFLights[i].FlightSaterday) + Convert.ToInt32(CIFLights[i].FlightSunday));
                            csvcalendar.WriteField(Convert.ToInt32(CIFLights[i].FlightMonday));
                            csvcalendar.WriteField(Convert.ToInt32(CIFLights[i].FlightTuesday));
                            csvcalendar.WriteField(Convert.ToInt32(CIFLights[i].FlightWednesday));
                            csvcalendar.WriteField(Convert.ToInt32(CIFLights[i].FlightThursday));
                            csvcalendar.WriteField(Convert.ToInt32(CIFLights[i].FlightFriday));
                            csvcalendar.WriteField(Convert.ToInt32(CIFLights[i].FlightSaterday));
                            csvcalendar.WriteField(Convert.ToInt32(CIFLights[i].FlightSunday));
                            csvcalendar.WriteField(String.Format("{0:yyyyMMdd}", CIFLights[i].FromDate));
                            csvcalendar.WriteField(String.Format("{0:yyyyMMdd}", CIFLights[i].ToDate));
                            csvcalendar.NextRecord();

                            // Trips

                            //var item4 = _Airlines.Find(q => q.Name == CIFLights[i].FlightAirline);
                            //string TEMP_IATA = item4.IATA;
                            var FromAirportInfo = IATAAirports.Find(q => q.stop_iata == CIFLights[i].FromIATA);
                            var ToAirportInfo = IATAAirports.Find(q => q.stop_iata == CIFLights[i].ToIATA);

                            csvtrips.WriteField(CIFLights[i].FromIATA + CIFLights[i].ToIATA + CIFLights[i].FlightAirline);
                            csvtrips.WriteField(CIFLights[i].FromIATA + CIFLights[i].ToIATA + CIFLights[i].FlightAirline + CIFLights[i].FlightNumber.Replace(" ", "") + String.Format("{0:yyyyMMdd}", CIFLights[i].FromDate) + String.Format("{0:yyyyMMdd}", CIFLights[i].ToDate) + Convert.ToInt32(CIFLights[i].FlightMonday) + Convert.ToInt32(CIFLights[i].FlightTuesday) + Convert.ToInt32(CIFLights[i].FlightWednesday) + Convert.ToInt32(CIFLights[i].FlightThursday) + Convert.ToInt32(CIFLights[i].FlightFriday) + Convert.ToInt32(CIFLights[i].FlightSaterday) + Convert.ToInt32(CIFLights[i].FlightSunday));
                            csvtrips.WriteField(CIFLights[i].FromIATA + CIFLights[i].ToIATA + CIFLights[i].FlightAirline + CIFLights[i].FlightNumber.Replace(" ", "") + String.Format("{0:yyyyMMdd}", CIFLights[i].FromDate) + String.Format("{0:yyyyMMdd}", CIFLights[i].ToDate) + Convert.ToInt32(CIFLights[i].FlightMonday) + Convert.ToInt32(CIFLights[i].FlightTuesday) + Convert.ToInt32(CIFLights[i].FlightWednesday) + Convert.ToInt32(CIFLights[i].FlightThursday) + Convert.ToInt32(CIFLights[i].FlightFriday) + Convert.ToInt32(CIFLights[i].FlightSaterday) + Convert.ToInt32(CIFLights[i].FlightSunday));
                            csvtrips.WriteField(ToAirportInfo.stop_city);
                            csvtrips.WriteField(CIFLights[i].FlightNumber);
                            csvtrips.WriteField("");
                            csvtrips.WriteField("");
                            csvtrips.WriteField("");
                            csvtrips.WriteField("1");
                            csvtrips.WriteField("");
                            csvtrips.NextRecord();

                            // Depart Record
                            csvstoptimes.WriteField(CIFLights[i].FromIATA + CIFLights[i].ToIATA + CIFLights[i].FlightAirline + CIFLights[i].FlightNumber.Replace(" ", "") + String.Format("{0:yyyyMMdd}", CIFLights[i].FromDate) + String.Format("{0:yyyyMMdd}", CIFLights[i].ToDate) + Convert.ToInt32(CIFLights[i].FlightMonday) + Convert.ToInt32(CIFLights[i].FlightTuesday) + Convert.ToInt32(CIFLights[i].FlightWednesday) + Convert.ToInt32(CIFLights[i].FlightThursday) + Convert.ToInt32(CIFLights[i].FlightFriday) + Convert.ToInt32(CIFLights[i].FlightSaterday) + Convert.ToInt32(CIFLights[i].FlightSunday));
                            csvstoptimes.WriteField(String.Format("{0:HH:mm:ss}", CIFLights[i].DepartTime));
                            csvstoptimes.WriteField(String.Format("{0:HH:mm:ss}", CIFLights[i].DepartTime));
                            csvstoptimes.WriteField(FromAirportInfo.stop_city);
                            csvstoptimes.WriteField("0");
                            csvstoptimes.WriteField("");
                            csvstoptimes.WriteField("0");
                            csvstoptimes.WriteField("0");
                            csvstoptimes.WriteField("");
                            csvstoptimes.WriteField("");
                            csvstoptimes.NextRecord();
                            // Arrival Record
                            if (CIFLights[i].DepartTime.TimeOfDay < System.TimeSpan.Parse("23:59:59") && CIFLights[i].ArrivalTime.TimeOfDay > System.TimeSpan.Parse("00:00:00"))
                            //if (!CIFLights[i].FlightNextDayArrival)
                            {
                                csvstoptimes.WriteField(CIFLights[i].FromIATA + CIFLights[i].ToIATA + CIFLights[i].FlightAirline + CIFLights[i].FlightNumber.Replace(" ", "") + String.Format("{0:yyyyMMdd}", CIFLights[i].FromDate) + String.Format("{0:yyyyMMdd}", CIFLights[i].ToDate) + Convert.ToInt32(CIFLights[i].FlightMonday) + Convert.ToInt32(CIFLights[i].FlightTuesday) + Convert.ToInt32(CIFLights[i].FlightWednesday) + Convert.ToInt32(CIFLights[i].FlightThursday) + Convert.ToInt32(CIFLights[i].FlightFriday) + Convert.ToInt32(CIFLights[i].FlightSaterday) + Convert.ToInt32(CIFLights[i].FlightSunday));
                                csvstoptimes.WriteField(String.Format("{0:HH:mm:ss}", CIFLights[i].ArrivalTime));
                                csvstoptimes.WriteField(String.Format("{0:HH:mm:ss}", CIFLights[i].ArrivalTime));
                                csvstoptimes.WriteField(ToAirportInfo.stop_city);
                                csvstoptimes.WriteField("2");
                                csvstoptimes.WriteField("");
                                csvstoptimes.WriteField("0");
                                csvstoptimes.WriteField("0");
                                csvstoptimes.WriteField("");
                                csvstoptimes.WriteField("");
                                csvstoptimes.NextRecord();
                            }
                            else
                            {
                                //add 24 hour for the gtfs time
                                int hour = CIFLights[i].ArrivalTime.Hour;
                                hour = hour + 24;
                                int minute = CIFLights[i].ArrivalTime.Minute;
                                string strminute = minute.ToString();
                                if (strminute.Length == 1) { strminute = "0" + strminute; }
                                csvstoptimes.WriteField(CIFLights[i].FromIATA + CIFLights[i].ToIATA + CIFLights[i].FlightAirline + CIFLights[i].FlightNumber.Replace(" ", "") + String.Format("{0:yyyyMMdd}", CIFLights[i].FromDate) + String.Format("{0:yyyyMMdd}", CIFLights[i].ToDate) + Convert.ToInt32(CIFLights[i].FlightMonday) + Convert.ToInt32(CIFLights[i].FlightTuesday) + Convert.ToInt32(CIFLights[i].FlightWednesday) + Convert.ToInt32(CIFLights[i].FlightThursday) + Convert.ToInt32(CIFLights[i].FlightFriday) + Convert.ToInt32(CIFLights[i].FlightSaterday) + Convert.ToInt32(CIFLights[i].FlightSunday));
                                csvstoptimes.WriteField(hour + ":" + strminute + ":00");
                                csvstoptimes.WriteField(hour + ":" + strminute + ":00");
                                csvstoptimes.WriteField(ToAirportInfo.stop_city);
                                csvstoptimes.WriteField("2");
                                csvstoptimes.WriteField("");
                                csvstoptimes.WriteField("0");
                                csvstoptimes.WriteField("0");
                                csvstoptimes.WriteField("");
                                csvstoptimes.WriteField("");
                                csvstoptimes.NextRecord();
                            }
                        }
                    }
                }
            }
            // Create Zip File
            string startPath = myDir;
            string zipPath = gtfsDir + "\\LuftHansa.zip";
            if (File.Exists(zipPath)) { File.Delete(zipPath); }
            ZipFile.CreateFromDirectory(startPath, zipPath, CompressionLevel.Fastest, false);

            //Console.WriteLine("Insert into Database...");
            //for (int i = 0; i < CIFLights.Count; i++) // Loop through List with for)
            //{
            //    using (SqlConnection connection = new SqlConnection("Server=(local);Database=CI-Import;Trusted_Connection=True;"))
            //    {
            //        using (SqlCommand command = new SqlCommand())
            //        {
            //            command.Connection = connection;            // <== lacking
            //            command.CommandType = CommandType.StoredProcedure;
            //            command.CommandText = "InsertFlight";
            //            command.Parameters.Add(new SqlParameter("@FlightSource", "Lufthansa"));
            //            command.Parameters.Add(new SqlParameter("@FromIATA", CIFLights[i].FromIATA));
            //            command.Parameters.Add(new SqlParameter("@ToIATA", CIFLights[i].ToIATA));
            //            command.Parameters.Add(new SqlParameter("@FromDate", CIFLights[i].FromDate));
            //            command.Parameters.Add(new SqlParameter("@ToDate", CIFLights[i].ToDate));
            //            command.Parameters.Add(new SqlParameter("@FlightMonday", CIFLights[i].FlightMonday));
            //            command.Parameters.Add(new SqlParameter("@FlightTuesday", CIFLights[i].FlightTuesday));
            //            command.Parameters.Add(new SqlParameter("@FlightWednesday", CIFLights[i].FlightWednesday));
            //            command.Parameters.Add(new SqlParameter("@FlightThursday", CIFLights[i].FlightThursday));
            //            command.Parameters.Add(new SqlParameter("@FlightFriday", CIFLights[i].FlightFriday));
            //            command.Parameters.Add(new SqlParameter("@FlightSaterday", CIFLights[i].FlightSaterday));
            //            command.Parameters.Add(new SqlParameter("@FlightSunday", CIFLights[i].FlightSunday));
            //            command.Parameters.Add(new SqlParameter("@DepartTime", CIFLights[i].DepartTime));
            //            command.Parameters.Add(new SqlParameter("@ArrivalTime", CIFLights[i].ArrivalTime));
            //            command.Parameters.Add(new SqlParameter("@FlightNumber", CIFLights[i].FlightNumber));
            //            command.Parameters.Add(new SqlParameter("@FlightAirline", CIFLights[i].FlightAirline));
            //            command.Parameters.Add(new SqlParameter("@FlightOperator", CIFLights[i].FlightOperator));
            //            command.Parameters.Add(new SqlParameter("@FlightAircraft", CIFLights[i].FlightAircraft));
            //            command.Parameters.Add(new SqlParameter("@FlightCodeShare", CIFLights[i].FlightCodeShare));
            //            command.Parameters.Add(new SqlParameter("@FlightNextDayArrival", CIFLights[i].FlightNextDayArrival));
            //            command.Parameters.Add(new SqlParameter("@FlightDuration", CIFLights[i].FlightDuration));
            //            command.Parameters.Add(new SqlParameter("@FlightNextDays", CIFLights[i].FlightNextDays));
            //            foreach (SqlParameter parameter in command.Parameters)
            //            {
            //                if (parameter.Value == null)
            //                {
            //                    parameter.Value = DBNull.Value;
            //                }
            //            }


            //            try
            //            {
            //                connection.Open();
            //                int recordsAffected = command.ExecuteNonQuery();
            //            }

            //            finally
            //            {
            //                connection.Close();
            //            }
            //        }
            //    }

            //}

        }




    }

}

