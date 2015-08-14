#if NET35
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Documents;
using System.Windows.Xps.Packaging;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Searcharoo.Common
{
    /// <summary>
    /// Load a Microsoft XPS document - REQUIRES .NET 3.x
    /// </summary>
    /// <remarks>
    /// A compiler flag "NET35" must be set before this class can be used
    /// </remarks>
    public class XpsDocument : DownloadDocument
    {
        private string _WordsOnly;

        public XpsDocument(Uri location)
            : base(location)
        { 
            Extension = "xps";
        }

        public override void Parse()
        {
            // no parsing (for now). perhaps in future we can regex look for urls (www.xxx.com) and try to link to them...
        }

        public override string WordsOnly
        {
            get { return _WordsOnly; }
        }

        /// <remarks>
        /// .NET System.IO.Compression and zip files
        /// http://blogs.msdn.com/dotnetinterop/archive/2006/04/05/.NET-System.IO.Compression-and-zip-files.aspx
        /// </remarks>
        public override bool GetResponse(System.Net.HttpWebResponse webresponse)
        {
            string filename = System.IO.Path.Combine(
                          Preferences.DownloadedTempFilePath
                        , (System.IO.Path.GetFileName(this.Uri.LocalPath)));
            this.Title = System.IO.Path.GetFileNameWithoutExtension(filename);

            SaveDownloadedFile(webresponse, filename);
            try
            {
                XpsDocument xpsDoc = new XpsDocument(filename, System.IO.FileAccess.Read);
                FixedDocumentSequence docSeq = xpsDoc.GetFixedDocumentSequence();
                for (int pageNum = 0; pageNum < docSeq.DocumentPaginator.PageCount; pageNum++)
                {
                    DocumentPage docPage = docSeq.DocumentPaginator.GetPage(pageNum);

                    foreach (System.Windows.UIElement uie in ((FixedPage)docPage.Visual).Children)
                    {
                        if (uie is System.Windows.Documents.Glyphs)
                        {
                            _WordsOnly += " " + ((System.Windows.Documents.Glyphs)uie).UnicodeString;
                        }
                    }
                }
                this.All = _WordsOnly;

                System.IO.File.Delete(filename);    // clean up
            }
            catch (Exception ex2)
            {
                //                ProgressEvent(this, new ProgressEventArgs(2, "IFilter failed on " + this.Uri + " " + e.Message + ""));
            }
            if (this.All != string.Empty)
            {
                this.Description = base.GetDescriptionFromWordsOnly(WordsOnly);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
#endif