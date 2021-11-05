using System.Collections.Generic;

namespace Scribbit
{
    public class EditHistory
    {
        private List<Dictionary<string, bool>> _steps;
        private string _firstVersion;
        
        public EditHistory(string content)
        {
            _steps = new();
            _firstVersion = content;
        }

        public void Changed(string current)
        {
            
        }
    }

    public class EditAction
    {
        
    }
}