using System;
using System.Collections.Generic;
using System.Text;

namespace Searcharoo.Common
{
    /// <summary>
    /// 
    /// </summary>
    public class ResultFile : File
    {
        private int _Rank;
        public ResultFile(File sourceFile)
        {
            this.Url = sourceFile.Url;
            this.Title = sourceFile.Title;
            this.Description = sourceFile.Description;
            this.CrawledDate = sourceFile.CrawledDate;
            this.Size = sourceFile.Size;
            this.Rank = -1;
            this.GpsLocation = sourceFile.GpsLocation;
            this.KeywordString = sourceFile.KeywordString;
            this.Extension = sourceFile.Extension;
        }
        public int Rank
        {
            get { return _Rank; }
            set { _Rank = value; }
        }
        public string GpsLocationHtml
        {
            get
            {
                if (this.GpsLocation == null)
                {
                    return "";
                }
                else
                {
                    return String.Format(@"<a href='http://maps.google.com/maps?f=q&hl=en&geocode=&q={2}&ll={0},{1}&ie=UTF8&t=h&z=16&iwloc=addr' target='_blank' class='geo'><span class='geo'>
<span class='latitude'>{0}</span>,<span class='longitude'>{1}</span>
</span></a>", 
this.GpsLocation.Latitude, this.GpsLocation.Longitude, this.Title);
                }
            }
        }



        public string TitleText
        {
            get 
            {
                return this.Title.Replace("&", "");
            }
        }
        public string DescriptionText
        {
            get
            {
                return this.Description.Replace("&", "");
            }
        }
        public string GpsLocationText
        {
            get
            {
                if (this.GpsLocation == null)
                {
                    return "0,0";
                }
                else
                {
                    return String.Format(@"{0},{1}", this.GpsLocation.Longitude, this.GpsLocation.Latitude);
                }
            }
        }
    }
}
