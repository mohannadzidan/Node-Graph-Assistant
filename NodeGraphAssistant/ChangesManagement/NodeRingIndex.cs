using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGA.ChangesManagement
{
    class NodeRingIndex : Change
    {
        NodeRing nodeRing;
        int newIndex;
        int oldIndex;
        public NodeRingIndex(NodeRing nodeRing, int newIndex){
            this.nodeRing = nodeRing;
            this.newIndex = newIndex;
        }

        public override void Apply()
        {
            oldIndex = ((Node)nodeRing.Parent).Elements.IndexOf(nodeRing);
            ((Node)nodeRing.Parent).SetElementIndex(nodeRing, newIndex);
        }

        public override void Revert()
        {
            ((Node)nodeRing.Parent).SetElementIndex(nodeRing, oldIndex);
        }
    }
}
