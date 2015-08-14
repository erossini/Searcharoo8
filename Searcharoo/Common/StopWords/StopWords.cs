using System;

namespace Searcharoo.Common
{
    public class NoStopping : IStopper
    {
        /// <summary>
        /// Basic 'noop' implementation
        /// </summary>
        /// <param name="word">Word to check against the Stop list</param>
        /// <returns>The input word is always returned unchanged</returns>
        public virtual string StopWord(string word)
        {
            return word;
        }
    }
    /// <summary>
    /// The most basic 'stop word' processor, just ignores
    /// any and ALL words of one or two characters.
    /// </summary>
    /// <remarks>
    /// Examples of words ignored:
    /// a of we in or i to 
    /// </remarks>
    public class ShortStopper : IStopper
    {
        public virtual string StopWord(string word)
        {
            if (word.Length <= 2)
            {
                return String.Empty;
            }
            else
            {
                return word;
            }
        }
    }
    /// <summary>
    /// List of Stop words in a switch statement; feel free to
    /// add additional words to this list. 
    /// Note: it only checks words that are 3 or 4 characters long,
    /// as the base() method already excludes 1 and 2 char words.
    /// </summary>
    /// <remarks>
    /// Examples of words ignored:
    /// the and that you this for but with are have was out not
    /// </remarks>
    public class ListStopper : ShortStopper
    {
        public override string StopWord(string word)
        {
            word = base.StopWord(word);
            if ((word != String.Empty) && (word.Length <= 4))
            {
                switch (word.ToLower())
                {
                    case "the":
                    case "and":
                    case "that":
                    case "you":
                    case "this":
                    case "for":
                    case "but":
                    case "with":
                    case "are":
                    case "have":
                    case "was":
                    case "out":
                    case "not":
                        return String.Empty;
                    //						break;
                }
            }
            return word;
        }
    }

    /// <summary>
    /// TODO: implement a Stopper that will read in the word list
    /// from a file. 
    /// http://snowball.tartarus.org/algorithms/english/stop.txt
    /// </summary>
    [Obsolete("well, not obsolete, just not written yet")]
    public class FileStopper : IStopper
    {
        /// <summary>
        /// Because this method will use an intelligent list to filter
        /// out stop words, it probably won't need to inherit from the
        /// dodgy implementations above.
        /// </summary>
        public string StopWord(string word)
        {
            throw new NotImplementedException();
        }
    }
}
