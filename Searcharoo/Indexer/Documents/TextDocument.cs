using System;
using System.Collections.Generic;
using System.Text;

namespace Searcharoo.Common
{
    /// <summary>
    /// ASCII Text Document (text/plain)
    /// </summary>
    public class TextDocument : Document
    {
        private string _All;
        private string _WordsOnly;

        public override string WordsOnly
        {
            get { return _WordsOnly; }
        }
        public override string[] WordsArray
        {
            get { return this.WordsStringToArray(WordsOnly); }
        }
        /// <summary>
        /// Set 'all' and 'words only' to the same value (no parsing)
        /// </summary>
        public override string All {
            get { return _All; }
            set { 
                _All = value;
                _WordsOnly = value;
            }
        }

        

        #region Constructor requires Uri
        public TextDocument(Uri location):base(location)
        {
            Extension = "txt";
        }
        #endregion

        public override void Parse()
        {
            // no parsing, by default the content is used "as is"
        }
        public override bool GetResponse(System.Net.HttpWebResponse webresponse)
        {
            //http://www.c-sharpcorner.com/Code/2003/Dec/ReadingWebPageSources.asp
            System.IO.StreamReader stream = new System.IO.StreamReader
                (webresponse.GetResponseStream(), System.Text.Encoding.ASCII);

            this.Uri = webresponse.ResponseUri; // we *may* have been redirected... and we want the *final* URL
            this.Length = webresponse.ContentLength;
            this.All = stream.ReadToEnd();
            this.Title = System.IO.Path.GetFileName(this.Uri.AbsoluteUri);
            this.Description = base.GetDescriptionFromWordsOnly(WordsOnly);
            stream.Close();
            return true; 
        }
    }
}
