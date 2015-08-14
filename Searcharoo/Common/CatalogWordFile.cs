using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Searcharoo.Common
{
    /// <summary>
    /// Used solely by the <see cref="Catalog"/> class to XmlSerialize the index
    /// to disk when binary serialization will not work due to Trust level issues.
    /// </summary>
    [Serializable]
    public class CatalogWordFile
    {
        #region Private Fields: _text, _fileIds
        private string _text;
        //private List<int> _fileIds = new List<int>();
        Dictionary<int, List<int>> _FileIdsWithPosition;
        #endregion

        public CatalogWordFile()
        {
            _FileIdsWithPosition = new Dictionary<int, List<int>>();
        }

        /// <summary>
        /// The word that has been indexed
        /// </summary>
        [XmlElement("t")]
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }
        /// <summary>
        /// The 'generated identifiers' of the File objects associated with the Word
        /// </summary>
        [XmlElement("ii")]
        public string[] FileIdsWithPositionString
        {
            get 
            {
                List<string> l = new List<string>();
                string f = "";
                foreach (int fileId in _FileIdsWithPosition.Keys)
                {
                    f = "";
                    foreach (int wordPosition in _FileIdsWithPosition[fileId])
                    {
                        f += wordPosition + ",";
                    }
                    l.Add(fileId.ToString() + ":" + f.Trim(','));
                }
                return l.ToArray(); 
            }
            set 
            { 
                //List<string> l = value;
                string[] l = value;
                //_fileIds = new List<int>();
                foreach (string fileInfo in l)
                {
                    string[] fileInfoA = fileInfo.Split(':');
                    int fileId = Convert.ToInt32(fileInfoA[0]);
                    string[] wordPositionsA = fileInfoA[1].Split(',');
                    List<int> wordPositions = new List<int>();
                    foreach (string s in wordPositionsA)
                    {
                        wordPositions.Add(Convert.ToInt32(s));
                    }
                    //_fileIds.Add(fileId);
                    _FileIdsWithPosition.Add(fileId, wordPositions);
                }
            }
        }
        /// <summary>
        /// The 'generated identifiers' of the File objects associated with the Word
        /// and all the 'position indexes' of that Word in that File (comma-seperate
        /// </summary>
        [XmlIgnore]
        public Dictionary<int, List<int>> FileIdsWithPosition
        {
            get { return _FileIdsWithPosition; }
            set { _FileIdsWithPosition = value; }
        }


        ///// <summary>
        ///// The 'generated identifiers' of the File objects associated with the Word
        ///// </summary>
        //[XmlElement("i")]
        //[Obsolete("Replaced by FileIdsWithPositionString in [v7]")]
        //public List<int> FileIds
        //{
        //    get { return _fileIds; }
        //    set { _fileIds = value; }
        //}
    }
}
