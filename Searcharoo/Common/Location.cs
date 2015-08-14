using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Searcharoo.Common
{
    [Serializable]
    public class Location
    {
        public Location () { }

        public Location (double latitude, double longitude) 
        {
            _Latitude = latitude;
            _Longitude = longitude;
        }
        [XmlIgnore]
        private double _Latitude = 0;
        [XmlIgnore]
        private double _Longitude = 0;

        [XmlAttribute("lat")]
        public double Latitude { get { return _Latitude; } set { _Latitude = value; } }
        [XmlAttribute("lon")]
        public double Longitude { get { return _Longitude; } set { _Longitude = value; } }

        
        public static Location FromString(string location) 
        {
            string[] coords = location.Split(new char[] { ',', ';' });
            if (coords.Length >= 2)
            {
                double lat = double.MinValue, lon = double.MinValue;
                bool ok = double.TryParse(coords[0], out lat);
                ok = ok && double.TryParse(coords[1], out lon);
                if (ok)
                {
                    return new Location(lat, lon);
                }
            }
            return null;
        }
        public override string ToString()
        {
            return _Latitude.ToString() + "," + _Longitude.ToString();
        }
    }
}
