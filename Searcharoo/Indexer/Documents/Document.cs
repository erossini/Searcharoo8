using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Searcharoo.Common
{
    /// <summary>
    /// Base class for any 'Document' that can be downloaded and indexed off the Internet
    /// </summary>
    public abstract class Document
    {
        #region Private fields: Uri, All, ContentType, MimeType, Extension, Title, Length, Description, Keywords, GpsLocation
        private Uri _Uri;
        private string _All;
        private string _ContentType;
        private string _MimeType = String.Empty;
        private string _Extension = String.Empty;
        private string _Title;
        private long _Length;
        /// <summary>Html &lt;meta http-equiv='description'&gt; tag</summary>
        private string _Description = String.Empty;
        /// <summary>Html &lt;meta http-equiv='keywords'&gt; tag</summary>
        private List<string> _Keywords = new List<string>();
        /// <summary>GPS location</summary>
        private Location _GpsLocation;
        #endregion

        /// <summary>
        /// Subclasses must implement GetResponse
        /// </summary>
        public abstract bool GetResponse(System.Net.HttpWebResponse webresponse);
        /// <summary>
        /// Subclasses must implement Parse
        /// </summary>
        public abstract void Parse();

        public ArrayList LocalLinks = new ArrayList();      // [v7] bugfix
        public ArrayList ExternalLinks = new ArrayList();   // [v7] bugfix

        public virtual string All
        {
            get { return _All; }
            set { _All = value; }
        }
        public virtual string ContentType
        {
            get { return _ContentType; }
            set { _ContentType = value; }
        }

        public virtual string MimeType
        {
            get { return _MimeType; }
            set { _MimeType = value; }
        }

        public virtual string Extension
        {
            get { return _Extension; }
            set { _Extension = value; }
        }

        public abstract string WordsOnly { get; }
        
        public virtual string Title
        {
            get { return _Title; }
            set { _Title = value; }
        }

        public virtual long Length
        {
            get { return _Length; }
            set { _Length = value; }
        }

        public virtual string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }
        /// <summary>
        /// Keywords (tags)
        /// </summary>
        public virtual List<string> Keywords
        {
            get { return _Keywords; }
            set { _Keywords = value; }
        }
        /// <summary>
        /// Comma-seperated list of keywords
        /// </summary>
        public string KeywordString 
        {
            get
            {
                string s = string.Empty;
                int i = 0;
                foreach (string word in _Keywords)
                {
                    if (i > 0) s += ", ";
                    s += word;
                    i++;
                }
                return s;
            }
            set 
            {
                SetKeywords(value);
            }
        }
        /// <summary>
        /// X = Longitude, Y = Latitude
        /// </summary>
        public Location GpsLocation
        {
            get { return _GpsLocation; }
            set { _GpsLocation = value; }
        }
        /// <summary>
        /// http://www.ietf.org/rfc/rfc2396.txt
        /// </summary>
        public virtual Uri Uri
        {
            get { return _Uri; }
            set { _Uri = value; }
        }

        public virtual string[] WordsArray
        {
            get { return this.WordsStringToArray(WordsOnly); }
        }

        /// <summary>
        /// Most document types don't have embedded robot information
        /// so they'll always be allowed to be followed 
        /// (assuming there are links to follow)
        /// </summary>
        public virtual bool RobotFollowOK
        {
            get { return true; }
        }
        /// <summary>
        /// Most document types don't have embedded robot information
        /// so they'll always be allowed to be indexed 
        /// (assuming there is content to index)
        /// </summary>
        public virtual bool RobotIndexOK
        {
            get { return true; }
        }

        /// <summary>
        /// Constructor for any document requires the Uri be specified
        /// </summary>
        public Document(Uri uri)
        {
            _Uri = uri;
        }

       

        protected string[] WordsStringToArray(string words)
        {
            // COMPRESS ALL WHITESPACE into a single space, seperating words
            if (!String.IsNullOrEmpty(words))
            {
                Regex r = new Regex(@"\s+");            //remove all whitespace
                string compressed = r.Replace(words, " ");
                return compressed.Split(' ');
            }
            else
            {
                return new string[0];
            }
        }


        protected string GetDescriptionFromWordsOnly(string wordsonly)
        {
            string description = string.Empty;
            if (wordsonly.Length > Preferences.SummaryCharacters)
            {
                description = wordsonly.Substring(0, Preferences.SummaryCharacters);
            }
            else
            {
                description = WordsOnly;
            }
            description = System.Text.RegularExpressions.Regex.Replace(description, @"\s+", " ").Trim();
            return description;
        }

        /// <summary>
        /// Added in [v6]
        /// </summary>
        /// <param name="keywords"></param>
        /// <returns></returns>
        protected bool SetKeywords(string keywords)
        {
            string[] words = keywords.Split(new char[] { ',', ';'});
            foreach (string word in words)
            {
                if (word.Trim() != "")
                {
                    this.Keywords.Add(word.Trim());
                }
            }
            return true;
        }

        /// <summary>
        /// Added in [v6]
        /// </summary>
        protected bool SetGpsCoordinates(string coordinates)
        {
            this._GpsLocation = Location.FromString(coordinates);
            return (_GpsLocation != null);
        }
    }
}