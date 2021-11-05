using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace Scribbit
{
    class MessageDialog : Dialog
    {
        [UI] private Label _message = null;
        [UI] private Button _button1 = null;
        [UI] private Button _button2 = null;
        [UI] private Button _button3 = null;

        public delegate void DialogResultHandler(object sender, DialogResultArgs e);
        public new event DialogResultHandler OnResponse;
        
        public MessageDialog(string message, string title) : this(message, title, new Builder("Dialog.glade")) { }

        private MessageDialog(string message, string title, Builder builder) : base(builder.GetRawOwnedObject("Dialog"))
        {
            builder.Autoconnect(this);
            
            _message.Text = message;
            Title = title;
            
            _button1.Clicked += delegate { OnResponse?.Invoke(this, new(ResponseType.Yes)); };
            _button2.Clicked += delegate { OnResponse?.Invoke(this, new(ResponseType.No)); };
            _button3.Clicked += delegate { OnResponse?.Invoke(this, new(ResponseType.Cancel)); };

            Response += Dialog_Response;
        }

        private void Dialog_Response(object o, ResponseArgs args)
        {
            Hide();
        }
    }
    
    public class DialogResultArgs
    {
        public readonly ResponseType ResponseType;

        public DialogResultArgs(ResponseType type)
        {
            ResponseType = type;
        }
    }
}