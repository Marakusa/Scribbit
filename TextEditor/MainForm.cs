using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace TextEditor
{
    public partial class MainForm : Form
    {
        private bool changed = false;
        private string editorFile = "";

        public MainForm()
        {
            InitializeComponent();

            NewFile();
        }

        private void textbox_TextChanged(object sender, EventArgs e)
        {
            changed = true;
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            string title = "Untitled";

            if (editorFile != "")
                title = Path.GetFileName(editorFile);

            Text = (changed ? "*" : "") + title + " - Text Editor";
        }

        private void NewFile()
        {
            if (changed)
            {
                DialogResult dialogResult = MessageBox.Show("Do you want to save the currently edited file?", "New File", MessageBoxButtons.YesNoCancel);

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

            editorFile = "";
            textbox.Text = "";
            changed = false;

            UpdateTitle();
        }
        private void OpenFile()
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

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
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

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W && e.Control)
                Application.Exit();
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            AboutForm aboutForm = new AboutForm();
            aboutForm.ShowDialog();
        }
    }
}
