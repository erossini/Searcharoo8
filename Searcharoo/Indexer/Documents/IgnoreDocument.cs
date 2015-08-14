using System;
using System.Collections.Generic;
using System.Text;

namespace Searcharoo.Common
{
    /// <summary>
    /// Document instance when the Uri is to be ignored (ie not indexable).
    /// </summary>
    /// <remarks>
    /// Created this in case we still want to use the Document class, say 
    /// to get the filesize for reporting or something...
    /// </remarks>
    public class IgnoreDocument : Document
    {
        #region Constructor requires Uri
        public IgnoreDocument(Uri location) : base(location)
        {
            //_Uri = location;
        }
        #endregion

        public override string WordsOnly
        {
            get { return string.Empty; }
        }
        public override void Parse ()
        {
            // no parsing, by default the content is used "as is"
        }
        public override bool GetResponse (System.Net.HttpWebResponse webresponse)
        {
            return false;
        }
    }
}
