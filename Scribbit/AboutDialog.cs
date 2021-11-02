using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace Scribbit
{
    class About : AboutDialog
    {
        public About() : this(new Builder("About.glade")) { }

        private About(Builder builder) : base(builder.GetRawOwnedObject("AboutDialog"))
        {
            builder.Autoconnect(this);
            Response += Dialog_Response;
        }

        private void Dialog_Response(object o, ResponseArgs args)
        {
            Destroy();
        }
    }
}