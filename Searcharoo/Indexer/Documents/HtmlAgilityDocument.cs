using System;
using System.Collections.Generic;
using System.Text;

namespace Searcharoo.Common
{
    public class HtmlAgilityDocument : HtmlDocument
    {
        public HtmlAgilityDocument(Uri location) : base(location) { }
    }
}
