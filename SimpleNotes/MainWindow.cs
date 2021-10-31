using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Gdk;
using GLib;
using Gtk;
using Application = Gtk.Application;
using Key = Gdk.Key;
using UI = Gtk.Builder.ObjectAttribute;
using Window = Gtk.Window;

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

        public MainWindow() : this(new("Main.glade")) { }

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);

            DeleteEvent += Window_DeleteEvent;
            _textArea.Buffer.Changed += TextChanged;
            _fileNew.Activated += (object sender, EventArgs e) => NewFile();
            _fileOpen.Activated += (object sender, EventArgs e) => OpenFile();
            _fileSave.Activated += (object sender, EventArgs e) => SaveFile(false);
            _fileSaveAs.Activated += (object sender, EventArgs e) => SaveFile(true);
            _fileQuit.Activated += (object sender, EventArgs e) => Close();
            KeyPressEvent += KeyBindings;

            NewFile();
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            if (_changed)
            {
                a.RetVal = true;

                MessageDialog dialog = new("Do you want to save the currently edited file before closing?", "SimpleNotes");
                dialog.OnResponse += (object sender, DialogResultArgs e) =>
                {
                    if (e.responseType == ResponseType.Yes)
                    {
                        OnSaved = (success) =>
                        {
                            if (!success)
                            {
                                dialog.Destroy();
                            }
                            else
                            {
                                _editorFile = "";
                                _textArea.Buffer.Text = "";
                                _changed = false;

                                UpdateTitle();

                                dialog.Destroy();

                                Application.Quit();
                            }
                        };

                        SaveFile(false);

                        return;
                    }
                    else if (e.responseType == ResponseType.Cancel)
                    {
                        Show();
                        dialog.Destroy();
                        return;
                    }

                    dialog.Destroy();

                    Application.Quit();
                };
                dialog.DestroyWithParent = true;
                dialog.Show();
            }
            else
            {
                Application.Quit();
            }
        }

        private bool _changed = false;
        private string _editorFile = "";

        private void TextChanged(object sender, EventArgs e)
        {
            _changed = true;
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            string title = "Untitled";

            if (_editorFile != "")
                title = System.IO.Path.GetFileName(_editorFile);

            Title = (_changed ? "*" : "") + title + " - SimpleNotes";
        }

        private delegate void FileSavedHandler(bool success);
        private event FileSavedHandler OnSaved;

        private void NewFile()
        {
            if (_changed)
            {
                MessageDialog dialog = new("Do you want to save the currently edited file?", "New File");
                dialog.OnResponse += (object sender, DialogResultArgs e) =>
                {
                    if (e.responseType == ResponseType.Yes)
                    {
                        OnSaved = (success) =>
                        {
                            if (!success)
                            {
                                dialog.Destroy();
                            }
                            else
                            {
                                _editorFile = "";
                                _textArea.Buffer.Text = "";
                                _changed = false;

                                UpdateTitle();

                                dialog.Destroy();
                            }
                        };

                        SaveFile(false);

                        return;
                    }
                    else if (e.responseType == ResponseType.Cancel)
                    {
                        dialog.Destroy();
                        return;
                    }

                    _editorFile = "";
                    _textArea.Buffer.Text = "";
                    _changed = false;

                    UpdateTitle();

                    dialog.Destroy();
                };
                dialog.DestroyWithParent = true;
                dialog.Show();
            }
            else
            {
                _editorFile = "";
                _textArea.Buffer.Text = "";
                _changed = false;

                UpdateTitle();
            }
        }
        private void OpenFile()
        {
            FileChooserDialog openFileDialog = new("Open a file...", null, FileChooserAction.Open);
            openFileDialog.Response += (o, args) =>
            {
                if (args.ResponseId == ResponseType.Accept)
                {
                    if (_changed)
                    {
                        MessageDialog dialog = new("Do you want to save the currently edited file?", "Open a file");
                        dialog.OnResponse += (object sender, DialogResultArgs e) =>
                        {
                            if (e.responseType == ResponseType.Yes)
                            {
                                OnSaved = (success) =>
                                {
                                    if (!success)
                                    {
                                        dialog.Destroy();

                                        openFileDialog.Destroy();
                                    }
                                    else
                                    {
                                        _editorFile = openFileDialog.File.Uri.LocalPath;
                                        _textArea.Buffer.Text = File.ReadAllText(_editorFile);
                                        _changed = false;
                                        UpdateTitle();

                                        dialog.Destroy();

                                        openFileDialog.Destroy();
                                    }
                                };
                                SaveFile(false);

                                openFileDialog.Destroy();

                                return;
                            }
                            else if (e.responseType == ResponseType.Cancel)
                            {
                                dialog.Destroy();

                                openFileDialog.Destroy();

                                return;
                            }

                            _editorFile = openFileDialog.File.Uri.LocalPath;
                            _textArea.Buffer.Text = File.ReadAllText(_editorFile);
                            _changed = false;
                            UpdateTitle();

                            dialog.Destroy();

                            openFileDialog.Destroy();
                        };
                        dialog.Show();
                    }
                    else
                    {
                        _editorFile = openFileDialog.File.Uri.LocalPath;
                        _textArea.Buffer.Text = File.ReadAllText(_editorFile);
                        _changed = false;
                        UpdateTitle();

                        openFileDialog.Destroy();
                    }
                }
            };

            openFileDialog.AddButton("Open", ResponseType.Accept);
            openFileDialog.AddButton("Cancel", ResponseType.Cancel);
            openFileDialog.Show();
        }
        private void SaveFile(bool saveAs)
        {
            if (!saveAs && File.Exists(_editorFile))
            {
                File.WriteAllText(_editorFile, _textArea.Buffer.Text);

                _changed = false;
                UpdateTitle();

                OnSaved?.Invoke(true);
            }
            else
            {
                FileChooserDialog saveFileDialog = new("Save file as...", null, FileChooserAction.Save);
                saveFileDialog.Response += (o, args) =>
                {
                    if (args.ResponseId == ResponseType.Accept)
                    {
                        _editorFile = saveFileDialog.File.Uri.LocalPath;

                        if (!Directory.Exists(System.IO.Path.GetDirectoryName(_editorFile)))
                            Directory.CreateDirectory(_editorFile);

                        File.WriteAllText(_editorFile, _textArea.Buffer.Text);

                        _changed = false;
                        UpdateTitle();

                        OnSaved?.Invoke(true);
                    }
                    else
                    {
                        OnSaved?.Invoke(false);
                    }

                    saveFileDialog.Destroy();
                };

                saveFileDialog.AddButton("Save", ResponseType.Accept);
                saveFileDialog.AddButton("Cancel", ResponseType.Cancel);
                saveFileDialog.Show();
            }
        }

        private void KeyBindings(object o, KeyPressEventArgs args)
        {
            EventKey key = args.Event;
            if ((key.State & ModifierType.ControlMask) != 0)
            {
                if (key.Key == Key.n) NewFile();
                else if (key.Key == Key.o) OpenFile();
                else if (key.Key == Key.s) SaveFile(false);
                else if (key.Key == Key.S) SaveFile(true);
                else if (key.Key == Key.q) Close();
            }
        }

        public void OpenFile(string file)
        {
            if (File.Exists(file))
            {
                _editorFile = file;
                _textArea.Buffer.Text = File.ReadAllText(_editorFile);
                _changed = false;
                UpdateTitle();
            }
        }

        /*private void KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W && e.Control)
                Application.Exit();
        }*/

        /*private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            AboutForm aboutForm = new();
            aboutForm.ShowDialog();
        }*/
    }
}
