using System;

namespace Searcharoo.Common
{
    /// <summary>
    /// IStopper 
    /// </summary>
    /// <remarks>
    /// http://libraries.mit.edu/tutorials/general/stopwords.html
    /// http://www.tbray.org/ongoing/When/200x/2003/07/11/Stopwords
    /// 
    /// slightly off-topic, anti-thesaurus or "site-configurable stop-words"
    /// http://www.hastingsresearch.com/net/06-anti-thesaurus.shtml
    /// </remarks>
    public interface IStopper
    {
        /// <summary>
        /// Returns the stemmed form of a word
        /// </summary>
        /// <param name="word">The word to stem. It must be capitalized</param>
        /// <returns>The stemmed word</returns>
        string StopWord(string word);
    }
}
