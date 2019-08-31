using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGA.ChangesManagement
{
    class NodeAdd : Change
    {
        float x, y;
        Node node;
        List<Wire> connections = new List<Wire>();

        public NodeAdd(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        public override void Apply()
        {
            if (node == null)
            {
                node = new Node(x, y);
                Program.canvas.Drawbles.Add(node);
            }
            else {
                node.Attach();
                connections.ForEach((w) => w.Attach());
            }
        }

        public override void Revert()
        {
            if (node != null)
            {
                foreach (Drawable d in node.Elements)
                {
                    connections.AddRange(((NodeRing)d).Connections);
                }
                node.Detach();
            }
        }
    }
}
