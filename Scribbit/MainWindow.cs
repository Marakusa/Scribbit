using System;
using System.IO;
using Gdk;
using Gtk;
using Application = Gtk.Application;
using Key = Gdk.Key;
using UI = Gtk.Builder.ObjectAttribute;
using Window = Gtk.Window;

namespace Scribbit
{
    public class MainWindow : Window
    {
        public AppConfig Config = new();

        [UI] private TextView _textArea = null;
        [UI] private TextView _find = null;
        [UI] private ImageMenuItem _fileNew = null;
        [UI] private ImageMenuItem _fileOpen = null;
        [UI] private ImageMenuItem _fileSave = null;
        [UI] private ImageMenuItem _fileSaveAs = null;
        [UI] private ImageMenuItem _fileQuit = null;
        [UI] private ImageMenuItem _helpAbout = null;
        [UI] private ImageMenuItem _editCopy = null;
        [UI] private ImageMenuItem _editPaste = null;
        [UI] private ImageMenuItem _editCut = null;
        [UI] private ImageMenuItem _editFind = null;
        [UI] private ImageMenuItem _editFindReplace = null;

        private About _aboutDialog = new();

        public MainWindow() : this(new Builder("Main.glade")) { }

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);

            string configDirectory =
                $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/.config/Scribbit/";
            
            GetSize(out int width, out int height);
            GetPosition(out int x, out int y);

            if (!Directory.Exists(configDirectory))
                Directory.CreateDirectory(configDirectory);
            
            if (!File.Exists($"{configDirectory}/position"))
                File.WriteAllText($"{configDirectory}/position",
                    $"{x},{y},{width},{height},{(IsMaximized ? "1" : "0")}");
            else
            {
                string[] lines = File.ReadAllText($"{configDirectory}/position").Split("\n");
                string[] values = lines[0].Split(",");
                
                int savedX = x;
                int savedY = y;
                int savedW = width;
                int savedH = height;
                int savedM = 0;

                if (values.Length > 0)
                    int.TryParse(values[0], out savedX);
                if (values.Length > 1)
                    int.TryParse(values[1], out savedY);
                if (values.Length > 2)
                    int.TryParse(values[2], out savedW);
                if (values.Length > 3)
                    int.TryParse(values[3], out savedH);
                if (values.Length > 4)
                    int.TryParse(values[4], out savedM);

                Move(savedX, savedY);
                Resize(savedW, savedH);

                if (savedM == 1)
                    Maximize();
            }

            DeleteEvent += Window_DeleteEvent;
            _textArea.Buffer.Changed += (sender, args) =>
            {
                Console.WriteLine(_textArea.Buffer.Text);
            };
            _textArea.DeleteFromCursor += DeleteRange;
            _fileNew.Activated += delegate { NewFile(); };
            _fileOpen.Activated += delegate { OpenFile(); };
            _fileSave.Activated += delegate { SaveFile(false); };
            _fileSaveAs.Activated += delegate { SaveFile(true); };
            _fileQuit.Activated += delegate { Close(); };
            _helpAbout.Activated += delegate
            {
                if (_aboutDialog != null)
                    _aboutDialog.Destroy();

                _aboutDialog = new();
                _aboutDialog.Show();
            };
            _editCopy.Activated += delegate(object? sender, EventArgs args) { _textArea.Buffer.CopyClipboard(Clipboard.GetDefault(Display.Default)); };
            _editCut.Activated += delegate(object? sender, EventArgs args) { _textArea.Buffer.CutClipboard(Clipboard.GetDefault(Display.Default), true); };
            _editPaste.Activated += delegate(object? sender, EventArgs args) { _textArea.Buffer.PasteClipboard(Clipboard.GetDefault(Display.Default)); };
            _editFind.Activated += delegate(object? sender, EventArgs args) { Find(); };
            KeyPressEvent += KeyBindings;

            NewFile();
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            string configDirectory =
                $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/.config/Scribbit/";
            
            GetSize(out int width, out int height);
            GetPosition(out int x, out int y);

            if (!Directory.Exists(configDirectory))
                Directory.CreateDirectory(configDirectory);
            
            File.WriteAllText($"{configDirectory}/position", $"{x},{y},{width},{height},{(IsMaximized ? "1" : "0")}");
            
            if (_changed)
            {
                a.RetVal = true;

                MessageDialog dialog = new("Do you want to save the currently edited file before closing?", "Scribbit");
                dialog.OnResponse += (object sender, DialogResultArgs e) =>
                {
                    if (e.ResponseType == ResponseType.Yes)
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
                    else if (e.ResponseType == ResponseType.Cancel)
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

        private bool _changed;
        private string _editorFile = "";

        private void DeleteRange(object sender, DeleteFromCursorArgs e)
        {
            foreach (var v in e.Args)
            {
                Console.WriteLine(v.ToString());
            }
            Console.WriteLine(e.Args.Length);
            Console.WriteLine(e.Count);
            Console.WriteLine(e.Type.ToString());
            _changed = true;
            UpdateTitle();
        }

        private void Find()
        {
            _find.Visible = !_find.Visible;
            
            if (_find.Visible)
                _find.GrabFocus();
            else
                _textArea.GrabFocus();
            
            if (_find.Buffer.Text.Length > 0 && _find.Visible)
            {
                // TODO: Select all find field
            }
        }
        
        private void UpdateTitle()
        {
            string title = "Untitled";

            if (_editorFile != "")
                title = System.IO.Path.GetFileName(_editorFile);

            Title = $"{(_changed ? "*" : "")}{title} - Scribbit";
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
                    if (e.ResponseType == ResponseType.Yes)
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

                            OnSaved = null;
                        };

                        SaveFile(false);

                        return;
                    }
                    else if (e.ResponseType == ResponseType.Cancel)
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
                    string path = openFileDialog.File.Uri.LocalPath;

                    if (_changed)
                    {
                        MessageDialog dialog = new("Do you want to save the currently edited file?", "Open a file");
                        dialog.OnResponse += (object sender, DialogResultArgs e) =>
                        {
                            if (e.ResponseType == ResponseType.Yes)
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
                                        _editorFile = path;
                                        _textArea.Buffer.Text = File.ReadAllText(_editorFile);
                                        _changed = false;
                                        UpdateTitle();

                                        dialog.Destroy();

                                        openFileDialog.Destroy();
                                    }

                                    OnSaved = null;
                                };
                                SaveFile(false);

                                openFileDialog.Destroy();

                                return;
                            }
                            else if (e.ResponseType == ResponseType.Cancel)
                            {
                                dialog.Destroy();

                                openFileDialog.Destroy();

                                return;
                            }

                            _editorFile = path;
                            _textArea.Buffer.Text = File.ReadAllText(_editorFile);
                            _changed = false;
                            UpdateTitle();

                            dialog.Destroy();

                            openFileDialog.Destroy();
                        };
                        dialog.Show();
                        openFileDialog.Hide();
                    }
                    else
                    {
                        _editorFile = path;
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
                else if (key.Key == Key.f) Find();
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
