using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace typegen
{

    public static class GeneralUtility
    {
        public static string Indent(int level)
        {
            string indent = "";
            while (level > 0)
            {
                indent += "    ";
                level -= 1;
            }
            return indent;
        }

        public static void WriteIfDifferent(string targetFile, string content)
        {
            if (System.IO.File.Exists(targetFile))
            {
                var txt = System.IO.File.ReadAllText(targetFile);
                if (txt != content)
                    System.IO.File.WriteAllText(targetFile, content);
            }
            else
                System.IO.File.WriteAllText(targetFile, content);
        }

        public static string ToPrettyString(string inStr)
        {
            // turn under-scores into spaces my_name_is_bob
            inStr = inStr.Replace('_', ' ');

            // trim AFTER under-scores to spaces because of `myVariable_`
            inStr = inStr.Trim();

            if (char.IsLower(inStr[0]))
                inStr = char.ToUpper(inStr[0]) + inStr.Substring(1);

            inStr = Regex.Replace(inStr, "([A-Z0-9]+)", " $1").Trim();

            return inStr;
        }
    }
}
