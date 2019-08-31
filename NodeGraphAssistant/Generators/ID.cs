using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGA.Generators
{
    public class ID
    {
        public int value;
        public Drawable reference;
        public ID(int value, Drawable reference)
        {
            this.value = value;
            this.reference = reference;
        }
    }
}
