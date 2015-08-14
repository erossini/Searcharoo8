using System;

namespace Searcharoo.Common
{
    /// <summary>
    /// No-op - if Go Words are disabled this class will be used.
    /// </summary>
    public class NoGoWord : IGoWord
    {
        public bool IsGoWord(string word)
        {
            return false;
        }
    }
    /// <summary>
    /// List of Go words in a switch statement; feel free to
    /// add additional words to this list. 
    /// </summary>
    public class ListGoWord : IGoWord
    {
        public bool IsGoWord(string word)
        {
            switch (word.ToLower())
            {
                case "c#":
                case "vb.net":
                case "asp.net":
                    return true;
                //					break;
            }
            return false;
        }
    }
    /// <summary>
    /// TODO: implement a Go Word method that will read in the word list
    /// from a file. 
    /// http://snowball.tartarus.org/algorithms/english/stop.txt
    /// </summary>
    [Obsolete("well, not obsolete, just not written yet")]
    public class FileGoWord : IGoWord
    {
        /// <summary>
        /// Because this method will use an intelligent list to filter
        /// out stop words, it probably won't need to inherit from the
        /// dodgy implementations above.
        /// </summary>
        public bool IsGoWord(string word)
        {
            throw new NotImplementedException();
        }
    }
}
