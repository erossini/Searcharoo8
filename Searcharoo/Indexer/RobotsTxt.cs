using System;
using System.Collections;
using System.Text;
using Searcharoo.Common; // Preferences (for Proxy)

namespace Searcharoo.Indexer
{
    /// <summary>
    /// Represents the rules for a specific domain for a specific host 
    /// (ie it aggregates all the rules that match the UserAgent, plus the special * rules)
    /// 
    /// http://www.robotstxt.org/
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    public class RobotsTxt
    {
        #region Private Fields: _FileContents, _UserAgent, _Server, _DenyUrls, _LogString
        private string _FileContents;
        private string _UserAgent;
        private string _Server;
        /// <summary>lowercase string array of url fragments that are 'denied' to the UserAgent for this RobotsTxt instance</summary>
        private ArrayList _DenyUrls = new ArrayList();
        private string _LogString = string.Empty;
        #endregion

        #region Constructors: require starting Url and UserAgent to create an object
        private RobotsTxt()
        { }

        public RobotsTxt(Uri startPageUri, string userAgent)
        {
            _UserAgent = userAgent;
            _Server = startPageUri.Host;

            try
            {
                System.Net.WebProxy proxyObject = null;
                if (Preferences.UseProxy)
                {   // [v6] stephenlane80 suggested proxy code
                    proxyObject = new System.Net.WebProxy(Preferences.ProxyUrl, true);
                    proxyObject.Credentials = System.Net.CredentialCache.DefaultCredentials;
                }
                System.Net.HttpWebRequest req = (System.Net.HttpWebRequest)System.Net.WebRequest.Create("http://" + startPageUri.Authority + "/robots.txt");
                if (Preferences.UseProxy) req.Proxy = proxyObject; // [v6] stephenlane80

                System.Net.HttpWebResponse webresponse = (System.Net.HttpWebResponse)req.GetResponse();

                if (webresponse.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine("ROBOTS.TXT request returned HttpStatus " + webresponse.StatusCode.ToString());
                    _FileContents = String.Empty;
                    return;
                }

                using (System.IO.StreamReader stream = new System.IO.StreamReader(webresponse.GetResponseStream(), Encoding.ASCII))
                {
                    _FileContents = stream.ReadToEnd();
                } // stream.Close();

                //ProgressEvent(this, new ProgressEventArgs(1, "robots.txt file loaded from " + server + "robots.txt"));

                // [v6] fix by maaguirr (Matt) to read Unix-based ROBOTS.TXT files
                string[] fileLines = _FileContents.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                bool rulesApply = false;
                foreach (string line in fileLines)
                {
                    if (line.Trim() != "")
                    {
                        RobotInstruction ri = new RobotInstruction(line);
                        switch (ri.Instruction[0])
                        {
                            case '#':   //then comment - ignore
                                break;
                            case 'u':   // User-Agent
                                if ((ri.UrlOrAgent.IndexOf("*") >= 0)
                                  || (ri.UrlOrAgent.IndexOf(_UserAgent) >= 0))
                                { // these rules apply
                                    rulesApply = true;
                                    Console.WriteLine(ri.UrlOrAgent + " " + rulesApply);
                                }
                                else
                                {
                                    rulesApply = false;
                                }
                                break;
                            case 'd':   // Disallow
                                if (rulesApply)
                                {
                                    _DenyUrls.Add(ri.UrlOrAgent.ToLower());
                                    Console.WriteLine("D " + ri.UrlOrAgent);
                                }
                                else
                                {
                                    Console.WriteLine("D " + ri.UrlOrAgent + " is for another user-agent");
                                }
                                break;
                            case 'a':   // Allow
                                Console.WriteLine("A" + ri.UrlOrAgent);
                                break;
                            default:
                                // empty/unknown/error
                                Console.WriteLine("# Unrecognised robots.txt entry [" + line + "]");
                                break;
                        }
                    }
                }
            }
            catch (System.Net.WebException)
            {
                _FileContents = String.Empty;
                //ProgressEvent(this, new ProgressEventArgs(1, "No robots.txt file found at " + server));
            }
            catch (System.Security.SecurityException)
            {
                _FileContents = String.Empty;
                //ProgressEvent(this, new ProgressEventArgs(1, "Could not load ROBOTS.TXT file from " + server));
            }
        }
        #endregion

        #region Methods: Allow
        /// <summary>
        /// Does the parsed robots.txt file allow this Uri to be spidered for this user-agent?
        /// </summary>
        /// <remarks>
        /// This method does all its "matching" in lowercase - it expects the _DenyUrl 
        /// elements to be ToLower() and it calls ToLower on the passed-in Uri...
        /// </remarks>
        public bool Allowed (Uri uri)
        {
            if (_DenyUrls.Count == 0) return true;

            string url = uri.AbsolutePath.ToLower();
            foreach (string denyUrlFragment in _DenyUrls)
            {
                if (url.Length >= denyUrlFragment.Length)
                {
                    if (url.Substring(0, denyUrlFragment.Length) == denyUrlFragment)
                    {
                        return false;
                    } // else not a match
                } // else url is shorter than fragment, therefore cannot be a 'match'
            }
            if (url == "/robots.txt") return false;
            // no disallows were found, so allow
            return true;
        }
        #endregion

        #region Private class: RobotInstruction
        /// <summary>
        /// Use this class to read/parse the robots.txt file
        /// </summary>
        /// <remarks>
        /// Types of data coming into this class
        /// User-agent: * ==> _Instruction='User-agent', _Url='*'
        /// Disallow: /cgi-bin/ ==> _Instruction='Disallow', _Url='/cgi-bin/'
        /// Disallow: /tmp/ ==> _Instruction='Disallow', _Url='/tmp/'
        /// Disallow: /~joe/ ==> _Instruction='Disallow', _Url='/~joe/'
        /// </remarks>
        private class RobotInstruction
        {
            private string _Instruction;
            private string _Url = string.Empty;

            /// <summary>
            /// Constructor requires a line, hopefully in the format [instuction]:[url]
            /// </summary>
            public RobotInstruction (string line) 
            {
                string instructionLine = line;
                int commentPosition = instructionLine.IndexOf('#');
                if (commentPosition == 0)
                {
                    _Instruction = "#";
                }
                if (commentPosition >= 0)
                {   // comment somewhere on the line, trim it off
                    instructionLine = instructionLine.Substring(0, commentPosition);
                }
                if (instructionLine.Length > 0)
                {   // wasn't just a comment line (which should have been filtered out before this anyway
                    string[] lineArray = instructionLine.Split(':');
                    _Instruction = lineArray[0].Trim().ToLower();
                    if (lineArray.Length > 1)
                    {
                        _Url = lineArray[1].Trim();
                    }
                }
            }
            /// <summary>
            /// Lower-case part of robots.txt line, before the colon (:)
            /// </summary>
            public string Instruction
            {
                get { return _Instruction; }
            }
            /// <summary>
            /// Lower-case part of robots.txt line, after the colon (:)
            /// </summary>
            public string UrlOrAgent
            {
                get { return _Url; }
            }
        }
        #endregion
    }
}
