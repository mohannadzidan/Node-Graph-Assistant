using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGA.ChangesManagement
{
    public abstract class Change
    {
        public abstract void Revert();
        public abstract void Apply();
    }
}
