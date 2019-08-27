using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace NGA.Holders
{
    public abstract class Holder<T>
    {
        public int id;
        public int parentId;

        protected Holder(int parentId, int id)
        {
            this.parentId = parentId;
            this.id = id;
        }

        public abstract T Release(CanvasHolder holder, CanvasHolder.IDGenerator generator);

    }

}