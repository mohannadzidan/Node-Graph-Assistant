using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGA.ChangesManagement
{

    public class NodeRemoved : Change
    {
        Node[] affectedElements;
        List<Wire> connections= new List<Wire>();
        public NodeRemoved(Node[] affectedElements) {
            this.affectedElements = affectedElements;
            foreach (Node node in affectedElements) {
                foreach (Drawable d in node.Elements) {
                    connections.AddRange(((NodeRing)d).Connections);
                }
            }
        }

        public override void Apply()
        {
            foreach (Node e in affectedElements) e.Detach();
            connections.ForEach((w) => w.Detach());
        }

        public override void Revert()
        {
            foreach (Node e in affectedElements) e.Attach();
            connections.ForEach((w) => w.Attach());

        }
    }
}
