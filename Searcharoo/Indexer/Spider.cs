using System;
using System.Xml.Serialization;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using Searcharoo.Common;
using Searcharoo.Common.Extensions;

namespace Searcharoo.Indexer
{
    /// <summary>
    /// The Spider that crawls your website, link by link.
    /// </summary>
    /// <remarks>
    /// In the Searcharoo (v2), this code was 'embedded' in an ASPX page. 
    /// This was for ease of reporting 'progress' via Response.Write
    /// statements. The code now uses an EventHandler to trigger progress reporting
    /// by the calling code - so now it could be Reponse.Write, or saved to a file 
    /// or any other mechanism. (v4) takes advantage of this by wrapping the Spider in a console
    /// application so you can run it outside of a website.
    /// 
    /// Some of the references used when researching this code:
    ///
    /// C# and the Web: Writing a Web Client Application with Managed Code in the Microsoft .NET Framework - not helpful...
    /// http://msdn.microsoft.com/msdnmag/issues/01/09/cweb/default.aspx
    ///
    /// Retrieving a List of Links & Images from a Web Page
    /// http://www.dotnetjunkies.com/Tutorial/1B219C93-7702-4ADF-9106-DFFDF90914CF.dcik
    /// 
    /// FUTURE: In case connecting via a Proxy is required for the spidering
    /// http://www.experts-exchange.com/Programming/Programming_Languages/Dot_Net/Q_20974147.html
    /// http://msdn.microsoft.com/library/en-us/cpref/html/frlrfsystemnetglobalproxyselectionclasstopic.asp
    /// </remarks>
    public class Spider
    {
        #region Private fields: visited, count, catalog, 
        private Uri _CurrentStartUri = null;
        private string _CurrentStartUriString = String.Empty;
        /// <summary></summary>
        private ArrayList _Visited = new ArrayList();
        /// <summary></summary>
        private Hashtable _VisitedHashtable = new Hashtable();
        /// <summary></summary>
        //private int _Count = 0;
        /// <summary></summary>
        private Catalog _Catalog;
        //private Cache _Cache;
        
        /// <summary>Stemmer to use</summary>
        private IStemming _Stemmer;

        /// <summary>Stemmer to use</summary>
        private IStopper _Stopper;

        /// <summary>Go word parser to use</summary>
        private IGoWord _GoChecker;

        /// <summary>Loads and acts as 'authorisation' for robot-excluded Urls</summary>
        private RobotsTxt _Robot;

        /// <summary>SIMONJONES</summary>
        System.Net.CookieContainer _CookieContainer = new System.Net.CookieContainer();
        #endregion

        #region Public events/handlers: SpiderProgressEvent
        /// <summary>
        /// Event Handler to communicate progress and errors back to the calling code
        /// </summary>
        /// <remarks>
        /// Learn about Events from a few different places
        /// http://www.codeproject.com/csharp/csevents01.asp
        /// http://www.csharphelp.com/archives/archive253.html
        /// http://www.devhood.com/Tutorials/tutorial_details.aspx?tutorial_id=380
        /// </remarks>
        public event SpiderProgressEventHandler SpiderProgressEvent;

        /// <summary>
        /// Only trigger the event if a Handler has been attached.
        /// </summary>
        private void ProgressEvent(object sender, ProgressEventArgs pea)
        {
            if (this.SpiderProgressEvent != null)
            {
                SpiderProgressEvent(sender, pea);
            }
        }
        #endregion

        /// <summary>
        /// Takes a single Uri (Url) and returns the catalog that is generated
        /// by following all the links from that point.
        /// </summary>
        /// <remarks>
        ///This is the MAIN method of the indexing system.
        /// </remarks>
        public Catalog BuildCatalog (Uri startPageUri)
        {
            return BuildCatalog(new Uri[]{startPageUri});
            /*
            _Catalog = new Catalog();
            
            _CurrentStartUri = startPageUri;    // to compare against fully qualified links
            _CurrentStartUriString = _CurrentStartUri.AbsoluteUri.ToString().ToLower();
            ProgressEvent(this, new ProgressEventArgs(1, "Spider.Catalog (single Uri) " + startPageUri.AbsoluteUri));
            // Setup Stop, Go, Stemming
            SetPreferences();

            _Robot = new RobotsTxt(startPageUri, Preferences.RobotUserAgent);

            // GETS THE FIRST DOCUMENT, AND STARTS THE SPIDER! -- create the 'root' document to start the search
            // HtmlDocument htmldoc = new HtmlDocument(startPageUri);
            // RECURSIVE CALL TO 'Process()' STARTS HERE
            ProcessUri(startPageUri, 0);

            // Now we've FINISHED Spidering
            ProgressEvent(this, new ProgressEventArgs(1, "Spider.Catalog() complete."));
            ProgressEvent(this, new ProgressEventArgs(2, "Serializing to disk location " + Preferences.CatalogFileName));

            // Serialization of the Catalog, so we can load it again if the server Application is restarted
            _Catalog.Save();
           
            ProgressEvent(this, new ProgressEventArgs(3, "Save to disk " + Preferences.CatalogFileName + " successful"));

            return _Catalog;// finished, return to the calling code to 'use'
             */
        }

        /// <summary>
        /// [v6]
        /// </summary>
        /// <param name="startPageUri">array of start pages</param>
        /// <returns>Catalog of words/documents</returns>
        public Catalog BuildCatalog(Uri[] startPageUris)
        {
            _Catalog = new Catalog(); //_Cache = new Cache(); // [v7]
            ProgressEvent(this, new ProgressEventArgs(1, "Spider.Catalog (Uri Array) count: " + startPageUris.Length.ToString()));
            // Setup Stop, Go, Stemming
            SetPreferences();

            foreach (Uri startPageUri in startPageUris)
            {
                _CurrentStartUri = startPageUri;    // to compare against fully qualified links
                _CurrentStartUriString = _CurrentStartUri.AbsoluteUri.ToString().ToLower();

                ProgressEvent(this, new ProgressEventArgs(1, "Spider.Catalog (start Uri) " + startPageUri.AbsoluteUri));

                _Robot = new RobotsTxt(startPageUri, Preferences.RobotUserAgent);

                // GETS THE FIRST DOCUMENT, AND STARTS THE SPIDER! -- create the 'root' document to start the search
                // HtmlDocument htmldoc = new HtmlDocument(startPageUri);
                // RECURSIVE CALL TO 'Process()' STARTS HERE
                ProcessUri(startPageUri, 0);

                ProgressEvent(this, new ProgressEventArgs(1, "Spider.Catalog (end Uri) " + startPageUri.AbsoluteUri));
            }
            // Now we've FINISHED Spidering
            ProgressEvent(this, new ProgressEventArgs(1, "Spider.Catalog() complete."));
            ProgressEvent(this, new ProgressEventArgs(2, "Serializing to disk location " + Preferences.CatalogFileName));

            // Serialization of the Catalog, so we can load it again if the server Application is restarted
            _Catalog.Save();
            //_Cache.Save(); //[v7]

            ProgressEvent(this, new ProgressEventArgs(3, "Save to disk " + Preferences.CatalogFileName + " successful"));

            return _Catalog;// finished, return to the calling code to 'use'
        }

        /// <summary>
        /// Setup Stop, Go, Stemming
        /// </summary>
        private void SetPreferences()
        {
            switch (Preferences.StemmingMode)
            {
                case 1:
                    ProgressEvent(this, new ProgressEventArgs(1, "Stemming enabled."));
                    _Stemmer = new PorterStemmer();	//Stemmer = new SnowStemming();
                    break;
                case 2:
                    ProgressEvent(this, new ProgressEventArgs(1, "Stemming enabled."));
                    _Stemmer = new PorterStemmer();
                    break;
                default:
                    ProgressEvent(this, new ProgressEventArgs(1, "Stemming DISabled."));
                    _Stemmer = new NoStemming();
                    break;
            }
            switch (Preferences.StoppingMode)
            {
                case 1:
                    ProgressEvent(this, new ProgressEventArgs(1, "Stop words shorter than 3 chars."));
                    _Stopper = new ShortStopper();
                    break;
                case 2:
                    ProgressEvent(this, new ProgressEventArgs(1, "Stop words from list."));
                    _Stopper = new ListStopper();
                    break;
                default:
                    ProgressEvent(this, new ProgressEventArgs(1, "Stopping DISabled."));
                    _Stopper = new NoStopping();
                    break;
            }
            switch (Preferences.GoWordMode)
            {
                case 1:
                    ProgressEvent(this, new ProgressEventArgs(1, "Go Words enabled."));
                    _GoChecker = new ListGoWord();
                    break;
                default:
                    ProgressEvent(this, new ProgressEventArgs(1, "Go Words DISabled."));
                    _GoChecker = new NoGoWord();
                    break;
            }
        }


        /// <summary>
        /// Recursive 'process' method: takes the uri input, downloads it (following redirects if required)
        /// receiving a Document subclass, then calling the Parse() method to get the words which
        /// are then added to the Catalog.
        /// </summary>
        protected int ProcessUri(Uri uri, int level)
        {
            // [j105 Rob] recursion fix 
            // http://www.codeproject.com/aspnet/Spideroo.asp?df=100&forumid=71481&select=1862807#xx1862807xx
            if (level > Preferences.RecursionLimit) return Preferences.RecursionLimit;

            int wordcount = 0;
            string url = uri.AbsoluteUri.ToLower(); // [v6]

            if (!_Robot.Allowed(uri))
            {
                ProgressEvent(this, new ProgressEventArgs(2, "RobotsTxt exclusion prevented indexing of " + url + ""));
            }
            else
            {
                bool alreadyVisited = _Visited.Contains(url);

                if (!alreadyVisited && Preferences.UseDefaultDocument)
                {   // [v7] First-attempt at treating 'folder' Urls (eg mysite.com/Photos) and default documents (eg mysite.com/Photos/Default.aspx)
                    // as the SAME PAGE to prevent duplicates in the search results. To do this, when we find a Url that looks like a 'folder'
                    // (eg. no file extension OR ends with a / slash) we add all three 'variations' of that Url to the _Visited list so the other
                    // variations aren't even retrieved/indexed.
                    string defaultDoc = Preferences.DefaultDocument;
                    int defaultDocLength = defaultDoc.Length;
                    int defaultDocLengthPlusSlash = defaultDoc.Length;

                    if (url.LastIndexOf("/") == (url.Length - 1))
                    {   // Variation #1: ends in slash /
                        alreadyVisited = _Visited.Contains(url + defaultDoc) || _Visited.Contains(url.Trim('/'));
                        _Visited.Add(url + defaultDoc);
                        _Visited.Add(url.Trim('/'));
                    }
                    else if (System.IO.Path.GetExtension(url) == "")
                    {   // Variation #2: no file extension
                        alreadyVisited = _Visited.Contains(url + "/" + defaultDoc) || _Visited.Contains(url + "/");
                        _Visited.Add(url + "/" + defaultDoc);
                        _Visited.Add(url + "/");
                    }
                    else if (url.LastIndexOf(defaultDoc) == (url.Length - defaultDocLength))
                    {   // Variation #3: ends in /default.aspx (or whatever the specified default document is: index.html, default.htm, etc)
                        alreadyVisited = _Visited.Contains(url.Substring(0, (url.Length - defaultDocLengthPlusSlash))) 
                                      || _Visited.Contains(url.Substring(0, (url.Length - defaultDocLength)));
                        _Visited.Add(url.Substring(0, (url.Length - defaultDocLengthPlusSlash)));
                        _Visited.Add(url.Substring(0, (url.Length - defaultDocLength)));
                    }
                }
                if (alreadyVisited)
                {
                    ProgressEvent(this, new ProgressEventArgs(2, url + " already spidered"));
                }
                else
                {
                    _Visited.Add(url); 
                    ProgressEvent(this, new ProgressEventArgs(2, url + " being downloaded"));
                    // ### IMPORTANT ### 
                    // Uri is actually retrieved here!
                    Document downloadDocument = Download(uri);

                    if (null == downloadDocument)
                    {
                        ProgressEvent(this, new ProgressEventArgs(1, "Download() failed on " + url + ""));
                    }
                    else
                    {
                        // ### IMPORTANT ### 
                        // Uri downloaded content is actually parsed here!
                        downloadDocument.Parse();
                        if (downloadDocument.RobotIndexOK)
                        {
                            wordcount = AddToCatalog (downloadDocument);
                        }
                        else
                        {
                            ProgressEvent(this, new ProgressEventArgs(2, "RobotMeta exclusion prevented indexing of " + url + ""));
                        }
                    }

                    if (wordcount > 0)
                    {
                        ProgressEvent(this, new ProgressEventArgs(1, downloadDocument.Title + " parsed " + wordcount + " words!"));
                        ProgressEvent(this, new ProgressEventArgs(4, downloadDocument.Title + " " + downloadDocument.Uri.AbsoluteUri + System.Environment.NewLine
                                                                    + (downloadDocument.RobotIndexOK ? "Indexed" : "RobotMeta Excluded Index")
                                                                    + downloadDocument.Description));
                    }
                    else
                    {
                        ProgressEvent(this, new ProgressEventArgs(2, url + " parsed but zero words found."));
                    }
                    // [v7] bugfix
                    if (null == downloadDocument)
                    { 
                        // why is it null here?
                        System.Diagnostics.Debug.WriteLine(url + " resulted in a null downloadDocument");
                    }
                    else
                    {
                        // Move some 'External' to 'Local' links
                        ArrayList elinks = (ArrayList)downloadDocument.ExternalLinks.Clone();
                        for (int l = 0; l < elinks.Count; l++)
                        {
                            string link = elinks[l].ToString();
                            Uri linkUri = new Uri(link);
                                                    //if (link.ToLower().StartsWith(this._CurrentStartUriString))
                            if (_CurrentStartUri.IsBaseOf(linkUri))
                            {   // if this link is actually 'under' the starting one, treat it as internal (even 
                                // though it started with http:
                                downloadDocument.ExternalLinks.Remove(link);
                                downloadDocument.LocalLinks.Add(link);
                            }
                        }

                        // ### Loop through the 'local' links in the document ###
                        // ### and parse each of them recursively ###
                        if (null != downloadDocument && null != downloadDocument.LocalLinks && downloadDocument.RobotFollowOK)
                        { // only if the Robot meta says it's OK
                            foreach (object link in downloadDocument.LocalLinks)
                            {
                                try
                                {
                                    Uri urlToFollow = new Uri(downloadDocument.Uri, link.ToString());
                                    ProcessUri(urlToFollow, level + 1); // calls THIS method, recursively
                                }
                                catch (Exception ex)
                                {
                                    ProgressEvent(this, new ProgressEventArgs(2, "new Uri(" + downloadDocument.Uri + ", " + link.ToString() + ") invalid : " + ex.Message + ""));
                                }
                            }
                        } // process local links
                    } // document was not null
                } // not visited
            } // robot allowed
            return level;
        }

        /// <summary>
        /// Attempts to download the Uri and (based on it's MimeType) use the DocumentFactory
        /// to get a Document subclass object that is able to parse the downloaded data.
        /// </summary>
        /// <remarks>
        /// http://www.123aspx.com/redir.aspx?res=28320
        /// </remarks>
        protected Document Download (Uri uri)
        {
            bool success = false;
            // Open the requested URL

            System.Net.WebProxy proxyObject = null;
            if (Preferences.UseProxy)
            {   // [v6] stephenlane80 suggested proxy code
                proxyObject = new System.Net.WebProxy(Preferences.ProxyUrl, true);
                proxyObject.Credentials = System.Net.CredentialCache.DefaultCredentials;
            }
            // [v6] Erick Brown [work] suggested fix for & in querystring
            string unescapedUri = Regex.Replace(uri.AbsoluteUri, @"&amp;amp;", @"&", RegexOptions.IgnoreCase);
            System.Net.HttpWebRequest req = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(unescapedUri); 

            req.AllowAutoRedirect = true;
            req.MaximumAutomaticRedirections = 3;
            req.UserAgent = Preferences.UserAgent; //"Mozilla/6.0 (MSIE 6.0; Windows NT 5.1; Searcharoo.NET; robot)";
            req.KeepAlive = true;
            req.Timeout = Preferences.RequestTimeout * 1000; //prefRequestTimeout 
            if (Preferences.UseProxy) req.Proxy = proxyObject; // [v6] stephenlane80

            // SIMONJONES http://codeproject.com/aspnet/spideroo.asp?msg=1421158#xx1421158xx
            req.CookieContainer = new System.Net.CookieContainer();
            req.CookieContainer.Add(_CookieContainer.GetCookies(uri));

            // Get the stream from the returned web response
            System.Net.HttpWebResponse webresponse = null;
            try
            {
                webresponse = (System.Net.HttpWebResponse)req.GetResponse();
            }
            catch (System.Net.WebException we)
            {   //remote url not found, 404; remote url forbidden, 403
                ProgressEvent(this, new ProgressEventArgs(2, "skipped  " + uri.AbsoluteUri + " response exception:" + we.ToString() + ""));
            }

            Document currentUriDocument = null;
            if (webresponse != null)
            {
                /* SIMONJONES */
                /* **************** this doesn't necessarily work yet...
                if (webresponse.ResponseUri != htmldoc.Uri)
                {	// we've been redirected, 
                    if (visited.Contains(webresponse.ResponseUri.ToString().ToLower()))
                    {
                        return true;
                    }
                    else
                    {
                        visited.Add(webresponse.ResponseUri.ToString().ToLower());
                    }
                }*/

                try
                {
                    webresponse.Cookies = req.CookieContainer.GetCookies(req.RequestUri);
                    // handle cookies (need to do this in case we have any session cookies)
                    foreach (System.Net.Cookie retCookie in webresponse.Cookies)
                    {
                        bool cookieFound = false;
                        foreach (System.Net.Cookie oldCookie in _CookieContainer.GetCookies(uri))
                        {
                            if (retCookie.Name.Equals(oldCookie.Name))
                            {
                                oldCookie.Value = retCookie.Value;
                                cookieFound = true;
                            }
                        }
                        if (!cookieFound)
                        {
                            _CookieContainer.Add(retCookie);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ProgressEvent(this, new ProgressEventArgs(3, "Cookie processing error : " + ex.Message + ""));
                }
                /* end SIMONJONES */
                
                currentUriDocument = DocumentFactory.New(uri, webresponse);
                success = currentUriDocument.GetResponse(webresponse);
                webresponse.Close();
                ProgressEvent(this, new ProgressEventArgs(2, "Trying index mime type: " + currentUriDocument.MimeType + " for " + currentUriDocument.Uri + ""));
                
                _Visited.Add(currentUriDocument.Uri);   // [v7] brad1213@yahoo.com capture redirected Urls
                                                        // relies on Document 'capturing' the final Uri
                                                        // this.Uri = webresponse.ResponseUri;
            }
            else
            {
                ProgressEvent(this, new ProgressEventArgs(2, "No WebResponse for " + uri + ""));
                success = false;
            }
            return currentUriDocument;
        }

        /// <summary>
        /// Add the Document subclass to the catalog, BY FIRST 'copying' the main
        /// properties into a File class. The distinction is a bit arbitrary: Documents
        /// are downloaded and indexed, but their content is modelled in as a File
        /// class in the Catalog (and represented as a ResultFile object in the search ASPX page)
        /// </summary>
        /// <return>Number of words catalogued in the Document</return>
        protected int AddToCatalog(Document downloadDocument)
        {
            File infile = new File(downloadDocument.Uri.AbsoluteUri
                , downloadDocument.Title.UnicodeToCharacter()
                , downloadDocument.Description.UnicodeToCharacter()
                , DateTime.Now
                , downloadDocument.Length
                , downloadDocument.GpsLocation
                , downloadDocument.Extension
                , downloadDocument.KeywordString.UnicodeToCharacter());

            // ### Loop through words in the file ###
            int i = 0, j = 0;   // count of words, count of words _indexed
            string key = "";    // temp variables
            
            foreach (string word in downloadDocument.WordsArray)
            {
                key = word.UnicodeToCharacter().ToLower();
                if (!_GoChecker.IsGoWord(key))
                {	// not a special case, parse like any other word
                    RemovePunctuation(ref key);

                    if (!IsNumber(ref key))
                    {	// not a number, so get rid of numeric seperators and catalog as a word
                        // TODO: remove inline punctuation, split hyphenated words?
                        // http://blogs.msdn.com/ericgu/archive/2006/01/16/513645.aspx
                        key = System.Text.RegularExpressions.Regex.Replace(key, "[,.]", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                        // Apply Stemmer (set by preferences)
                        key = _Stemmer.StemWord(key);

                        // Apply Stopper (set by preferences)
                        key = _Stopper.StopWord(key);
                    }
                }
                else
                {
                    ProgressEvent(this, new ProgressEventArgs(4, "Found GoWord " + key + " in " + downloadDocument.Title));
                }
                if (key != String.Empty)
                {
                    _Catalog.Add(key, infile, i);
                    j++;
                }
                i++;
            }
            _Catalog.FileCache.Add(downloadDocument.WordsArray, infile);
            return i;
        }

        /// <summary>
        /// Each word is identified purely by the whitespace around it. It could still include punctuation
        /// attached to either end of the word, or "in" the word (ie a dash, which we will remove for
        /// indexing purposes)
        /// </summary>
        /// <remarks>
        /// Andrey Shchekin suggests 'unicode' regex [\w] - equivalent to [\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Pc}]
        /// http://www.codeproject.com/cs/internet/Searcharoo_4.asp?df=100&forumid=397394&select=1992575#xx1992575xx
        /// so [^\w0-9,.] as a replacement for [^a-z0-9,.]
        /// which might remove the need for 'AssumeAllWordsAreEnglish'. TO BE TESTED.
        /// </remarks>
        private void RemovePunctuation(ref string word)
        {   // this stuff is a bit 'English-language-centric'
            if (Preferences.AssumeAllWordsAreEnglish)
            {   // if all words are english, this strict parse to remove all punctuation ensures
                // words are reduced to their least unique form before indexing
                //word = System.Text.RegularExpressions.Regex.Replace(word, @"[^a-z0-9,.]", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                
                // [v6] testing better i18n
                word = System.Text.RegularExpressions.Regex.Replace(word, @"[^\w0-9,.]", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
            else 
            {   // by stripping out this specific list of punctuation only, there is potential to leave lots 
                // of cruft in the word before indexing BUT this will allow any language to be indexed
                word = word.Trim(' ','?','\"',',','\'',';',':','.','(',')','[',']','%','*','$','-'); 
            }
        }
        /// <summary>
        /// TODO: parse numbers here 
        /// ie remove thousands seperator, currency, etc
        /// and also trim decimal part, so number searches are only on the integer value
        /// </summary>
        private bool IsNumber(ref string word)
        {
            try
            {
                long number = Convert.ToInt64(word); //;int.Parse(word);
                word = number.ToString();
                return (word != String.Empty);
            }
            catch
            {
                return false;
            }
        }
    }
}