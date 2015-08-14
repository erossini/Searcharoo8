using System;
using System.IO;
using System.Xml;
using ionic.utils.zip;

namespace Searcharoo.Common
{
    /// <summary>
    /// Special handling for PDF IFilter documents
    /// </summary>
    /// <remarks>
    /// Extend the IFilter handling with iTextSharp:
    /// 1) extract metadata (Title)
    /// 2) fallback indexing if IFilter fails.
    /// </remarks>
    public class PdfDocument : FilterDocument
    {
        public PdfDocument(Uri location) : base(location)
        {
            Extension = "pdf";
        }
        
        /// <summary>
        /// Uses the GetResponseCore and GetResponseCoreFinalize to 'inherit' the IFilter behaviour
        /// but also extend it with iTextSharp
        /// </summary>
        /// <remarks>
        /// Add iTextSharp to get better 'title'
        /// [v7] fix by brad1213@yahoo.com	
        /// </remarks>
        public override bool GetResponse(System.Net.HttpWebResponse webresponse)
        {
            string filename = System.IO.Path.Combine(Preferences.DownloadedTempFilePath, (System.IO.Path.GetFileName(this.Uri.LocalPath)));

            base.GetResponseCore(webresponse, filename);

            // [v7] fix by brad1213@yahoo.com
            iTextSharp.text.pdf.PdfReader pdfReader = new iTextSharp.text.pdf.PdfReader(filename);
            if (null != pdfReader.Info["Title"])
            {   // overwrite the 'filename' with the embedded title
                string pdfTitle = Convert.ToString(pdfReader.Info["Title"]).Trim();
                if (!String.IsNullOrEmpty(pdfTitle))
                {
                    this.Title = pdfTitle;
                }
            }

            // Now, since we've loaded the iTextSharp library, and the EPocalipse IFilter sometimes
            // fails (old Acrobat, installation problem, etc); let's try 'indexing' the PDF with iTextSharp
            // [v7]
            if (String.IsNullOrEmpty(this.All))
            {
                this.All = String.Empty;
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                // Following code from:
                // http://www.vbforums.com/showthread.php?t=475759
                for (int p = 1; p <= pdfReader.NumberOfPages; p++)
                {
                    byte[] pageBytes = pdfReader.GetPageContent(p);

                    if (null != pageBytes)
                    {
                        iTextSharp.text.pdf.PRTokeniser token = new iTextSharp.text.pdf.PRTokeniser(new iTextSharp.text.pdf.RandomAccessFileOrArray(pageBytes));
                        while (token.NextToken())
                        {
                            iTextSharp.text.pdf.PRTokeniser.TokType tknType = token.TokenType;
                            string tknValue = token.StringValue;

                            if (tknType == iTextSharp.text.pdf.PRTokeniser.TokType.STRING)
                            {
                                sb.Append(token.StringValue);
                            }
                            else if (tknType == iTextSharp.text.pdf.PRTokeniser.TokType.NUMBER && tknValue == "-600")
                            {
                                sb.Append(" ");
                            }
                            else if (tknType == iTextSharp.text.pdf.PRTokeniser.TokType.OTHER && tknValue == "TJ")
                            {
                                sb.Append(" ");
                            }
                        }
                    }
                }
                this.All += sb.ToString().Replace('\0', ' ');
            }
            pdfReader.Close();
            
            base.GetResponseCoreFinalize(filename);
            
            if (!String.IsNullOrEmpty(this.All))
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

