using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace SimpleNotes
{
    public class MainWindow : Window
    {
        [UI] private TextView _textArea = null;
        [UI] private ImageMenuItem _fileNew = null;
        [UI] private ImageMenuItem _fileOpen = null;
        [UI] private ImageMenuItem _fileSave = null;
        [UI] private ImageMenuItem _fileSaveAs = null;
        [UI] private ImageMenuItem _fileQuit = null;
        [UI] private ImageMenuItem _aboutHelp = null;

        public MainWindow() : this(new Builder("Main.glade")) { }

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);

            DeleteEvent += Window_DeleteEvent;
            _textArea.Buffer.Changed += TextChanged;
            _fileNew.Activated += (object sender, EventArgs e) => NewFile();
            
            NewFile();
        }
        
        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }

        private bool changed = false;
        private string editorFile = "";

        private void TextChanged(object sender, EventArgs e)
        {
            changed = true;
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            string title = "Untitled";

            if (editorFile != "")
                title = System.IO.Path.GetFileName(editorFile);

            Title = (changed ? "*" : "") + title + " - SimpleNotes";
        }

        private void NewFile()
        {
            if (changed)
            {
                MessageDialog dialog = new MessageDialog("Do you want to save the currently edited file?", "New File");
                dialog.OnResponse += (object sender, DialogResultArgs e) => 
                {
                    //MessageDialog dialogResult = MessageBox.Show("Do you want to save the currently edited file?", "New File", MessageBoxButtons.YesNoCancel);

                    Console.WriteLine(e.responseType.ToString());
                    
                    if (e.responseType == ResponseType.Yes)
                    {
                        //if (!SaveFile(false))
                        //{
                        //    dialog.Destroy();
                        //    return;
                        //}
                        
                        editorFile = "";
                        _textArea.Buffer.Text = "";
                        changed = false;

                        UpdateTitle();
                    }
                    else if (e.responseType == ResponseType.No)
                    {
                        editorFile = "";
                        _textArea.Buffer.Text = "";
                        changed = false;

                        UpdateTitle();
                    }
                    
                    dialog.Destroy();
                };
                dialog.DestroyWithParent = true;
                dialog.Show();
            }
            else
            {
                editorFile = "";
                _textArea.Buffer.Text = "";
                changed = false;

                UpdateTitle();
            }
        }
        /*private void OpenFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Open a file";

            DialogResult result = openFileDialog.ShowDialog();

            if (result == DialogResult.Cancel)
                return;

            if (changed)
            {
                DialogResult dialogResult = MessageBox.Show("Do you want to save the currently edited file?", "Open a file", MessageBoxButtons.YesNoCancel);

                if (dialogResult == DialogResult.Yes)
                {
                    if (!SaveFile(false))
                        return;
                }
                else if (dialogResult == DialogResult.Cancel)
                {
                    return;
                }
            }

            editorFile = openFileDialog.FileName;

            textbox.Text = File.ReadAllText(editorFile);

            changed = false;

            UpdateTitle();
        }
        private bool SaveFile(bool saveAs)
        {
            if (!saveAs && File.Exists(editorFile))
            {
                File.WriteAllText(editorFile, textbox.Text);
            }
            else
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "Save as...";
                saveFileDialog.FileName = "Untitled";
                saveFileDialog.DefaultExt = "txt";
                saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                DialogResult result = saveFileDialog.ShowDialog();

                if (result == DialogResult.Cancel)
                    return false;

                editorFile = saveFileDialog.FileName;

                if (!Directory.Exists(Path.GetDirectoryName(editorFile)))
                    Directory.CreateDirectory(editorFile);

                File.WriteAllText(editorFile, textbox.Text);
            }

            changed = false;

            UpdateTitle();

            return true;
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewFile();
        }

        private void oPenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFile(false);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFile(true);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Closing()
        {
            if (changed)
            {
                DialogResult dialogResult = MessageBox.Show("Do you want to save the currently edited file before closing?", "Text Editor", MessageBoxButtons.YesNoCancel);

                if (dialogResult == DialogResult.Yes)
                {
                    if (!SaveFile(false))
                        e.Cancel = true;
                }
                else if (dialogResult == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        /*private void KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W && e.Control)
                Application.Exit();
        }*/

        /*private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            AboutForm aboutForm = new AboutForm();
            aboutForm.ShowDialog();
        }*/
    }
}
