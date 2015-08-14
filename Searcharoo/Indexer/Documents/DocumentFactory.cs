using System;
#region Microsoft Office 2007 MimeTypes (for reference)
/*
 * http://www.therightstuff.de/2006/12/16/Office+2007+File+Icons+For+Windows+SharePoint+Services+20+And+SharePoint+Portal+Server+2003.aspx
 * MIME Types for Office 2007 documents
.docm,application/vnd.ms-word.document.macroEnabled.12
.docx,application/vnd.openxmlformats-officedocument.wordprocessingml.document
.dotm,application/vnd.ms-word.template.macroEnabled.12
.dotx,application/vnd.openxmlformats-officedocument.wordprocessingml.template
.potm,application/vnd.ms-powerpoint.template.macroEnabled.12
.potx,application/vnd.openxmlformats-officedocument.presentationml.template
.ppam,application/vnd.ms-powerpoint.addin.macroEnabled.12
.ppsm,application/vnd.ms-powerpoint.slideshow.macroEnabled.12
.ppsx,application/vnd.openxmlformats-officedocument.presentationml.slideshow
.pptm,application/vnd.ms-powerpoint.presentation.macroEnabled.12
.pptx,application/vnd.openxmlformats-officedocument.presentationml.presentation
.xlam,application/vnd.ms-excel.addin.macroEnabled.12
.xlsb,application/vnd.ms-excel.sheet.binary.macroEnabled.12
.xlsm,application/vnd.ms-excel.sheet.macroEnabled.12
.xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet
.xltm,application/vnd.ms-excel.template.macroEnabled.12
.xltx,application
 */
#endregion

namespace Searcharoo.Common
{
    /// <summary>
    /// Return a Document subclass capable of downloading and parsing the
    /// given Uri/ContentType header information
    /// </summary>
    /// <remarks>
    /// [v6] Added reference to JpgDocument and XpsDocument
    /// </remarks>
    public static class DocumentFactory
    {
        /// <summary>
        /// Construct a Document instance 
        /// </summary>
        /// <remarks>
        /// In future, rather than being hardcoded switch statement, this method could
        /// use a 'provider' model where MIME-types and/or extensions are defined
        /// in the .config file, along with the assembly/class to use to process
        /// that type...
        /// </remarks>
        public static Document New (Uri uri, System.Net.HttpWebResponse contentType)
        {
            Document newDoc = new IgnoreDocument(uri);
            string mimeType = ParseMimeType(contentType.ContentType.ToString()).ToLower();
            string encoding = ParseEncoding(contentType.ToString()).ToLower();
            string extension = ParseExtension(uri.AbsoluteUri).ToLower();
            switch (mimeType)
            {
                case "text/css":
                    break;
                case "application/x-msdownload":
                    break;
                case "application/octet-stream":    // ZIP file or something unknown... give some a try
                    switch (extension)
                    { 
                        case ".docx":
                            newDoc = new DocxDocument(uri);
                            break;
                        case ".xlsx":
                            newDoc = new XlsxDocument(uri);
                            break;
                        case ".pptx":
                            newDoc = new PptxDocument(uri);
                            break;
                        case ".pdf":
                            newDoc = new PdfDocument(uri);
                            break;
#if NET35
                        case ".xps"
                            newDoc = new XpsDocument(uri);
                            break;
#endif                   
                    }
                    break;
                                                                                     // docx
                case "application/vnd.ms-word.document.12": 
                case "application/vnd.openxmlformats-officedocument.wordprocessingml":
                case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
                    newDoc = new DocxDocument(uri);
                    break;
                                                                                    // pptx
                case "application/vnd.openxmlformats-officedocument.presentationml":
                case "application/vnd.openxmlformats-officedocument.presentationml.presentation":
                    newDoc = new PptxDocument(uri);
                    break;
                                                                                    // xlsx
                case "application/vnd.openxmlformats-officedocument.spreadsheetml":
                case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                    newDoc = new XlsxDocument(uri);
                    break;
                case "application/pdf":                                             // pdf; changed from FilterDocument in v7
                    newDoc = new PdfDocument(uri);
                    break;
                case "application/vnd.ms-powerpoint":                               // ppt
                case "application/msword":                                          // xls
                    newDoc = new FilterDocument(uri);
                    break;
                case "text/plain":
                    newDoc = new TextDocument(uri);
                    break;
                case "text/xml":
                case "application/xml":
                    newDoc = new HtmlDocument(uri); // TODO: XmlDocument parser
                    break;
                case "application/rss+xml":
                case "application/rdf+xml":
                case "application/atom+xml":
                    newDoc = new HtmlDocument(uri); // TODO: RssDocument parser
                    break;
                case "application/xhtml+xml":
                    newDoc = new HtmlDocument(uri); // TODO: XhtmlDocument parser
                    break;
                case "text/html":
                    newDoc = new HtmlDocument(uri); // [v6] clarify code, suggested by "MADCookie2"
                    break;
                case "image/jpeg":
                    newDoc = new JpegDocument(uri); // [v6] now parse image EXIF data
                    break;
                default:
                    // none of the above matched...
                    if (mimeType.IndexOf("html") >= 0)
                    {   // If we got 'text' data (not images)
                        newDoc = new HtmlDocument(uri);
                    }
                    else if (mimeType.IndexOf("text") >= 0)
                    {   // If we got 'text' data (not images)
                        newDoc = new TextDocument(uri);
                    }
                    break;
            } // switch; if not set, defaults to IgnoreDocument
            newDoc.MimeType = mimeType;
            
            return newDoc;
        }

        #region Private Methods: ParseExtension, ParseMimeType, ParseEncoding
        private static string ParseExtension(string filename)
        {
            return System.IO.Path.GetExtension(filename).ToLower();
        }

        private static string ParseMimeType(string contentType)
        {
            string mimeType = string.Empty;
            string[] contentTypeArray = contentType.Split(';');
            // Set MimeType if it's blank
            if (mimeType == String.Empty && contentTypeArray.Length >= 1)
            {
                mimeType = contentTypeArray[0];
            }
            return mimeType;
        }

        private static string ParseEncoding(string contentType)
        {
            string encoding = string.Empty;
            string[] contentTypeArray = contentType.Split(';');
            // Set Encoding if it's blank
            if (encoding == String.Empty && contentTypeArray.Length >= 2)
            {
                int charsetpos = contentTypeArray[1].IndexOf("charset");
                if (charsetpos > 0)
                {
                    encoding = contentTypeArray[1].Substring(charsetpos + 8, contentTypeArray[1].Length - charsetpos - 8);
                }
            }
            return encoding;
        }
        #endregion
    }
}