using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace SimpleNotes
{
    class MessageDialog : Dialog
    {
        string message = "";
        string title = "";

        public MessageDialog(string message, string title) : this(message, title, new Builder("Dialog.glade")) { }

        private MessageDialog(string message, string title, Builder builder) : base(builder.GetRawOwnedObject("Dialog"))
        {
            this.message = message;
            this.title = title;

            builder.Autoconnect(this);
            DefaultResponse = ResponseType.Cancel;

            Response += Dialog_Response;
        }

        private void Dialog_Response(object o, ResponseArgs args)
        {
            Hide();
        }
    }
}