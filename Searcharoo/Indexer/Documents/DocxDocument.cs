using System;
using System.IO;
using System.Xml;
using ionic.utils.zip;

namespace Searcharoo.Common
{
    /// <summary>
    /// Load a Microsoft Word 2007 Xml file format
    /// </summary>
    /// <remarks>
    /// SharpZipLib
    /// http://www.icsharpcode.net/OpenSource/SharpZipLib/
    /// 
    /// unzip -p some.docx word/document.xml | perl -pe 's/<[^>]+>|[^[:print:]]+//g'
    /// 
    /// Building WordProcessingML Document...
    /// http://blogs.msdn.com/dmahugh/archive/2006/06/27/649007.aspx
    /// http://openxmldeveloper.org/articles/DocxClassFormattedText.aspx
    /// 
    /// .NET 2.0
    /// http://blogs.msdn.com/dotnetinterop/archive/2006/04/05/.NET-System.IO.Compression-and-zip-files.aspx
    /// 
    /// .NET 3.0
    /// http://msdn2.microsoft.com/en-us/library/system.io.packaging.zippackage.aspx
    /// OpenXml file formats
    /// http://blogs.msdn.com/erikaehrli/archive/2006/06/23/getstartedwithOpenXMLFileFormats.aspx
    /// 
    /// TODO: Extract Document Properties (Title, Keywords)
    /// http://msdn.microsoft.com/en-us/library/aa338205.aspx [Document Profiling]
    /// http://msdn.microsoft.com/en-us/library/bb243281.aspx
    /// </remarks>
    public class DocxDocument : DownloadDocument
    {
        /*
<?xml version="1.0" encoding="UTF-8" standalone="yes"?> 
<CoreProperties xmlns="http://schemas.microsoft.com/package/2005/06/md/core-properties"> 
   <Title>Word Document Sample</Title> 
   <Subject>Microsoft Office Word 2007</Subject> 
   <Creator>2007 Microsoft Office System User</Creator> 
   <Keywords/> 
   <Description>2007 Microsoft Office system .docx file</Description> 
   <LastModifiedBy>2007 Microsoft Office System User</LastModifiedBy> 
   <Revision>2</Revision> 
   <DateCreated>2005-05-05T20:01:00Z</DateCreated> 
   <DateModified>2005-05-05T20:02:00Z</DateModified> 
</CoreProperties> 
         */
        private string _WordsOnly;

        public DocxDocument(Uri location)
            : base(location)
        {
            Extension = "docx";
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
                        , (System.IO.Path.GetFileName(this.Uri.LocalPath)) );
            this.Title = System.IO.Path.GetFileNameWithoutExtension(filename);

            SaveDownloadedFile(webresponse, filename);
            try
            {
                string entryToExtract = @"word/document.xml";
                try
                {
                    using (ZipFile zip = ZipFile.Read(filename))
                    {
                        MemoryStream stream = new MemoryStream();
                        zip.Extract(entryToExtract, stream);
                        stream.Seek(0, SeekOrigin.Begin);
                        XmlDocument xmldoc = new XmlDocument();
                        xmldoc.Load(stream);
                        _WordsOnly = xmldoc.DocumentElement.InnerText; // TODO: may require looping to add spaces between elements
                        this.All = _WordsOnly;

                        #region DEPRECATED: Dodgy byte array solution
                        //byte[] byteArray;
                        //stream.Seek(0, SeekOrigin.Begin);
                        //// Read the first 20 bytes from the stream.
                        //byteArray = new byte[stream.Length];
                        //int count = stream.Read(byteArray, 0, 20);
                        //while (count < stream.Length)
                        //{
                        //    byteArray[count++] = Convert.ToByte(stream.ReadByte());
                        //}
                        //_WordsOnly = System.Text.Encoding.UTF8.GetString(byteArray);
                        //System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("<(.|\n)+?>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        //_WordsOnly = regex.Replace(_WordsOnly, " ");
                        //this.All = _WordsOnly;
                        #endregion
                    
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                System.IO.File.Delete(filename);    // clean up
            }
            catch (Exception)
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

