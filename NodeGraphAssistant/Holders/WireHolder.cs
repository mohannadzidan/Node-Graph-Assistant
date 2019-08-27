using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGA.Holders
{
    public class WireHolder : Holder<Wire>
    {
        public int firstRingId, secondRingId;

        Collider start, end;
        public WireHolder(int id, int firstRingId, int secondRingId) : base(-1, id)
        {
            this.firstRingId = firstRingId;
            this.secondRingId = secondRingId;
        }
      
        public override Wire Release(CanvasHolder holder, CanvasHolder.IDGenerator generator)
        {
            try
            {
                start = generator.Find(firstRingId).reference.Collider;
                end = generator.Find(secondRingId).reference.Collider;

            }
            catch (NullReferenceException)
            {
                throw new Exception("couldn't find (start) or (end) with the stored ids!");
            }

            Wire generatedWire = new Wire(start, end);
            generatedWire.ReconstructCollider();
            ((NodeRing)start.Drawable).Connections.Add(generatedWire);
            ((NodeRing)end.Drawable).Connections.Add(generatedWire);
            generator.Add(id, generatedWire);
            return generatedWire;
        }
    }
}
