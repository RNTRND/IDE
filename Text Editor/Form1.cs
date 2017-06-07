using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ScintillaNET;
using ScintillaNET_FindReplaceDialog;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Drawing.Printing;

namespace Text_Editor
{
    public partial class Main_Form : Form
    {
        private String path;
        FontDialog fontDlg = new FontDialog();
        private int tabCount = 1;

        FindReplace fr;

        private Point _imageLocation = new Point(13, 5);
        private Point _imgHitArea = new Point(13, 2);

        const int LEADING_SPACE = 12;
        const int CLOSE_SPACE = 15;
        const int CLOSE_AREA = 15;
        private string words;
        public Main_Form()
        {
            InitializeComponent();
            NewDocument(false);

            fr = new FindReplace(textarea);
            wordList w = new wordList();
            words = w.generateWorldList();
            fontDlg.ShowColor = true;
            fontDlg.ShowApply = true;
            fontDlg.ShowEffects = true;
            fontDlg.ShowHelp = true;

            textarea.StyleResetDefault();
            textarea.Styles[Style.Default].Font = "Consolas";
            textarea.Styles[Style.Default].Size = 10;
            //textarea.StyleClearAll();

            textarea.Lexer = Lexer.Python;

            textarea.SetProperty("tab.timmy.whinge.level", "1");
            textarea.SetProperty("fold", "1");
            textarea.WrapMode = ScintillaNET.WrapMode.Whitespace;
            // Line numbers
            textarea.Margins[0].Width = 16;

            // Use margin 2 for fold markers
            textarea.Margins[2].Type = MarginType.Symbol;
            textarea.Margins[2].Mask = Marker.MaskFolders;
            textarea.Margins[2].Sensitive = true;
            textarea.Margins[2].Width = 20;

            // Customize tab control
            //tabControl1.DrawItem += tabControl1_DrawItem;

            // Reset folder markers
            for (int i = Marker.FolderEnd; i <= Marker.FolderOpen; i++)
            {
                textarea.Markers[i].SetForeColor(SystemColors.ControlLightLight);
                textarea.Markers[i].SetBackColor(SystemColors.ControlDark);
            }
            textarea.Markers[Marker.Folder].Symbol = MarkerSymbol.BoxPlus;
            textarea.Markers[Marker.Folder].SetBackColor(SystemColors.ControlText);
            textarea.Markers[Marker.FolderOpen].Symbol = MarkerSymbol.BoxMinus;
            textarea.Markers[Marker.FolderEnd].Symbol = MarkerSymbol.BoxPlusConnected;
            textarea.Markers[Marker.FolderEnd].SetBackColor(SystemColors.ControlText);
            textarea.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
            textarea.Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.BoxMinusConnected;
            textarea.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
            textarea.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

            // Enable automatic folding
            textarea.AutomaticFold = (AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change);

            // Set the styles
            textarea.Styles[Style.Python.Default].ForeColor = Color.FromArgb(0x80, 0x80, 0x80);
            textarea.Styles[Style.Python.CommentLine].ForeColor = Color.Red; ;
            textarea.Styles[Style.Python.CommentLine].Italic = true;
            textarea.Styles[Style.Python.Number].ForeColor = Color.FromArgb(0x00, 0x7F, 0x7F);
            textarea.Styles[Style.Python.String].ForeColor = Color.LightBlue;
            textarea.Styles[Style.Python.Character].ForeColor = Color.Silver;
            textarea.Styles[Style.Python.Word].ForeColor = Color.FromArgb(0x00, 0x00, 0x7F);
            textarea.Styles[Style.Python.Word].Bold = true;
            textarea.Styles[Style.Python.Triple].ForeColor = Color.DarkRed;
            textarea.Styles[Style.Python.TripleDouble].ForeColor = Color.FromArgb(0x7F, 0x00, 0x00);
            textarea.Styles[Style.Python.ClassName].ForeColor = Color.FromArgb(0x00, 0x00, 0xFF);
            textarea.Styles[Style.Python.ClassName].Bold = true;
            textarea.Styles[Style.Python.DefName].ForeColor = Color.FromArgb(0x00, 0x7F, 0x7F);
            textarea.Styles[Style.Python.DefName].Bold = true;
            textarea.Styles[Style.Python.Operator].Bold = true;
            textarea.Styles[Style.Python.CommentBlock].ForeColor = Color.FromArgb(0x7F, 0x7F, 0x7F);
            textarea.Styles[Style.Python.CommentBlock].Italic = true;
            textarea.Styles[Style.Python.StringEol].ForeColor = Color.FromArgb(0x00, 0x00, 0x00);
            textarea.Styles[Style.Python.StringEol].BackColor = Color.FromArgb(0xE0, 0xC0, 0xE0);
            textarea.Styles[Style.Python.StringEol].FillLine = true;
            textarea.Styles[Style.Python.Word2].ForeColor = Color.FromArgb(0x40, 0x70, 0x90);
            textarea.Styles[Style.Python.Decorator].ForeColor = Color.FromArgb(0x80, 0x50, 0x00);

            // Important for Python
            textarea.ViewWhitespace = WhitespaceMode.VisibleAlways;

            //var python = "and as assert break class continue def del elif else except exec finally for from global if import in is lambda not or pass print raise return try while with yield var char int";

            var cython = "cdef cimport cpdef";

            textarea.SetKeywords(0, words + " " + cython);

        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewDocument(true);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog of = new OpenFileDialog() { Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*", Multiselect = false, ValidateNames = true, RestoreDirectory = true, Title = "Browse Text Files", DefaultExt = "txt" })
            {
                if (of.ShowDialog() == DialogResult.OK)
                {

                    using (StreamReader reader = new StreamReader(of.FileName))
                    {
                        try
                        {
                            TabPage tabPage = new TabPage(string.Format("new {0}", tabCount));
                            Scintilla newTab = new Scintilla();

                            tabControl1.TabPages.Add(tabPage);
                            tabPage.Controls.Add(newTab);
                            newTab.Dock = DockStyle.Fill;

                            init(newTab);

                            ////line numbers
                            //newTab.Margins[0].Width = 16;

                            //// Use margin 2 for fold markers
                            //newTab.Margins[2].Type = MarginType.Symbol;
                            //newTab.Margins[2].Mask = Marker.MaskFolders;
                            //newTab.Margins[2].Sensitive = true;
                            //newTab.Margins[2].Width = 20;

                            // path = of.FileName;
                            // string name1 = System.IO.Path.GetFileName(of.FileName);
                            //// tabControl1.SelectedTab.Text = name1;

                            var document = newTab.Document;
                            newTab.AddRefDocument(document);
                            newTab.Document = Document.Empty;
                            tabPage.Tag = document;

                            tabControl1.SelectedTab = tabPage;
                            tabCount++;

                            //NewDocument(true);
                            path = of.FileName;
                            Task<string> str = reader.ReadToEndAsync();
                            newTab.Text = str.Result;

                            string name = System.IO.Path.GetFileName(of.FileName);
                            tabControl1.SelectedTab.Text = name;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Message: ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        public async void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(path))
            {
                using (SaveFileDialog sv = new SaveFileDialog() { Filter = "Text files |*.txt", ValidateNames = true, Title = "Save Text Files", DefaultExt = "py", RestoreDirectory = true })
                {
                    if (sv.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            path = sv.FileName;
                            string name = System.IO.Path.GetFileName(sv.FileName);
                            tabControl1.SelectedTab.Text = name;

                            Main_Form.ActiveForm.Text = name + "- Text Editor";

                            using (StreamWriter sw = new StreamWriter(sv.FileName))
                            {
                                await sw.WriteLineAsync(textarea.Text);

                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Message: ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                    }

                }
            }
            else
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(path))
                    {
                        await sw.WriteLineAsync(textarea.Text);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Message: ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }

        private async void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sv = new SaveFileDialog() { Filter = "Text files |*.txt", ValidateNames = true, Title = "Save Text Files", DefaultExt = "txt", RestoreDirectory = true })
            {
                if (sv.ShowDialog() == DialogResult.OK)
                {
                    try
                    {

                        path = sv.FileName;

                        string name = System.IO.Path.GetFileName(sv.FileName);
                        tabControl1.SelectedTab.Text = name;

                        Main_Form.ActiveForm.Text = name + "- Text Editor";

                        using (StreamWriter sw = new StreamWriter(sv.FileName))
                        {
                            await sw.WriteLineAsync(textarea.Text);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Message: ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }

            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (textarea.Modified == false && string.IsNullOrEmpty(path))
            {
                Application.Exit();
            }
            else if (textarea.Modified == true)
            {
                DialogResult save_on_exit = MessageBox.Show("Save before exiting?", "Exit?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (save_on_exit == System.Windows.Forms.DialogResult.Yes)
                {
                    saveToolStripMenuItem_Click(sender, e);
                    Application.Exit();
                }
                //Application is exited when Cancel is clicked
                else if (save_on_exit == DialogResult.No)
                {
                    Application.Exit();
                }
            }
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textarea.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textarea.Redo();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textarea.Cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textarea.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textarea.Paste();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textarea.SelectAll();
        }

        private void colorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            if (cd.ShowDialog() == DialogResult.OK)
            {
                //textarea.Styles[Style.Default].ForeColor = cd.Color;
                textarea.Styles[Style.Default].BackColor = cd.Color;
            }
        }

        private void compileAndRunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (textarea.Modified == true)
            {
                DialogResult save_before_run = MessageBox.Show("Source was modified, save before execution?", "Save Before Run?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (save_before_run == DialogResult.OK)
                {
                    saveToolStripMenuItem_Click(sender, e);
                }
                //Nothing is done when cancel is clicked
            }

            String pypath = "";
            using (OpenFileDialog of = new OpenFileDialog() { Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*", Multiselect = false, ValidateNames = true, RestoreDirectory = true, Title = "Browse Files" })
            {
                if (of.ShowDialog() == DialogResult.OK)
                {
                    using (StreamReader reader = new StreamReader(of.FileName))
                    {
                        try
                        {
                            pypath = of.FileName;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Message: ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }


            // Create new process start info 
            ProcessStartInfo Py = new ProcessStartInfo(pypath);

            // make sure we can read the output from stdout 
            Py.UseShellExecute = false;
            Py.RedirectStandardOutput = true;

            Py.Arguments = path;

            Process PyProcess = new Process();
            // assign start information to the process 
            PyProcess.StartInfo = Py;

            //Console.WriteLine("Calling Python script with arguments {0} and {1}", x, y);
            // start the process 
            PyProcess.Start();

            // Read the standard output of the app we called.  
            // in order to avoid deadlock we will read output first 
            // and then wait for process terminate: 
            StreamReader myStreamReader = PyProcess.StandardOutput;
            string myString = myStreamReader.ReadToEnd();

            // wait exit signal from the app we called and then close it. 
            PyProcess.WaitForExit();
            PyProcess.Close();

            // write the output we got from python app 
            //console.Text = myString;

        }

        private void Main_Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (textarea.Modified == false && string.IsNullOrEmpty(path))
            {
                Application.Exit();
            }
            else if (textarea.Modified == true)
            {
                DialogResult save_on_exit = MessageBox.Show("Save before exiting?", "Exit?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (save_on_exit == System.Windows.Forms.DialogResult.Yes)
                {
                    saveToolStripMenuItem_Click(sender, e);
                    Application.Exit();
                }
                //Application is exited when Cancel is clicked
                else if (save_on_exit == System.Windows.Forms.DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        //private static string showMatch(string text, string expr)
        //{
        //    try
        //    {
        //        MatchCollection mc = Regex.Matches(text, expr);
        //        string[] typeMatch = new string[10];
        //        int i = 0;


        //        foreach (Match m in mc)
        //        {
        //            typeMatch[i++] = m.ToString();

        //        }
        //        String typeFinal = string.Join(" ", typeMatch);

        //        return typeFinal;
        //    }
        //    catch( Exception e)
        //    {
        //        return null;
        //    }

        //}

        /*private void searchList()
        {

            wordList w = new wordList();
            string words = w.generateWorldList();
            // Indicators 0-7 could be in use by a lexer
            // so we'll use indicator 8 to highlight words.
            const int NUM = 8;

            // Remove all uses of our indicator
            textarea.IndicatorCurrent = NUM;
            textarea.IndicatorClearRange(0, textarea.TextLength);

            // Update indicator appearance
            textarea.Indicators[NUM].Style = IndicatorStyle.StraightBox;
            textarea.Indicators[NUM].Under = true;
            textarea.Indicators[NUM].ForeColor = Color.Green;
            textarea.Indicators[NUM].OutlineAlpha = 50;
            textarea.Indicators[NUM].Alpha = 30;

            // Search the document
            textarea.TargetStart = 0;
            textarea.TargetEnd = textarea.TextLength;
            textarea.SearchFlags = SearchFlags.None;
            while (textarea.SearchInTarget(words) != -1)
            {
                // Mark the search results with the current indicator
                words.IndicatorFillRange(words.TargetStart, words.TargetEnd - words.TargetStart);

                // Search the remainder of the document
                words.TargetStart = words.TargetEnd;
                words.TargetEnd = words.TextLength;

            }

        }*/

        private void textarea_CharAdded(object sender, CharAddedEventArgs e)
        {
            try
            {

                int currentPos = textarea.CurrentPosition;
                int wordStartPos = textarea.WordStartPosition(currentPos, true);
                //var firstChar = textarea.GetCharAt(currentPos);
                char firstChar = (char)textarea.GetCharAt(currentPos);
                //String a = firstChar.ToString();
                //textarea.Text = a;
                //textarea.AppendText(a);
                var lenEntered = currentPos - wordStartPos;

                //string[] typeMatch = new string[25];
                //int i = 0;
                //console.Text = firstChar.ToString();
                //console.Text = words; 
                //MessageBox.Show(words);
                //string s = @"\<"+firstChar+"*>";
                //textarea.Text = s;
                //string s = @"\bS\S*";
                /*Regex rgx = new Regex(words);

                MatchCollection mc = Regex.Matches(words, s);

                foreach (Match m in mc)
                {
                    if (m != null)
                    {
                        typeMatch[i] = m.ToString();
                        console.Text = m.Value;
                        i++;
                    }

                }


                String typeFinal = string.Join(" ", typeMatch);
                */
                if (lenEntered > 0)
                {
                    if (!textarea.AutoCActive)
                    {
                        //textarea.AutoCShow(lenEntered, typeFinal);
                        textarea.AutoCShow(lenEntered, words);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message: ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void fontToolStripMenuItem_Click(object sender, EventArgs e)
        {

            fontDlg.ShowDialog();


            //if (fontDlg.ShowDialog() == DialogResult.OK & !String.IsNullOrEmpty(textarea.Text))
            //{    
            //textarea.Font = fontDlg.Font;
            //textarea.BackColor = fontDlg.Color;

            //textarea.Styles[Style.Default].Font = fontDlg.Font.ToString();
            //textarea.Styles[Style.Default].ForeColor = fontDlg.Color;
            //fontDlg.Font = null;
            //textarea.Styles[Style.].Size = fontDlg.;
            //textarea.StyleClearAll();
            //}

            textarea.StyleResetDefault();
            textarea.Styles[Style.Default].Font = fontDlg.Font.ToString();
            textarea.Styles[Style.Default].Size = 10;
            textarea.StyleClearAll();

            textarea.Lexer = Lexer.Python;

            textarea.SetProperty("tab.timmy.whinge.level", "1");
            textarea.SetProperty("fold", "1");

            //line numbers
            textarea.Margins[0].Width = 16;

            // Use margin 2 for fold markers
            textarea.Margins[2].Type = MarginType.Symbol;
            textarea.Margins[2].Mask = Marker.MaskFolders;
            textarea.Margins[2].Sensitive = true;
            textarea.Margins[2].Width = 20;

            // Reset folder markers
            for (int i = Marker.FolderEnd; i <= Marker.FolderOpen; i++)
            {
                textarea.Markers[i].SetForeColor(SystemColors.ControlLightLight);
                textarea.Markers[i].SetBackColor(SystemColors.ControlDark);
            }
        }

        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.Graphics.DrawString("x", e.Font, Brushes.Black, e.Bounds.Right - CLOSE_AREA, e.Bounds.Top + 4);
            e.Graphics.DrawString(this.tabControl1.TabPages[e.Index].Text, e.Font, Brushes.Black, e.Bounds.Left + LEADING_SPACE, e.Bounds.Top + 4);
            e.DrawFocusRectangle();
        }

        private void TabControl1_MouseDown(object sender, MouseEventArgs e)
        {
            Rectangle r = tabControl1.GetTabRect(this.tabControl1.SelectedIndex);
            Rectangle closeButton = new Rectangle(r.Right - 15, r.Top + 4, 9, 7);

            if (closeButton.Contains(e.Location))
            {
                if (MessageBox.Show("Save before exiting?", "Exit?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    saveToolStripMenuItem_Click(sender, e);
                    this.tabControl1.TabPages.Remove(this.tabControl1.SelectedTab);
                    Main_Form.ActiveForm.Text = "Text Editor";

                }
                else
                {
                    this.tabControl1.TabPages.Remove(this.tabControl1.SelectedTab);
                }
            }
        }

        private void SwitchDocument(Document nextDocument)
        {
            Scintilla newText = new Scintilla();
            var prevDocument = newText.Document;
            newText.AddRefDocument(prevDocument);
            newText.Dock = DockStyle.Fill;

            newText.Document = nextDocument;
            newText.ReleaseDocument(nextDocument);
            tabControl1.SelectedTab.Tag = prevDocument;
            tabControl1.SelectedTab.Controls.Add(newText);
            textarea.AddRefDocument(prevDocument);
            tabControl1.MouseDown += TabControl1_MouseDown;
        }

        public void init(Scintilla textarea)
        {
            fontDlg.ShowColor = true;
            fontDlg.ShowApply = true;
            fontDlg.ShowEffects = true;
            fontDlg.ShowHelp = true;

            textarea.StyleResetDefault();
            textarea.Styles[Style.Default].Font = "Consolas";
            textarea.Styles[Style.Default].Size = 10;
            textarea.StyleClearAll();

            textarea.Lexer = Lexer.Python;

            textarea.SetProperty("tab.timmy.whinge.level", "1");
            textarea.SetProperty("fold", "1");

            // Line numbers
            textarea.Margins[0].Width = 16;

            // Use margin 2 for fold markers
            textarea.Margins[2].Type = MarginType.Symbol;
            textarea.Margins[2].Mask = Marker.MaskFolders;
            textarea.Margins[2].Sensitive = true;
            textarea.Margins[2].Width = 20;

            // Customize tab control
            //tabControl1.DrawItem += tabControl1_DrawItem;

            // Reset folder markers
            for (int i = Marker.FolderEnd; i <= Marker.FolderOpen; i++)
            {
                textarea.Markers[i].SetForeColor(SystemColors.ControlLightLight);
                textarea.Markers[i].SetBackColor(SystemColors.ControlDark);
            }
            textarea.Markers[Marker.Folder].Symbol = MarkerSymbol.BoxPlus;
            textarea.Markers[Marker.Folder].SetBackColor(SystemColors.ControlText);
            textarea.Markers[Marker.FolderOpen].Symbol = MarkerSymbol.BoxMinus;
            textarea.Markers[Marker.FolderEnd].Symbol = MarkerSymbol.BoxPlusConnected;
            textarea.Markers[Marker.FolderEnd].SetBackColor(SystemColors.ControlText);
            textarea.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
            textarea.Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.BoxMinusConnected;
            textarea.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
            textarea.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

            // Enable automatic folding
            textarea.AutomaticFold = (AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change);

            // Set the styles
            textarea.Styles[Style.Python.Default].ForeColor = Color.FromArgb(0x80, 0x80, 0x80);
            textarea.Styles[Style.Python.CommentLine].ForeColor = Color.Red; ;
            textarea.Styles[Style.Python.CommentLine].Italic = true;
            textarea.Styles[Style.Python.Number].ForeColor = Color.FromArgb(0x00, 0x7F, 0x7F);
            textarea.Styles[Style.Python.String].ForeColor = Color.LightBlue;
            textarea.Styles[Style.Python.Character].ForeColor = Color.Silver;
            textarea.Styles[Style.Python.Word].ForeColor = Color.FromArgb(0x00, 0x00, 0x7F);
            textarea.Styles[Style.Python.Word].Bold = true;
            textarea.Styles[Style.Python.Triple].ForeColor = Color.DarkRed;
            textarea.Styles[Style.Python.TripleDouble].ForeColor = Color.FromArgb(0x7F, 0x00, 0x00);
            textarea.Styles[Style.Python.ClassName].ForeColor = Color.FromArgb(0x00, 0x00, 0xFF);
            textarea.Styles[Style.Python.ClassName].Bold = true;
            textarea.Styles[Style.Python.DefName].ForeColor = Color.FromArgb(0x00, 0x7F, 0x7F);
            textarea.Styles[Style.Python.DefName].Bold = true;
            textarea.Styles[Style.Python.Operator].Bold = true;
            textarea.Styles[Style.Python.CommentBlock].ForeColor = Color.FromArgb(0x7F, 0x7F, 0x7F);
            textarea.Styles[Style.Python.CommentBlock].Italic = true;
            textarea.Styles[Style.Python.StringEol].ForeColor = Color.FromArgb(0x00, 0x00, 0x00);
            textarea.Styles[Style.Python.StringEol].BackColor = Color.FromArgb(0xE0, 0xC0, 0xE0);
            textarea.Styles[Style.Python.StringEol].FillLine = true;
            textarea.Styles[Style.Python.Word2].ForeColor = Color.FromArgb(0x40, 0x70, 0x90);
            textarea.Styles[Style.Python.Decorator].ForeColor = Color.FromArgb(0x80, 0x50, 0x00);

            // Important for Python
            textarea.ViewWhitespace = WhitespaceMode.VisibleAlways;

            //var python = "and as assert break class continue def del elif else except exec finally for from global if import in is lambda not or pass print raise return try while with yield var char int";
            var python = "False None True and as assert break class continue def del elif else except finally for from global if import in is lambda nonlocal not or pass raise return try while with yield";
            var cython = "cdef cimport cpdef";

            textarea.SetKeywords(0, python + " " + cython);
        }

        public void NewDocument(bool addNewPage)
        {
            if (addNewPage)
            {
                TabPage tabPage = new TabPage(string.Format("new {0}", tabCount));
                Scintilla newTab = new Scintilla();

                tabControl1.TabPages.Add(tabPage);
                tabPage.Controls.Add(newTab);
                newTab.Dock = DockStyle.Fill;

                init(newTab);

                ////line numbers
                //newTab.Margins[0].Width = 16;

                //// Use margin 2 for fold markers
                //newTab.Margins[2].Type = MarginType.Symbol;
                //newTab.Margins[2].Mask = Marker.MaskFolders;
                //newTab.Margins[2].Sensitive = true;
                //newTab.Margins[2].Width = 20;

                var document = newTab.Document;
                newTab.AddRefDocument(document);
                newTab.Document = Document.Empty;
                tabPage.Tag = document;

                tabControl1.SelectedTab = tabPage;
                tabCount++;
            }
            else
            {
                var document = textarea.Document;
                textarea.AddRefDocument(document);
                textarea.Document = Document.Empty;
                tabControl1.SelectedTab.Tag = document;
                tabCount++;
            }
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            int currentTabIndex = tabControl1.SelectedIndex;
            var lastIndex = this.tabControl1.TabCount - 1;

            SwitchDocument((Document)tabControl1.TabPages[currentTabIndex].Tag);
        }



        private void Main_Form_Load(object sender, EventArgs e)
        {
            // get the inital length
            int tabLength = tabControl1.ItemSize.Width;

            // measure the text in each tab and make adjustment to the size
            for (int i = 0; i < this.tabControl1.TabPages.Count; i++)
            {
                TabPage currentPage = tabControl1.TabPages[i];

                int currentTabLength = TextRenderer.MeasureText(currentPage.Text, tabControl1.Font).Width;

                // adjust the length for what text is written
                currentTabLength += LEADING_SPACE + CLOSE_SPACE + CLOSE_AREA;

                if (currentTabLength > tabLength)
                {
                    tabLength = currentTabLength;
                }
            }

            // create the new size
            Size newTabSize = new Size(tabLength, tabControl1.ItemSize.Height);
            tabControl1.ItemSize = newTabSize;
            tabControl1.DrawItem += tabControl1_DrawItem;
            tabControl1.MouseDown += TabControl1_MouseDown;
        }

        private void closeTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Save before exiting?", "Exit?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                saveToolStripMenuItem_Click(sender, e);
                this.tabControl1.TabPages.Remove(this.tabControl1.SelectedTab);
                Main_Form.ActiveForm.Text = "Text Editor";
            }
            else if (dr == DialogResult.No)
            {
                this.tabControl1.TabPages.Remove(this.tabControl1.SelectedTab);
            }

        }

        private void closeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this.tabControl1.TabCount; i++)
            {
                if (i != tabControl1.SelectedIndex)
                {
                    if (textarea.Modified == true)
                    {
                        DialogResult dr = MessageBox.Show("Save before exiting?", "Exit?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                        if (dr == DialogResult.Yes)
                        {
                            saveToolStripMenuItem_Click(sender, e);
                            Main_Form.ActiveForm.Text = "Text Editor";
                            tabControl1.TabPages.RemoveAt(i--);
                        }
                        else if (dr == System.Windows.Forms.DialogResult.No)
                        {
                            Main_Form.ActiveForm.Text = "Text Editor";
                            tabControl1.TabPages.RemoveAt(i--);
                        }
                        else if (dr == System.Windows.Forms.DialogResult.Cancel)
                        {
                            break;
                        }

                    }
                    else
                    {
                        tabControl1.TabPages.RemoveAt(i--);

                        tabCount--;
                    }

                }
                tabControl1.SelectedTab.Text = "new";
            }
            // Application.Exit();


        }

        private void button1_Click(object sender, EventArgs e)
        {
            NewDocument(true);
        }

        private void indentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textarea.BeginUndoAction();

            multiPurposeFunction("\n", "\t", 1);
            multiPurposeFunction("\n", "\t", 1);

            textarea.EndUndoAction();
        }

        public void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fr.ShowFind();
        }

        private void multiPurposeFunction(String find, String replce, int n)
        {
            textarea.TargetStart = textarea.SelectionStart - n;
            textarea.TargetEnd = textarea.SelectionEnd;
            int end = textarea.TargetEnd;
            while (textarea.SearchInTarget(find) != -1)
            {
                textarea.ReplaceTarget(replce);


                textarea.TargetStart = textarea.SelectionStart;
                textarea.TargetEnd = end;

            }
            textarea.TargetStart = 0;
            textarea.TargetEnd = 0;


        }

        private void zoomInToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textarea.ZoomIn();
        }

        private void zoomOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textarea.ZoomOut();
        }

        private void commentToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            this.textarea.BeginUndoAction();

            multiPurposeFunction("\n", "# ", 1);
            multiPurposeFunction("\n", "# ", 1);

            this.textarea.EndUndoAction();
        }

        private void uncommentToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            this.textarea.BeginUndoAction();
            multiPurposeFunction("# ", "\n", 0);
            multiPurposeFunction("# ", "\n", 0);
            this.textarea.EndUndoAction();
        }

        private void outdentToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            textarea.BeginUndoAction();
            multiPurposeFunction("\t", "\n", 0);
            multiPurposeFunction("\t", "\n", 0);
            multiPurposeFunction("\t", "\n", 0);
            textarea.EndUndoAction();
        }

        private void wordWrapToolStripMenuItem_Click(object sender, EventArgs e)
        {

            textarea.AnchorPosition = textarea.CurrentPosition = 1;
            textarea.ScrollCaret();
            textarea.Focus();
        }

        private void findAndReplaceToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            fr.ShowReplace();
        }

        private void foldToolStripMenuItem1_Click(object sender, EventArgs e)
        {

            // Set the lexer
            textarea.Lexer = Lexer.Python;

            // Instruct the lexer to calculate folding
            textarea.SetProperty("fold", "1");
            textarea.SetProperty("fold.compact", "1");

            // Configure a margin to display folding symbols
            textarea.Margins[2].Type = MarginType.Symbol;
            textarea.Margins[2].Mask = Marker.MaskFolders;
            textarea.Margins[2].Sensitive = true;
            textarea.Margins[2].Width = 20;

            // Set colors for all folding markers
            for (int i = 25; i <= 31; i++)
            {
                textarea.Markers[i].SetForeColor(SystemColors.ControlLightLight);
                textarea.Markers[i].SetBackColor(SystemColors.ControlDark);
            }

            // Configure folding markers with respective symbols
            textarea.Markers[Marker.Folder].Symbol = MarkerSymbol.BoxPlus;
            textarea.Markers[Marker.FolderOpen].Symbol = MarkerSymbol.BoxMinus;
            textarea.Markers[Marker.FolderEnd].Symbol = MarkerSymbol.BoxPlusConnected;
            textarea.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
            textarea.Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.BoxMinusConnected;
            textarea.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
            textarea.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

            textarea.AutomaticFold = (AutomaticFold.Show | AutomaticFold.Change | AutomaticFold.Click);
        }

        private void unfoldToolStripMenuItem1_Click(object sender, EventArgs e)
        {

            // Set the lexer
            textarea.Lexer = Lexer.Python;

            // Instruct the lexer to calculate folding
            textarea.SetProperty("fold", "1");
            textarea.SetProperty("fold.compact", "1");

            // Configure a margin to display folding symbols
            textarea.Margins[2].Type = MarginType.Symbol;
            textarea.Margins[2].Mask = Marker.MaskFolders;
            textarea.Margins[2].Sensitive = true;
            textarea.Margins[2].Width = 20;

            // Set colors for all folding markers
            for (int i = 25; i <= 31; i++)
            {
                textarea.Markers[i].SetForeColor(SystemColors.ControlLightLight);
                textarea.Markers[i].SetBackColor(SystemColors.ControlDark);
            }

            // Configure folding markers with respective symbols
            textarea.Markers[Marker.Folder].Symbol = MarkerSymbol.BoxPlus;
            textarea.Markers[Marker.FolderOpen].Symbol = MarkerSymbol.BoxMinus;
            textarea.Markers[Marker.FolderEnd].Symbol = MarkerSymbol.BoxPlusConnected;
            textarea.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
            textarea.Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.BoxMinusConnected;
            textarea.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
            textarea.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

            textarea.AutomaticFold = (AutomaticFold.Show | AutomaticFold.None);
        }
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textarea.DeleteRange(textarea.SelectionStart, textarea.SelectedText.Length);
        }

        private void fontToolStripMenuItem1_Click(object sender, EventArgs e)
        {

            fontDlg.ShowDialog();

            textarea.StyleResetDefault();
            textarea.Styles[Style.Default].Font = fontDlg.Font.Name.ToString();
            textarea.Styles[Style.Default].Size = (int)fontDlg.Font.Size;
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            printDialog1.Document = ThePrintDocument;
            string strText = this.richTextBox1.Text;
            myReader = new StringReader(strText);
            if (printDialog1.ShowDialog() == DialogResult.OK)
            {
                this.ThePrintDocument.Print();
            }
        }

        void printButton_Click(object sender, EventArgs e)
        {
            CaptureScreen();
            printDocument1.Print();
        }


        Bitmap memoryImage;

        private void CaptureScreen()
        {
            Graphics myGraphics = this.CreateGraphics();
            Size s = this.Size;
            memoryImage = new Bitmap(s.Width, s.Height, myGraphics);
            Graphics memoryGraphics = Graphics.FromImage(memoryImage);
            memoryGraphics.CopyFromScreen(this.Location.X, this.Location.Y, 0, 0, s);
        }

        private void printDocument1_PrintPage(System.Object sender,
               System.Drawing.Printing.PrintPageEventArgs e)
        {
            e.Graphics.DrawImage(memoryImage, 0, 0);
        }



        public static void Main()
        {
            Application.Run(new Form1());
        }
    }
}
    }
}
