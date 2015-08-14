using System;
using System.Collections.Generic;
using System.Text;

namespace Searcharoo.Common
{
    /// <summary>
    /// WPD property retrieval in C#
    /// http://blogs.msdn.com/dimeby8/archive/2006/12/11/wpd-property-retrieval-in-c.aspx
    /// 
    /// Marshalling variant properties in C#
    /// http://blogs.msdn.com/dimeby8/archive/2006/12/12/marshalling-variant-properties-in-c.aspx
    /// 
    /// Blogs
    /// http://blogs.msdn.com/dimeby8/default.aspx
    /// 
    /// TODO: OfficeFilterDocument - to use DSOFile.dll to extract properties (mainly title, keywords)
    /// http://support.microsoft.com/kb/224351
    /// http://msdn.microsoft.com/en-us/library/aa190636(office.10).aspx
    /// </summary>
    public class FilterDocument : DownloadDocument
    {
        private string _All;
        private string _WordsOnly;

        public FilterDocument(Uri location):base(location)
        {}

        /// <summary>
        /// Set 'all' and 'words only' to the same value (no parsing)
        /// </summary>
        public override string All
        {
            get { return _All; }
            set { 
                _All = value;
                _WordsOnly = _All;
            }
        }
        
        public override string WordsOnly
        {
            get { return _WordsOnly; }
        }

        public override string[] WordsArray
        {
            get { return this.WordsStringToArray(WordsOnly); }
        }
       
        /// <summary>
        /// 
        /// </summary>
        public override void Parse()
        {
            // no parsing (for now). perhaps in future we can regex look for urls (www.xxx.com) and try to link to them...
        }

        public override bool GetResponse(System.Net.HttpWebResponse webresponse)
        {
            string filename = System.IO.Path.Combine(
                Preferences.DownloadedTempFilePath
            ,   (System.IO.Path.GetFileName(this.Uri.LocalPath))
            );

            GetResponseCore(webresponse, filename);
            GetResponseCoreFinalize(filename);

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

        /// <summary>
        /// [v7] Move the primary IFilter code into a protected method, 
        /// to make extension (ie. subclassing) easier
        /// </summary>
        protected void GetResponseCore(System.Net.HttpWebResponse webresponse, string filename)
        {
            this.Title = System.IO.Path.GetFileNameWithoutExtension(filename);
            SaveDownloadedFile(webresponse, filename);
            try
            {
                EPocalipse.IFilter.FilterReader ifil = new EPocalipse.IFilter.FilterReader(filename);
                this.All = ifil.ReadToEnd();
                ifil.Close();
            }
            catch (Exception argex)
            {
                System.Diagnostics.Debug.WriteLine(argex);
            }
        }

        /// <summary>
        /// [v7] Move the IFilter cleanup code into a protected method 
        /// so that subclasses can have a look at the file before it's deleted.
        /// </summary>
        protected void GetResponseCoreFinalize(string filename)
        {
            if (System.IO.File.Exists(filename))
            {   // clean up - delete the PDF file since we don't want to cache them (for now)
                System.IO.File.Delete(filename);    
            }
        }
    }
}