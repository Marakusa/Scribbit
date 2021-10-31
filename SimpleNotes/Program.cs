using System;
using Gtk;

namespace SimpleNotes
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Application.Init();

            var app = new Application("org.Marakusa.SimpleNotes", GLib.ApplicationFlags.None);
            app.Register(GLib.Cancellable.Current);

            var win = new MainWindow();
            app.AddWindow(win);

            win.Show();

            if (args.Length > 0)
                win.OpenFile(String.Join(" ", args));
            
            Application.Run();
        }
    }
}
