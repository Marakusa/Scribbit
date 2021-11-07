using System.Collections.Generic;

namespace Scribbit
{
    public class EditHistory
    {
        private int _currentStep = 0;
        private List<EditAction> _steps;
        private string _firstVersion;
        
        public EditHistory(string content)
        {
            _steps = new();
            _firstVersion = content;
        }

        public void Add(string added, int row, int index)
        {
            if (_steps.Count > _currentStep + 1)
                _steps.RemoveRange(_currentStep + 1, _steps.Count - _currentStep + 1);
            _steps.Add(new(added, false, new(row, index), new(row, index)));
            _currentStep++;
        }

        public void Undo()
        {
            
        }
    }

    public class EditAction
    {
        public string modifiedString;
        public CursorPoint startPoint;
        public CursorPoint endPoint;
        public bool removed;

        public EditAction(string modified, bool remove, CursorPoint start, CursorPoint end)
        {
            modifiedString = modified;
            removed = remove;
            startPoint = start;
            endPoint = end;
        }
    }

    public class CursorPoint
    {
        public int row;
        public int index;

        public CursorPoint(int row, int index)
        {
            this.row = row;
            this.index = index;
        }
    }
}