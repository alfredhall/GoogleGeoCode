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

            Console.WriteLine("Please enter an address ex: 1600+Amphitheatre+Parkway,+Mountain+View,+CA");
            String argAddress = Console.ReadLine();
            Console.WriteLine(argAddress);
            

            string apiKey = ConfigurationManager.AppSettings["GoogleMapsGeocodingAPIKey"].ToString();
            //string address = "1600+Amphitheatre+Parkway,+Mountain+View,+CA";
            string requestUri = "https://maps.googleapis.com/maps/api/geocode/json?" + "address=" + argAddress + "&key=" + apiKey;

            WebRequest request = WebRequest.Create(requestUri);
            WebResponse response = request.GetResponse();

            //display the http status of the response
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);

            Stream dataStream = response.GetResponseStream();

            StreamReader reader = new StreamReader(dataStream);

            string responseFromServer = reader.ReadToEnd();

            Console.WriteLine(responseFromServer);

            reader.Close();
            response.Close();



            //deserialize the json response
            GoogleMapsResponse googResp = JsonConvert.DeserializeObject<GoogleMapsResponse>(responseFromServer);

            Console.WriteLine(googResp.status);
            Console.WriteLine("--------");
            Console.WriteLine(googResp.results.Length);

            if (googResp.status == "OK")
            {
                if (googResp.results.Length > 0)
                {
                    for (int i = 0; i < googResp.results.Length; i++)
                    {
                        Console.WriteLine("lat = " + googResp.results[i].geometry.location.lat);
                        Console.WriteLine("lng = " + googResp.results[i].geometry.location.lng);

                    }
                }
                else
                {
                    Console.WriteLine("zero results returned");
                }
            }


        }

    }
}
