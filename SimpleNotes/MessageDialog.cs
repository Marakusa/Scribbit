using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace SimpleNotes
{
    class MessageDialog : Dialog
    {
        string message = "";
        string title = "";
        
        [UI] private Label _message = null;
        [UI] private Button _button1 = null;
        [UI] private Button _button2 = null;
        [UI] private Button _button3 = null;

        public delegate void DialogResultHandler(object sender, DialogResultArgs e);
        public event DialogResultHandler OnResponse;
        
        public MessageDialog(string message, string title) : this(message, title, new Builder("Dialog.glade")) { }

        private MessageDialog(string message, string title, Builder builder) : base(builder.GetRawOwnedObject("Dialog"))
        {
            this.message = message;
            this.title = title;
            
            builder.Autoconnect(this);
            
            _message.Text = this.message;
            Title = this.title;
            
            _button1.Clicked += (object sender, EventArgs args) => OnResponse?.Invoke(this, new(ResponseType.Yes));
            _button2.Clicked += (object sender, EventArgs args) => OnResponse?.Invoke(this, new(ResponseType.No));
            _button3.Clicked += (object sender, EventArgs args) => OnResponse?.Invoke(this, new(ResponseType.Cancel));

            Response += Dialog_Response;
        }

        private void Dialog_Response(object o, ResponseArgs args)
        {
            Hide();
        }
    }
    
    public class DialogResultArgs
    {
        public ResponseType responseType;

        public DialogResultArgs(ResponseType type)
        {
            responseType = type;
        }
    }
}