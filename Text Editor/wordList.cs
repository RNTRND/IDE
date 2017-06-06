using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Text_Editor
{
    class wordList
    {
        public String generateWorldList()
        {
            string[] snippets = { "False", "None", "True", "and", "as", "assert", "break", "class", "continue", "def", "del", "elif", "else", "except", "finally", "for", "from", "global", "if", "import", "in", "is", "lambda", "nonlocal", "not", "or", "pass", "raise", "return", "try", "while", "with", "yield" };
            Array.Sort(snippets);
            String AutoCompleteWords = string.Join(" ",snippets);
            return AutoCompleteWords;

        }
    }
}