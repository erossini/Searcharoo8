using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using cd.net;

using Searcharoo.Indexer;

namespace Searcharoo.Common
{
    /// <summary>
    /// Storage for parsed HTML data returned by ParsedHtmlData();
    /// </summary>
    /// <remarks>
    /// Arbitrary class to encapsulate just the properties we need 
    /// to index Html pages (Title, Meta tags, Keywords, etc).
    /// A 'generic' search engine would probably have a 'generic'
    /// document class, so maybe a future version of Searcharoo 
    /// will too...
    /// </remarks>
    public class HtmlDocument : Document
    {
        #region Private fields: _Uri, _ContentType, _RobotIndexOK, _RobotFollowOK
        private string _All = String.Empty;
        private Uri _Uri;
        private String _ContentType;
        private bool _RobotIndexOK = true;
        private bool _RobotFollowOK = true;
        private string _WordsOnly = string.Empty;
        /// <summary>MimeType so we know whether to try and parse the contents, eg. "text/html", "text/plain", etc</summary>
        private string _MimeType = String.Empty;
        /// <summary>Html &lt;title&gt; tag</summary>
        private String _Title = String.Empty;
        /// <summary>Html &lt;meta http-equiv='description'&gt; tag</summary>
        private string _Description = String.Empty;
        /// <summary>Length as reported by the server in the Http headers</summary>
        #endregion

        #region Constructor requires Uri
        public HtmlDocument(Uri location):base(location)
        {
            _Uri = location;
            Extension = "html";
        }
        #endregion

        #region Public Properties: Uri, RobotIndexOK
        /// <summary>
        /// http://www.ietf.org/rfc/rfc2396.txt
        /// </summary>
        public override Uri Uri
        {
            get { return _Uri; }
            set
            {
                _Uri = value;
            }
        }
        /// <summary>
        /// Whether a robot should index the text 
        /// found on this page, or just ignore it
        /// </summary>
        /// <remarks>
        /// Set when page META tags are parsed - no 'set' property
        /// More info:
        /// http://www.robotstxt.org/
        /// </remarks>
        public override bool RobotIndexOK
        {
            get { return _RobotIndexOK; }
        }
        /// <summary>
        /// Whether a robot should follow any links 
        /// found on this page, or just ignore them
        /// </summary>
        /// <remarks>
        /// Set when page META tags are parsed - no 'set' property
        /// More info:
        /// http://www.robotstxt.org/
        /// </remarks>
        public override bool RobotFollowOK
        {
            get { return _RobotFollowOK; }
        }

        public override string ContentType
        {
            get
            {
                return _ContentType;
            }
            set
            {
                _ContentType = value.ToString();
                string[] contentTypeArray = _ContentType.Split(';');
                // Set MimeType if it's blank
                if (_MimeType == String.Empty && contentTypeArray.Length >= 1)
                {
                    _MimeType = contentTypeArray[0];
                }
                // Set Encoding if it's blank
                if (Encoding == String.Empty && contentTypeArray.Length >= 2)
                {
                    int charsetpos = contentTypeArray[1].IndexOf("charset");
                    if (charsetpos > 0)
                    {
                        Encoding = contentTypeArray[1].Substring(charsetpos + 8, contentTypeArray[1].Length - charsetpos - 8);
                    }
                }
            }
        }
        #endregion

        #region Public fields: Encoding, All
         /// <summary>Encoding eg. "utf-8", "Shift_JIS", "iso-8859-1", "gb2312", etc</summary>
        public string Encoding = String.Empty;
        
        /// <summary>
        /// Raw content of page, as downloaded from the server
        /// Html stripped to make up the 'wordsonly'
        /// </summary>
        public override string All
        {
            get { return _All; }
            set { 
                _All = value;
                _WordsOnly = StripHtml(_All);
            }
        }
        public override string WordsOnly
        {
            get { return this.KeywordString + this._Description + this._WordsOnly; }
        }

        public override string Description
        {
            get {
                // ### If no META DESC, grab start of file text ###
                if (String.Empty == this._Description)
                {
                    if (_WordsOnly.Length > Preferences.SummaryCharacters)
                    {
                        _Description = _WordsOnly.Substring(0, Preferences.SummaryCharacters);
                    }
                    else
                    {
                        _Description = WordsOnly;
                    }
                    _Description = Regex.Replace(_Description, @"\s+", " ").Trim();
                }
                // http://authors.aspalliance.com/stevesmith/articles/removewhitespace.asp
                return _Description; 
            }
            set 
            {
                _Description = Regex.Replace(value, @"\s+", " ").Trim();
            }
        }
        #endregion

        #region Public Methods: SetRobotDirective, ToString()
        /// <summary>
        /// Pass in a ROBOTS meta tag found while parsing, 
        /// and set HtmlDocument property/ies appropriately
        /// </summary>
        /// <remarks>
        /// More info:
        /// * Robots Exclusion Protocol *
        /// - for META tags
        /// http://www.robotstxt.org/wc/meta-user.html
        /// - for ROBOTS.TXT in the siteroot
        /// http://www.robotstxt.org/wc/norobots.html
        /// </remarks>
        public void SetRobotDirective (string robotMetaContent)
        {
            robotMetaContent = robotMetaContent.ToLower();
            if (robotMetaContent.IndexOf("none") >= 0)
            {
                // 'none' means you can't Index or Follow!
                _RobotIndexOK = false;
                _RobotFollowOK = false;
            }
            else
            {
                if (robotMetaContent.IndexOf("noindex") >= 0) { _RobotIndexOK = false; }
                if (robotMetaContent.IndexOf("nofollow") >= 0) { _RobotFollowOK = false; }
            }
        }

        /// <summary>
        /// For debugging - output all links found in the page
        /// </summary>
        public override string ToString()
        {
            string linkstring = "";
            foreach (object link in LocalLinks)
            {
                linkstring += Convert.ToString(link) + "\r\n";
            }
            return Title + "\r\n" + Description + "\r\n----------------\r\n" + linkstring + "\r\n----------------\r\n" + All + "\r\n======================\r\n";
        }
        #endregion


        /// <summary>
        ///
        /// </summary>
        /// <remarks>
        /// "Original" link search Regex used by the code was from here
        /// http://www.dotnetjunkies.com/Tutorial/1B219C93-7702-4ADF-9106-DFFDF90914CF.dcik
        /// but it was not sophisticated enough to match all tag permutations
        ///
        /// whereas the Regex on this blog will parse ALL attributes from within tags...
        /// IMPORTANT when they're out of order, spaced out or over multiple lines
        /// http://blogs.worldnomads.com.au/matthewb/archive/2003/10/24/158.aspx
        /// http://blogs.worldnomads.com.au/matthewb/archive/2004/04/06/215.aspx
        ///
        /// http://www.experts-exchange.com/Programming/Programming_Languages/C_Sharp/Q_20848043.html
        /// 
        /// Parse GPS coordinates (latitude, longitude) [v6]
        /// http://en.wikipedia.org/wiki/Geotagging
        /// </remarks>
        public override void Parse()
        {
            string htmlData = this.All;	// htmlData will be munged

            //xenomouse http://www.codeproject.com/aspnet/Spideroo.asp?msg=1271902#xx1271902xx
            if (string.IsNullOrEmpty(this.Title))
            {   // title may have been set previously... non-HTML file type (this will be refactored out, later)
                // this.Title = Regex.Match(htmlData, @"(?<=<title[^\>]*>).*?(?=</title>)", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture).Value;
                // [v6] fix by Erick Brown for CRLFs in <title> tag
                // "Not only will the above work with line breaks, it also works with more variations of improperly formatted tags.   Further, it will not incorrectly catch tags that begin with "title" such as: <titlepage>"
                this.Title = Regex.Match(
                      htmlData
                    , @"(?<=<s*title(?:\s[^>]*)?\>)[\s\S]*?(?=\</\s*title(?:\s[^>]*)?\>)"
                    , RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture).Value;
                this.Title = this.Title.Trim(); //new char[] { '\r', '\n', ' '});
            }

            string metaKey = String.Empty, metaValue = String.Empty;
            foreach (Match metamatch in Regex.Matches(htmlData
                , @"<meta\s*(?:(?:\b(\w|-)+\b\s*(?:=\s*(?:""[^""]*""|'[^']*'|[^""'<> ]+)\s*)?)*)/?\s*>"
                , RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture))
            {
                metaKey = String.Empty;
                metaValue = String.Empty;
                // Loop through the attribute/value pairs inside the tag
                foreach (Match submetamatch in Regex.Matches(metamatch.Value.ToString()
                    , @"(?<name>\b(\w|-)+\b)\s*=\s*(""(?<value>[^""]*)""|'(?<value>[^']*)'|(?<value>[^""'<> ]+)\s*)+"
                    , RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture))
                {

                    if ("http-equiv" == submetamatch.Groups[1].ToString().ToLower())
                    {
                        metaKey = submetamatch.Groups[2].ToString();
                    }
                    if (("name" == submetamatch.Groups[1].ToString().ToLower())
                        && (metaKey == String.Empty))
                    { // if it's already set, HTTP-EQUIV takes precedence
                        metaKey = submetamatch.Groups[2].ToString();
                    }
                    if ("content" == submetamatch.Groups[1].ToString().ToLower())
                    {
                        metaValue = submetamatch.Groups[2].ToString();
                    }
                }
                switch (metaKey.ToLower())
                {
                    case "description":
                        this.Description = metaValue;
                        break;
                    case "keywords":
                    case "keyword":
                        base.SetKeywords(metaValue);// Keywords = metaValue;
                        break;
                    case "robots":
                    case "robot":
                        this.SetRobotDirective(metaValue);
                        break;
                    case "icbm":            // <meta name="ICBM" content="50.167958, -97.133185">
                    case "geo.position":    // <meta name="geo.position" content="50.167958;-97.133185">
                        this.SetGpsCoordinates(metaValue);
                        break;
                }
//                ProgressEvent(this, new ProgressEventArgs(4, metaKey + " = " + metaValue));
            }

            string link = String.Empty;

            ArrayList linkLocal = new ArrayList();
            ArrayList linkExternal = new ArrayList();

            // Remove all non 'ignore' comments
            // [v7] fix by brad1213@yahoo.com	
            htmlData = Regex.Replace(htmlData, @"<!--.*?[^" + Preferences.IgnoreRegionTagNoIndex + "]-->", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);


            // http://msdn.microsoft.com/library/en-us/script56/html/js56jsgrpregexpsyntax.asp
            // Original Regex, just found <a href=""> links; and was "broken" by spaces, out-of-order, etc
            // @"(?<=<a\s+href="").*?(?=""\s*/?>)"
            // Looks for the src attribute of:
            // <A> anchor tags
            // <AREA> imagemap links
            // <FRAME> frameset links
            // <IFRAME> floating frames
            // <IMG> for images - new in [v6]
            foreach (Match match in Regex.Matches(htmlData
                , @"(?<anchor><\s*(a|area|frame|iframe|img)\s*(?:(?:\b\w+\b\s*(?:=\s*(?:""[^""]*""|'[^']*'|[^""'<> ]+)\s*)?)*)?\s*>)"
                , RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture))
            {
                // Parse ALL attributes from within tags... IMPORTANT when they're out of order!!
                // in addition to the 'href' attribute, there might also be 'alt', 'class', 'style', 'area', etc...
                // there might also be 'spaces' between the attributes and they may be ", ', or unquoted
                link = String.Empty;
//                ProgressEvent(this, new ProgressEventArgs(4, "Match:" + System.Web.HttpUtility.HtmlEncode(match.Value) + ""));
                foreach (Match submatch in Regex.Matches(match.Value.ToString()
                    , @"(?<name>\b\w+\b)\s*=\s*(""(?<value>[^""]*)""|'(?<value>[^']*)'|(?<value>[^""'<> \s]+)\s*)+"
                    , RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture))
                {
                    // we're only interested in the href attribute (although in future maybe index the 'alt'/'title'?)
//                    ProgressEvent(this, new ProgressEventArgs(4, "Submatch: " + submatch.Groups[1].ToString() + "=" + submatch.Groups[2].ToString() + ""));
                    if ("href" == submatch.Groups[1].ToString().ToLower())
                    {
                        link = submatch.Groups[2].ToString();
                        if (link != "#") break; // break if this isn't just a placeholder href="#", which implies maybe an onclick attribute exists
                    }
                    if ("onclick" == submatch.Groups[1].ToString().ToLower())
                    {   // maybe try to parse some javascript in here

                        string jscript = submatch.Groups[2].ToString();
                        // some code here to extract a filename/link to follow from the onclick="_____"
                        // say it was onclick="window.location='top.htm'"
                        int firstApos = jscript.IndexOf("'");
                        int secondApos = jscript.IndexOf("'", firstApos + 1);
                        if (secondApos > firstApos)
                        {
                            link = jscript.Substring(firstApos + 1, secondApos - firstApos - 1);
                            break;  // break if we found something, ignoring any later href="" which may exist _after_ the onclick in the <a> element
                        }
                    }
                    if ("src" == submatch.Groups[1].ToString().ToLower())
                    {   // [v6] indexes images <img src="???">
                        link = submatch.Groups[2].ToString();
                        break;
                    }
                }
                // [v6] fix by "mike-j-g"
                link = link.ToLower();

                // strip off internal links, so we don't index same page over again
                if (link.IndexOf("#") > -1)
                {   // hash links are intra-page links (eg href="index.html#bottom" )
                    link = link.Substring(0, link.IndexOf("#"));
                }
                if (link.IndexOf("javascript:") == -1
                    && link.IndexOf("mailto:") == -1
                    && !link.StartsWith("#")
                    && link != String.Empty)
                {   // #NOT# javascript, mailto, # or empty
                    if ((link.Length > 8) && (link.StartsWith("http://")
                        || link.StartsWith("https://")
                        || link.StartsWith("file://")
                        || link.StartsWith("//")
                        || link.StartsWith(@"\\")))
                    {
                        linkExternal.Add(link);
//                        ProgressEvent(this, new ProgressEventArgs(4, "External link: " + link));
                    }
                    else if (link.StartsWith("?"))
                    {
                        // it's possible to have /?query which sends the querystring to the
                        // 'default' page in a directory
                        linkLocal.Add(this.Uri.AbsolutePath + link);
//                        ProgressEvent(this, new ProgressEventArgs(4, "? Internal default page link: " + link));
                    }
                    else
                    {
                        linkLocal.Add(link);
//                        ProgressEvent(this, new ProgressEventArgs(4, "I Internal link: " + link));
                    }
                } // add each link to a collection
            } // foreach
            this.LocalLinks = linkLocal;
            this.ExternalLinks = linkExternal;
        } // Parse

        public override bool GetResponse(System.Net.HttpWebResponse webresponse)
        {
            string enc = "utf-8"; // default
            if (webresponse.ContentEncoding != String.Empty)
            {
                // Use the HttpHeader Content-Type in preference to the one set in META
                this.Encoding = webresponse.ContentEncoding;
            }
            else if (this.Encoding == String.Empty)
            {
                // TODO: if still no encoding determined, try to readline the stream until we find either
                // * META Content-Type or * </head> (ie. stop looking for META)
                this.Encoding = enc; // default
            }
            //http://www.c-sharpcorner.com/Code/2003/Dec/ReadingWebPageSources.asp
            System.IO.StreamReader stream = new System.IO.StreamReader
                (webresponse.GetResponseStream(), System.Text.Encoding.GetEncoding(this.Encoding));

            this.Uri = webresponse.ResponseUri; // we *may* have been redirected... and we want the *final* URL
            this.Length = webresponse.ContentLength;
            this.All = stream.ReadToEnd();
            stream.Close();
            return true; //success
        }

        /// <summary>
        /// Stripping HTML
        /// http://www.4guysfromrolla.com/webtech/042501-1.shtml
        /// </summary>
        /// <remarks>
        /// Using regex to find tags without a trailing slash
        /// http://concepts.waetech.com/unclosed_tags/index.cfm
        ///
        /// http://msdn.microsoft.com/library/en-us/script56/html/js56jsgrpregexpsyntax.asp
        ///
        /// Replace html comment tags
        /// http://www.faqts.com/knowledge_base/view.phtml/aid/21761/fid/53
        /// </remarks>
        protected string StripHtml(string Html)
        {
            //Strips the <script> tags from the Html
            string scriptregex = @"<scr" + @"ipt[^>.]*>[\s\S]*?</sc" + @"ript>";
            System.Text.RegularExpressions.Regex scripts = new System.Text.RegularExpressions.Regex(scriptregex, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture);
            string scriptless = scripts.Replace(Html, " ");

            //Strips the <style> tags from the Html
            string styleregex = @"<style[^>.]*>[\s\S]*?</style>";
            System.Text.RegularExpressions.Regex styles = new System.Text.RegularExpressions.Regex(styleregex, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture);
            string styleless = styles.Replace(scriptless, " ");

            //Strips the <NOSEARCH> tags from the Html (where NOSEARCH is set in the web.config/Preferences class)
            //TODO: NOTE: this only applies to INDEXING the text - links are parsed before now, so they aren't "excluded" by the region!! (yet)
            string ignoreless = string.Empty;
            if (Preferences.IgnoreRegions)
            {
                string noSearchStartTag = "<!--" + Preferences.IgnoreRegionTagNoIndex + "-->";
                string noSearchEndTag = "<!--/" + Preferences.IgnoreRegionTagNoIndex + "-->";
                string ignoreregex = noSearchStartTag + @"[\s\S]*?" + noSearchEndTag;
                System.Text.RegularExpressions.Regex ignores = new System.Text.RegularExpressions.Regex(ignoreregex, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture);
                ignoreless = ignores.Replace(styleless, " ");
            }
            else
            {
                ignoreless = styleless;
            }

            //Strips the <!--comment--> tags from the Html	
            //string commentregex = @"<!\-\-.*?\-\->";		// alternate suggestion from antonello franzil 
            string commentregex = @"<!(?:--[\s\S]*?--\s*)?>";
            System.Text.RegularExpressions.Regex comments = new System.Text.RegularExpressions.Regex(commentregex, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture);
            string commentless = comments.Replace(ignoreless, " ");

            //Strips the HTML tags from the Html
            System.Text.RegularExpressions.Regex objRegExp = new System.Text.RegularExpressions.Regex("<(.|\n)+?>", RegexOptions.IgnoreCase);

            //Replace all HTML tag matches with the empty string
            string output = objRegExp.Replace(commentless, " ");

            //Replace all _remaining_ < and > with &lt; and &gt;
            output = output.Replace("<", "&lt;");
            output = output.Replace(">", "&gt;");

            objRegExp = null;
            return output;
        }
    }
}
