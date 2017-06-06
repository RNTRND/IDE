using System;
using System.Collections.Generic;
using System.IO;
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
           // FileStream fr = new FileStream("keyWords.txt", FileMode.Open, FileAccess.Read, FileShare.Read);
            string[] snippets = File.ReadAllText( Path.Combine(Environment.CurrentDirectory, "..\\..\\keyWords.txt")).Split(new Char[] { ' ' });
            Array.Sort(snippets);
            String AutoCompleteWords = string.Join(" ",snippets);
            return AutoCompleteWords;

        }
    }
}