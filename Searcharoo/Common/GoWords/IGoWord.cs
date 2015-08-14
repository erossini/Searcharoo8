using System;

namespace Searcharoo.Common
{
    /// <summary>
    /// IGoWord
    /// </summary>
    public interface IGoWord
    {
        /// <summary>
        /// Returns true if the word is 'specially marked' for indexing,
        /// bypassing any other Trimming, StopWord or Stemming processing.
        /// </summary>
        /// <remarks>
        /// This method is used to force special strings (possibly including punctuation)
        /// to be indexed and searched without other similar but meaningless cruft clogging
        /// up the catalog, eg. C# html+time 
        /// Note that Go Words CANNOT contain spaces, if they do they'll be recognised as 
        /// two seperate words.
        /// </remarks>
        /// <param name="word">The word to check</param>
        /// <returns>true if 'special', false if the word should be processed normally</returns>
        bool IsGoWord(string word);
    }
}
