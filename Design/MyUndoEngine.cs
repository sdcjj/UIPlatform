using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Design;

namespace UIPlatform
{
    public class MyUndoEngine : UndoEngine
    {
        private Stack<UndoEngine.UndoUnit> undoStack = new Stack<UndoEngine.UndoUnit>();
        private Stack<UndoEngine.UndoUnit> redoStack = new Stack<UndoEngine.UndoUnit>();

        public MyUndoEngine(IServiceProvider provider) : base(provider) { }

        public bool EnableUndo
        {
            get { return undoStack.Count > 0; }
        }

        public bool EnableRedo
        {
            get { return redoStack.Count > 0; }
        }

        public void Undo()
        {
            if (undoStack.Count > 0)
            {
                try
                {
                    UndoEngine.UndoUnit unit = undoStack.Pop();
                    unit.Undo();
                    redoStack.Push(unit);
                }
                catch { }
            }
        }

        public void Redo()
        {
            if (redoStack.Count > 0)
            {
                try
                {
                    UndoEngine.UndoUnit unit = redoStack.Pop();
                    unit.Undo();
                    undoStack.Push(unit);
                }
                catch { }
            }
        }

        protected override void AddUndoUnit(UndoEngine.UndoUnit unit)
        {
            undoStack.Push(unit);
        }
    }
}
