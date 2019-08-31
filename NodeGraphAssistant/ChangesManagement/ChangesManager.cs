using NGA.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGA.ChangesManagement
{
    public class ChangesManager
    {
        List<Change> undoStack = new List<Change>();
        List<Change> redoStack = new List<Change>();
        IDGenerator idGenerator = new IDGenerator();
        /// <summary>
        /// push a new change to undo stack (clears redo stack)
        /// </summary>
        /// <param name="change"></param>
        public void Push(Change change)
        {
            if (undoStack.Count >= 10)
            {
                undoStack.RemoveAt(0);
                undoStack.Add(change);
                redoStack.Clear();
            }
            else
            {
                undoStack.Add(change);
                redoStack.Clear();
            }
            Console.WriteLine("Undo:" + undoStack.Count + " Redo:" + redoStack.Count);
        }
        public bool Undo()
        {
            if (undoStack.Count == 0) return false;
            Change change = undoStack[undoStack.Count - 1];
            change.Revert(); // revert last change
            redoStack.Add(change); // push that change to redo stack
            undoStack.RemoveAt(undoStack.Count - 1); // remove the change from the undo stack
            Console.WriteLine("Undo:" + undoStack.Count + " Redo:" + redoStack.Count);
            return true;
        }
        public bool Redo()
        {

            if (redoStack.Count == 0) return false;
            Change change = redoStack[redoStack.Count - 1];
            change.Apply(); // revert last change
            undoStack.Add(change); // push that change to undo stack
            redoStack.RemoveAt(redoStack.Count - 1); // remove the change from the redo stack
            Console.WriteLine("Undo:" + undoStack.Count + " Redo:" + redoStack.Count);
            return true;
        }

    }
}
