using System;

namespace Searcharoo.Common 
{
    /// <summary>
    /// Retrieve data from web.config (or app.config)
    /// </summary>
    public static class Preferences
    {
        #region Private Fields
        private static int _RecursionLimit;
        private static string _UserAgent;
        private static string _RobotUserAgent;
        private static int _RequestTimeout;
        private static bool _AssumeAllWordsAreEnglish;
        private static string _IgnoreRegionTagNoIndex;
        private static int _SummaryCharacters;
        private static string _DownloadedTempFilePath;
        private static bool _InMediumTrust;
        private static string _ProxyUrl;
        private static string _DefaultDocument;
        #endregion

        /// <summary>
        /// Load preferences from *.config (web.config for ASPX, app.config for the console program)
        /// and apply defaults where the values are not found.
        /// </summary>
        static Preferences()
        { 
            _AssumeAllWordsAreEnglish = true;

            _RecursionLimit = IfNullDefault("Searcharoo_RecursionLimit", 200);
            _UserAgent =  IfNullDefault("Searcharoo_UserAgent", "Mozilla/6.0 (MSIE 6.0; Windows NT 5.1; Searcharoo.NET; robot)");
            _RobotUserAgent = IfNullDefault("Searcharoo_RobotUserAgent", "Searcharoo");
            _RequestTimeout = IfNullDefault("Searcharoo_RequestTimeout", 5);
            _IgnoreRegionTagNoIndex = IfNullDefault("Searcharoo_IgnoreRegionTagNoIndex", "");
            _SummaryCharacters  = IfNullDefault("Searcharoo_SummaryChars", 350);

            _DownloadedTempFilePath = IfNullDefault("Searcharoo_TempFilepath", "");
            _InMediumTrust = IfNullDefault("Searcharoo_InMediumTrust", true);

            _ProxyUrl = IfNullDefault("Searcharoo_ProxyUrl", "");

            _DefaultDocument = IfNullDefault("Searcharoo_DefaultDocument", ""); //[v7]
        }
        /// <summary>
        /// Seconds to wait for a page to respond, before giving up. 
        /// Default: 5 seconds
        /// </summary>
        public static int RequestTimeout
        {
            get
            {
                return _RequestTimeout; // IfNullDefault("Searcharoo_RequestTimeout", 5);
            }
        }

        /// <summary>
        /// First page to search - should have LOTS of links to follow
        /// Default: http://localhost/
        /// </summary>
        public static string StartPage
        {
            get
            {
                return IfNullDefault("Searcharoo_VirtualRoot", @"http://localhost/");
            }
        }

        /// <summary>
        /// Limit to the number of 'levels' of links to follow
        /// Default: 200 
        /// </summary>
        public static int RecursionLimit
        {
            get
            {
                return _RecursionLimit; // IfNullDefault("Searcharoo_RecursionLimit", 200);
            }
        }

        /// <summary>
        /// Request another page after waiting x seconds; use zero ONLY on your own/internal sites
        /// Default: 1 
        /// </summary>
        [Obsolete("Not currently used")]
        public static int SpeedLimit
        {
            get
            {
                return IfNullDefault("Searcharoo_SpeedLimit", 1);
            }
        }

        /// <summary>
        /// Whether to use stemming (English only), and if so, what mode [ Off | StemOnly | StemAndOriginal ]
        /// Default: Off
        /// </summary>
        public static int StemmingMode
        {
            get
            {
                return IfNullDefault("Searcharoo_StemmingType", 0);
            }
        }

        /// <summary>
        /// Whether to use stemming (English only), and if so, what mode [ Off | Short | List ]
        /// Default: Off
        /// </summary>
        public static int StoppingMode
        {
            get
            {
                return IfNullDefault("Searcharoo_StoppingType", 0);
            }
        }
        /// <summary>
        /// Whether to use go words (English only), and if so, what mode [ Off | On ]
        /// Default: Off
        /// </summary>
        public static int GoWordMode
        {
            get
            {
                return IfNullDefault("Searcharoo_GoType", 0);
            }
        }

        /// <summary>
        /// Number of characters to include in 'file summary'
        /// Default: 350
        /// </summary>
        public static int SummaryCharacters
        {
            get
            {
                return _SummaryCharacters; // IfNullDefault("Searcharoo_SummaryChars", 350);
            }
        }

        /// <summary>
        /// User Agent sent with page requests, in case you wish to change it
        /// Default: Mozilla/6.0 (MSIE 6.0; Windows NT 5.1; Searcharoo.NET; robot)
        /// </summary>
        public static string UserAgent
        {
            get
            {
                return _UserAgent; // IfNullDefault("Searcharoo_UserAgent", "Mozilla/6.0 (MSIE 6.0; Windows NT 5.1; Searcharoo.NET; robot)");
            }
        }

        /// <summary>
        /// User Agent detected by robots
        /// Default: Searcharoo
        /// </summary>
        public static string RobotUserAgent
        {
            get
            {
                return _RobotUserAgent; // IfNullDefault("Searcharoo_RobotUserAgent", "Searcharoo");
            }
        }

        /// <summary>
        /// Application[] cache key where the Catalog is stored, in case you need to alter it
        /// Default: Searcharoo_Catalog
        /// </summary>
        public static string CatalogCacheKey
        {
            get
            {
                return IfNullDefault("Searcharoo_CacheKey", "Searcharoo_Catalog");
            }
        }

        /// <summary>
        /// Name of file where the Catalog object is serialized (.dat and .xml)
        /// Default: searcharoo
        /// </summary>
        public static string CatalogFileName
        {
            get
            {   //TODO: remove HttpContext dependency!
                string location = IfNullDefault("Searcharoo_CatalogFilepath", "");
                if (location == "")
                {
                    location = System.Web.HttpContext.Current.Server.MapPath("~/");
                }
                location = location + IfNullDefault("Searcharoo_CatalogFilename", "searcharoo");
                return location;
            }
        }

        /// <summary>
        /// Location to save files that must be downloaded to disk before indexing 
        /// (eg IFilter docs like Word, PDF, Powerpoint)
        /// Default: "C:\temp\"
        /// </summary>
        public static string DownloadedTempFilePath
        {
            get
            {   //TODO: remove HttpContext dependency!
                string location = _DownloadedTempFilePath; // IfNullDefault("Searcharoo_TempFilepath", "");
                if (location == "")
                {
                    if (null == System.Web.HttpContext.Current)
                    {
                        location = @"C:\Temp\";
                    }
                    else
                    { 
                        location = System.Web.HttpContext.Current.Server.MapPath("~/Temp/"); 
                    }
                }
                return location;
            }
        }

        /// <summary>
        /// Number of result links to include per page
        /// Default: 10
        /// </summary>
        public static int ResultsPerPage
        {
            get
            {
                return IfNullDefault("Searcharoo_DefaultResultsPerPage", 10);
            }
        }

        /// <summary>
        /// Language to use when none is supplied (or supplied language is not available)
        /// Default: en-US
        /// </summary>
        public static string DefaultLanguage
        {
            get
            {
                return IfNullDefault("Searcharoo_DefaultLanguage", "en-US");
            }
        }

        /// <summary>
        /// Whether to create the Xml Serialized Catalog (for debugging purposes).
        /// The filesize tends to be quite large (DOUBLE the source data size) so
        /// it is FALSE by default.
        /// Default: false
        /// </summary>
        public static bool DebugSerializeXml
        {
            get
            {
                return IfNullDefault("Searcharoo_DebugSerializeXml", false);
            }
        }

        /// <summary>
        /// Whether to create the Xml Serialized Catalog (for debugging purposes).
        /// The filesize tends to be quite large (DOUBLE the source data size) so
        /// it is FALSE by default.
        /// Default: false
        /// </summary>
        public static string QuerystringParameterName
        {
            get
            {
                return IfNullDefault("Searcharoo_QuerystringParameter", "searchfor");
            }
        }

        /// <summary>
        /// Tagname to extract from html documents before parsing (to 'ignore' menus, for example)
        /// </summary>
        public static string IgnoreRegionTagNoIndex
        {
            get
            {
                return _IgnoreRegionTagNoIndex; // IfNullDefault("Searcharoo_IgnoreRegionTagNoIndex", "");
            }
        }

        /// <summary>
        /// Whether to ignore sections of HTML wrapped in a special comment tag
        /// </summary>
        public static bool IgnoreRegions
        {
            get { return IgnoreRegionTagNoIndex.Length > 0;  }
        }

        /// <summary>
        /// Controls how agressively the parser strips punctuation from a word before indexing it
        /// </summary>
        public static bool AssumeAllWordsAreEnglish
        {
            get { return _AssumeAllWordsAreEnglish; }
        }

        /// <summary>
        /// Whether the application is running in medium-trust (and requires Xml rather than Binary serialization)
        /// Default: true
        /// </summary>
        public static bool InMediumTrust
        {
            get
            {
                return _InMediumTrust; // IfNullDefault("Searcharoo_RequestTimeout", 5);
            }
        }

        /// <summary>
        /// Whether a proxy server has been specified [v6]
        /// </summary>
        public static bool UseProxy
        {
            get {
                return (_ProxyUrl != "");
            }
        }
        /// <summary>
        /// eg. http://proxy:80/ [v6]
        /// </summary>
        public static string ProxyUrl
        {
            get {
                return _ProxyUrl;
            }
        }

        /// <summary>
        /// Whether to treat a 'directory root' as a call for the default document.
        /// Eg. /Documents/ is treated as the same url as /Documents/default.aspx
        /// </summary>
        public static bool UseDefaultDocument
        {
            get
            {
                return (_DefaultDocument != "");
            }
        }
        /// <summary>
        /// eg. default.aspx [v7]
        /// </summary>
        public static string DefaultDocument
        {
            get
            {
                return _DefaultDocument.ToLower();
            }
        }

        #region Private Methods: IfNullDefault
        private static int IfNullDefault(string appSetting, int defaultValue)
        {
            return System.Configuration.ConfigurationManager.AppSettings[appSetting] == null ? defaultValue : Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings[appSetting]);
        }
        private static string IfNullDefault(string appSetting, string defaultValue)
        {
            return System.Configuration.ConfigurationManager.AppSettings[appSetting] == null ? defaultValue : System.Configuration.ConfigurationManager.AppSettings[appSetting];
        }
        private static bool IfNullDefault(string appSetting, bool defaultValue)
        {
            return System.Configuration.ConfigurationManager.AppSettings[appSetting] == null ? defaultValue : Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings[appSetting]);
        }
        #endregion

    }  // Preferences class
}
