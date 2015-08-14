using System;
using System.Collections.Generic;
using System.Text;

namespace Searcharoo.Common
{
    public abstract class DownloadDocument : Document
    {
        public DownloadDocument(Uri location)
            : base(location)
        {}

        protected void SaveDownloadedFile(System.Net.HttpWebResponse webresponse, string filename)
        {
            System.IO.Stream filestream = webresponse.GetResponseStream();
            this.Uri = webresponse.ResponseUri;

            using (System.IO.BinaryReader reader = new System.IO.BinaryReader(filestream))
            {
                using (System.IO.FileStream iofilestream = new System.IO.FileStream(filename, System.IO.FileMode.Create))
                {
                    int BUFFER_SIZE = 1024;
                    byte[] buf = new byte[BUFFER_SIZE];
                    int n = reader.Read(buf, 0, BUFFER_SIZE);
                    while (n > 0)
                    {
                        iofilestream.Write(buf, 0, n);
                        n = reader.Read(buf, 0, BUFFER_SIZE);
                    }

                    this.Uri = webresponse.ResponseUri;
                    this.Length = iofilestream.Length;
                    iofilestream.Close();
                    iofilestream.Dispose();
                }
                reader.Close();
            }
        }
    }
}
