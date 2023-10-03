/*
DeepDungeon is licensed under a
Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.

You should have received a copy of the license along with this
work. If not, see <http://creativecommons.org/licenses/by-nc-sa/4.0/>.

Orginal work done by zzi, contibutions by Omninewb, Freiheit, and mastahg
                                                                                 */

using System;

namespace LlamaLibrary.Memory.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class OffsetAttribute : Attribute
    {
        public bool IgnoreCache;
        public string Pattern = "";
        public string PatternCN = "";


        public OffsetAttribute(string pattern, bool ignoreCache = false, int expectedValue = 0)
        {
            Pattern = pattern;
            if (!Pattern.StartsWith("Search "))
            {
                Pattern = "Search " + Pattern;
            }

            if (PatternCN == "")
            {
                PatternCN = pattern;
            }

            IgnoreCache = ignoreCache;
        }

        public OffsetAttribute(string pattern, string cnpattern, bool ignoreCache = false, int expectedValue = 0)
        {
            /*if (pattern != "")
                Pattern = pattern;*/
            PatternCN = cnpattern;
            if (!PatternCN.StartsWith("Search "))
            {
                PatternCN = "Search " + PatternCN;
            }

            IgnoreCache = ignoreCache;
        }
    }

    public class OffsetCNAttribute : OffsetAttribute
    {
        public OffsetCNAttribute(string pattern, bool ignoreCache = false, int expectedValue = 0) : base("", pattern, ignoreCache, expectedValue)
        {
        }
    }
}