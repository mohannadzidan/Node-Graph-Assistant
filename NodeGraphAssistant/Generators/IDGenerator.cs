using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGA.Generators
{
    public class IDGenerator
    {
        List<ID> idsList = new List<ID>();
        public ID Add(Drawable reference)
        {
            ID id = new ID(idsList.Count, reference);
            idsList.Add(id);
            return id;
        }
        public ID Add(int value, Drawable reference)
        {
            ID id = new ID(value, reference);
            idsList.Add(id);
            return id;
        }
        public void Reset()
        {
            idsList.Clear();
        }
        public ID Find(Drawable reference)
        {
            return idsList.Find(id => id.reference == reference);
        }
        public ID Find(int value)
        {
            return idsList.Find(id => id.value == value);
        }
        public void RemoveAll(Predicate<ID> predicate) {
            idsList.RemoveAll(predicate);
        }
    }
   
}
