using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using Searcharoo.Common.Extensions;

namespace Searcharoo.Common
{
    /// <summary>
    /// File attributes
    /// </summary>
    /// <remarks>
    /// *Beware* ambiguity with System.IO.File - always fully qualify File object references
    /// 
    /// in V6 added KeywordString, GpsLocation and Extension
    /// </remarks>
    [Serializable]
    [System.Xml.Serialization.XmlInclude(typeof(Searcharoo.Common.Location))]
    public class File
    {
        #region Fields
        [XmlIgnore]
        private string _Url;
        [XmlIgnore]
        private string _Title;
        [XmlIgnore]
        private string _Extension; //[v6]
        [XmlIgnore]
        private string _Description;
        [XmlIgnore]
        private DateTime _CrawledDate;
        [XmlIgnore]
        private long _Size;
        [XmlIgnore]
        private Location _GpsLocation; //[v6]
        [XmlIgnore]
        private string _KeywordString; //[v6]
        [XmlIgnore]
        private int _IndexId; //[v7]
        #endregion

        [XmlAttribute("id")]
        public int IndexId
        {
            get { return _IndexId; }
            set { _IndexId = value; }
        }

        [XmlAttribute("u")]
        public string Url
        {
            get { return _Url; }
            set { _Url = value.UnicodeToCharacter(); }
        }
        [XmlAttribute("t")]
        public string Title
        {
            get { return _Title; }
            set { _Title = value.UnicodeToCharacter(); }
        }
        [XmlAttribute("e")]
        public string Extension
        {
            get { return _Extension; }
            set { _Extension = value; }
        }
        [XmlElement("d")]
        public string Description
        {
            get { return _Description; }
            set { _Description = value.UnicodeToCharacter(); }
        }
        [XmlAttribute("d")]
        public DateTime CrawledDate
        {
            get { return _CrawledDate; }
            set { _CrawledDate = value; }
        }
        [XmlAttribute("s")]
        public long Size
        {
            get { return _Size; }
            set { _Size = value; }
        }
        /// <summary>
        /// Cannot be serialized as an XmlAttribute - it's a complex type; slightly less space-efficient
        /// </summary>
        [XmlElement("gps")]
        public Location GpsLocation
        {
            get { return _GpsLocation; }
            set { _GpsLocation = value; }
        }
        /// <summary>
        /// Keyword string (comma seperated)
        /// </summary>
        [XmlElement("kw")]
        public string KeywordString
        {
            get { return _KeywordString; }
            set { _KeywordString = value.UnicodeToCharacter(); }
        }
        /// <summary>
        /// Required for serialization
        /// </summary>
        public File() 
        {
            _IndexId = -1;  // [v7]
        }

        /// <summary>
        /// Constructor requires all File attributes
        /// </summary>
        [Obsolete("Expects keywords, gpsLocation and extension in version 6")]
        public File(string url, string title, string description, DateTime datecrawl, long length)
        {
            _Title = title.UnicodeToCharacter();
            _Description = description.UnicodeToCharacter();
            _CrawledDate = datecrawl;
            _Url = url.UnicodeToCharacter();
            _Size = length;
            _IndexId = -1;  // [v7]
        }
       
        /// <summary>
        /// Constructor requires all File attributes
        /// </summary>
        public File(string url, string title, string description, DateTime datecrawl, long length
            , Location gpsLocation
            , string extension
            , string keywords)
        {
            _Title = title.UnicodeToCharacter();
            _Description = description.UnicodeToCharacter();
            _CrawledDate = datecrawl;
            _Url = url.UnicodeToCharacter();
            _Size = length;
            _GpsLocation = gpsLocation;
            _Extension = extension;
            _KeywordString = keywords.UnicodeToCharacter();
            _IndexId = -1;  // [v7]
        }

        /// <summary>
        /// Debug string
        /// </summary>
        public override string ToString()
        {
            return "\tFILE " + IndexId + " :: " + Url + " -- " + Title + " - " + Size + " bytes + \n\t" + Description + "\n";
        }
    }
}
