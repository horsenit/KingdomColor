using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace KingdomColor
{
    class ConsoleUtilities
    {
        public static T FindObjectByIdName<T>(string idName) where T : MBObjectBase
        {
            T obj = MBObjectManager.Instance.GetObject<T>(idName);
            if (obj == null)
            {
                foreach (T candidate in MBObjectManager.Instance.GetObjectTypeList<T>())
                {
                    if (candidate.GetName().ToString().Equals(idName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        obj = candidate;
                        break;
                    }
                }
            }
            return obj;
        }

        // awkward parser, inputting "you   a'b    c'd'" => "you", "a", "b c", "d", ""
        // game throws away quotes, presplits on spaces, oh well
        public static List<string> Resplit(List<string> args)
        {
            var concat = string.Join(" ", args);
            var output = new List<string>();
            var current = new StringBuilder();
            bool isEscape = false;
            bool isQuote = false;
            bool isSpace = true;
            foreach (var c in concat)
            {
                if (isEscape)
                {
                    current.Append(c);
                    isEscape = false;
                    continue;
                }
                switch (c)
                {
                    case '\\':
                        isEscape = true;
                        isSpace = false;
                        break;
                    case '\'':
                        if (isQuote)
                        {
                            output.Add(current.ToString());
                            current.Clear();
                            isQuote = false;
                            isSpace = true;
                        }
                        else
                        {
                            isSpace = false;
                            isQuote = true;
                        }
                        break;
                    case ' ':
                    case '\n':
                    case '\t':
                        if (isQuote)
                        {
                            current.Append(c);
                        }
                        else
                        {
                            if (!isSpace)
                            {
                                output.Add(current.ToString());
                                current.Clear();
                            }
                            isSpace = true;
                        }
                        break;
                    default:
                        current.Append(c);
                        isSpace = false;
                        break;
                }
            }
            if (!isSpace)
                output.Add(current.ToString());
            return output;
        }

        public static string GetObjectList<T>() where T : MBObjectBase
        {
            var output = new StringBuilder($"\nList of {typeof(T).Name}s\n==============================\n");
            foreach (var obj in MBObjectManager.Instance.GetObjectTypeList<T>())
                output.Append($" Id: {obj.StringId}, Name: '{obj.GetName()}'\n");
            return output.ToString();
        }
    }
}
