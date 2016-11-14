﻿using Newtonsoft.Json;
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
        public Object[] bounds;
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

            // -i   Input File one address per line
            String argInputFile = null;

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
                            break;

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
                            throw new Exception("Invalid argument specified!");
                           
                    }//endof argument switch statement
                }//endof argument loop
            }//endof test if arguments exist



            String jsonGeoCodeResponse = null;



            //if user did not specifiy an address or an input file containing addresses
            //then prompt the user to enter an address at this time
            if (argAddress == null && argInputFile == null)
            {
                Console.WriteLine("Please enter an address ex: 1600+Amphitheatre+Parkway,+Mountain+View,+CA");
                argAddress = Console.ReadLine();
                jsonGeoCodeResponse = GeoCodeAddress(argAddress, apiKey);

                if (argOutputFile != null)
                {
                    WriteResultsToFile(jsonGeoCodeResponse,argOutputFile);
                }
                else
                {
                    WriteResultsToConsole(jsonGeoCodeResponse);
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
                            jsonGeoCodeResponse = GeoCodeAddress(argAddress, apiKey);

                            Console.WriteLine(argAddress);

                            if (argOutputFile != null)
                            {
                                WriteResultsToFile(jsonGeoCodeResponse, argOutputFile);
                            }
                            else
                            {
                                WriteResultsToConsole(jsonGeoCodeResponse);
                            }

                        }
                    }
               

            }

            







           


        }//endof main method


        private static void PrintUsageExample()
        {
            Console.WriteLine("\nUsage: GoogleGeoCode [[-a Address]|[-i InputFile]] [-o OutputFile]");
        }


        private static String GeoCodeAddress(String argAddress, String apiKey)
        {
            string requestUri = "https://maps.googleapis.com/maps/api/geocode/json?" + "address=" + argAddress + "&key=" + apiKey;

            WebRequest request = WebRequest.Create(requestUri);
            WebResponse response = request.GetResponse();

            //display the http status of the response
            //Console.WriteLine(((HttpWebResponse)response).StatusDescription);

            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();

            reader.Close();
            response.Close();

            return responseFromServer;
        }



        private static void WriteResultsToFile(String jsonGeoCodeResponse, String outputFilePath)
        {
            
            return;
        }


        private static void WriteResultsToConsole(String jsonGeoCodeResponse)
        {
            //deserialize the json response
            GoogleMapsResponse googResp = JsonConvert.DeserializeObject<GoogleMapsResponse>(jsonGeoCodeResponse);


            if (googResp.status == "OK")
            {
                if (googResp.results.Length > 0)
                {
                    for (int i = 0; i < googResp.results.Length; i++)
                    {
                        Console.Write("lat = " + googResp.results[i].geometry.location.lat);
                        Console.WriteLine("lng = " + googResp.results[i].geometry.location.lng);
                    }
                }
                else
                {
                    Console.WriteLine("zero results returned");
                }
            }

            return;
        }



    }
}
