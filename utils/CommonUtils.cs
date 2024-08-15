using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rhacktool.utils
{
    public static class CommonUtils
    {
        public static string SanitizeFileName(string fileName)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(fileName.Length);

            foreach (char c in fileName)
            {
                if (Array.Exists(invalidChars, invalidChar => invalidChar == c))
                {
                    sb.Append('_');
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
    }
}
