using System;
using Gtk;

namespace Scribbit
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Application.Init();

            var app = new Application("org.Marakusa.Scribbit", GLib.ApplicationFlags.None);
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
