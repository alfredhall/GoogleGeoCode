using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;





namespace alfredhall
{


    class GoogleMapsResponse
    {
        public GoogleMapsResult[] results;
        public String status;
        public String error_message;
        public String jsonServerResponse;
    }

    class GoogleMapsResult
    {
        public String[] types;
        public String formatted_address;
        public GoogleMapsAddressComponent[] address_components;
        public String[] postcode_localities;
        public GoogleMapsGeometry geometry;
        public String partial_match;
        public String place_id;
    }

    class GoogleMapsAddressComponent
    {
        public String[] types;
        public String long_name;
        public String short_name;
    }

    class GoogleMapsGeometry
    {
        public GoogleMapsLocation location;
        public String location_type;
        public GoogleMapsViewport viewport;
        public GoogleMapsBounds bounds;
    }

    class GoogleMapsBounds
    {
        public GoogleMapsLocation northEast;
        public GoogleMapsLocation southWest;
    }

    class GoogleMapsLocation
    {
        public String lat;
        public String lng;
    }

    class GoogleMapsViewport
    {
        public GoogleMapsLocation northeast;
        public GoogleMapsLocation southwest;
    }



    class GoogleGeoCode
    {
        static void Main(string[] args)
        {

            /*
             * Documentation for Google Geocoding API located here
             * https://developers.google.com/maps/documentation/geocoding/intro
             * 
             * 
             */


            //Get our GoogleMaps Geocoding API Key from application configuration file
            string apiKey = ConfigurationManager.AppSettings["GoogleMapsGeocodingAPIKey"].ToString();


            /****  Command Line Arguments ****/

            // -a   Address, single address entered on command line
            String argAddress = null;

            // -h   Help, display help menu

            // -i   Input File one address per line
            String argInputFile = null;

            // -j   Display json output to screen
            bool displayJsonOutput = false;

            // -o   Output File latitute/longitude pair per line
            String argOutputFile = null; 

            //if arguments were specified on the command line
            //then loop through and parse out each argument
            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        //address
                        case "-a":
                            if ((i + 1) < args.Length)
                            {
                                argAddress = args[i + 1];
                                i++;
                            }
                            if( argInputFile != null) //if both address and input file specified, error
                            {
                                throw new Exception("Invalid argument specified!");
                            }
                            break;

                        //help 
                        case "-h":
                            PrintUsageExample();
                            return;
                            

                        //input file argument
                        case "-i":
                            if ((i + 1) < args.Length)
                            {
                                argInputFile = args[i + 1];
                                i++;
                            }
                            if (argAddress != null)  //if both input file and address specified, Error
                            {
                                throw new Exception("Invalid argument specified!");
                            }
                            if( File.Exists(argInputFile) == false) //make sure the input file exists
                            {
                                throw new Exception("Input file does not exist!");
                            }
                            break;

                        //display json response to console
                        case "-j":
                            displayJsonOutput = true;
                            break;
                        
                        //output file argument
                        case "-o":
                            if ((i + 1) < args.Length)
                            {
                                argOutputFile = args[i + 1];
                                i++;
                            }
                            break;

                        //invalid argument detected
                        default:
                            {
                                throw new Exception("Invalid argument specified!");
                            }

                    }//endof argument switch statement

                }//endof argument loop
            }//endof test if arguments exist



            GoogleMapsResponse googleResponse = null;
            System.IO.StreamWriter outputFile = null;



            if (argOutputFile != null)
            {
                outputFile = new StreamWriter(argOutputFile);
            }


            //if user did not specifiy an address or an input file containing addresses
            //then prompt the user to enter an address at this time
            if (argAddress == null && argInputFile == null)
            {
                Console.WriteLine("Please enter an address ex: 1600+Amphitheatre+Parkway,+Mountain+View,+CA");
                argAddress = Console.ReadLine();
                googleResponse = GeoCodeAddress(argAddress, apiKey);

                if (displayJsonOutput == true)
                {
                    Console.WriteLine(googleResponse.jsonServerResponse);
                }

                if (argOutputFile != null )
                {
                    
                        WriteResultsToFile(argAddress, googleResponse, outputFile);
                    
                }
                else
                {
                    
                        WriteResultsToConsole(argAddress, googleResponse);
                    
                }

            }



            //if user specified to use an input file
            //read each line of file, then geocode, then print to file (or screen by default)
            if( argInputFile != null)
            {
                
                    using (StreamReader sr = new StreamReader(argInputFile))
                    {
                        
                        while ((argAddress = sr.ReadLine()) != null)
                        {
                        googleResponse = GeoCodeAddress(argAddress, apiKey);

                            if (displayJsonOutput == true)
                            {
                                Console.WriteLine(googleResponse.jsonServerResponse);
                            }

                            if (argOutputFile != null)
                            {
                                
                                WriteResultsToFile(argAddress, googleResponse, outputFile);
                            }
                            else
                            {
                                WriteResultsToConsole(argAddress, googleResponse);
                            }

                        }
                    }
               

            }





            if (outputFile != null)
            {
                outputFile.Flush();
                outputFile.Close();
            }



           


        }//endof main method








        private static void PrintUsageExample()
        {
            Console.WriteLine("\nUsage: GoogleGeoCode [-h][[[-a Address]|[-i InputFile]] [-o OutputFile] [-j]]");
            Console.WriteLine("GoogleGeoCode -h     will display this help message");
        }

        


            private static GoogleMapsResponse GeoCodeAddress(String argAddress, String apiKey)
        {
            //url encode argument address
            argAddress = WebUtility.UrlEncode(argAddress);

            //for testing purposes, maybe should make this a 
            //command line argument for displaying current address being processed
            Console.WriteLine(argAddress);

            string requestUri = "https://maps.googleapis.com/maps/api/geocode/json?" + "address=" + argAddress + "&key=" + apiKey;

            WebRequest request = WebRequest.Create(requestUri);

            WebResponse response = null;
            Stream dataStream = null;
            StreamReader reader = null;
            string responseFromServer = null;

            //to do , if the following line throws an exception, ive seen 500 thrown,then retry a few times
            //and if still receiving the error, exit the program
            try
            {
                response = request.GetResponse();
                dataStream = response.GetResponseStream();
                reader = new StreamReader(dataStream);
                responseFromServer = reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                //todo  RETRY a few times
                Console.WriteLine(ex.Message);
                if (response != null)
                {
                    if (((HttpWebResponse)response) != null)
                    {
                        String strDescription = ((HttpWebResponse)response).StatusDescription;
                        Console.WriteLine(strDescription);
                    }
                }
                return null;
            }


            reader.Close();
            response.Close();


            if (responseFromServer != null)
            {
                //deserialize the json response
                GoogleMapsResponse googResp = JsonConvert.DeserializeObject<GoogleMapsResponse>(responseFromServer);
                googResp.jsonServerResponse = responseFromServer;
                return googResp;
            }


            return null;
        }




        

        private static void WriteResultsToFile(String searchAddress, GoogleMapsResponse googResp, StreamWriter outputFile)
        {
            if( googResp == null)
            {
                return;
            }


            /*
                Check the status code that was returned from Google
              
                Status Codes

                The "status" field within the Geocoding response object contains the status of the request, and may contain debugging information to help you track down why geocoding is not working. The "status" field may contain the following values:

                "OK" indicates that no errors occurred; the address was successfully parsed and at least one geocode was returned.
                
                "ZERO_RESULTS" indicates that the geocode was successful but returned no results. This may occur if the geocoder was passed a non-existent address.
                
                "OVER_QUERY_LIMIT" indicates that you are over your quota.
                
                "REQUEST_DENIED" indicates that your request was denied.
                
                "INVALID_REQUEST" generally indicates that the query (address, components or latlng) is missing.
                
                "UNKNOWN_ERROR" indicates that the request could not be processed due to a server error. The request may succeed if you try again.
             */

            if (googResp.status == "OK")
            {
                if (googResp.results.Length >= 0)
                {
                    for (int i = 0; i < googResp.results.Length; i++)
                    {
                        if (googResp.results[i].partial_match != null &&
                            googResp.results[i].partial_match.CompareTo("true") == 0)
                        {
                            outputFile.WriteLine(searchAddress + "|partial|" + googResp.results[i].formatted_address + "|" + googResp.results[i].geometry.location.lat + "|" + googResp.results[i].geometry.location.lng);
                        }
                        else
                        {
                            outputFile.WriteLine(searchAddress + "|exact|" + googResp.results[i].formatted_address + "|" + googResp.results[i].geometry.location.lat + "|" + googResp.results[i].geometry.location.lng);
                        }
                    }
                }
                else
                {
                    outputFile.WriteLine(searchAddress + "|" + "|" + "|");
                }
            }
            if( googResp.status == "OVER_QUERY_LIMIT")
            {
                //exit the application
                //Environment.Exit( exitCode );
                //first close out any open file handles
            }
            else
            {
                Console.WriteLine(googResp.error_message);
            }
        }


        private static void WriteResultsToConsole(String searchAddress, GoogleMapsResponse googResp)
        {
            if (googResp == null)
            {
                return;
            }
            


            if (googResp.status == "OK")
            {
                if (googResp.results.Length > 0)
                {
                    for (int i = 0; i < googResp.results.Length; i++)
                    {
                        if (googResp.results[i].partial_match != null &&
                            googResp.results[i].partial_match.CompareTo("true") == 0)
                        {
                            Console.WriteLine("partial match found for: " + searchAddress);
                            Console.WriteLine(googResp.results[i].formatted_address);
                            Console.Write("lat = " + googResp.results[i].geometry.location.lat);
                            Console.WriteLine(", lng = " + googResp.results[i].geometry.location.lng);
                        }
                        else
                        {
                            Console.WriteLine("exact match found for: " + searchAddress);
                            Console.WriteLine(googResp.results[i].formatted_address);
                            Console.Write("lat = " + googResp.results[i].geometry.location.lat);
                            Console.WriteLine(", lng = " + googResp.results[i].geometry.location.lng);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine(googResp.error_message);
            }

            return;
        }



    }
}
