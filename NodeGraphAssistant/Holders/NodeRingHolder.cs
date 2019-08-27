using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
namespace NGA.Holders
{
    public class NodeRingHolder : Holder<NodeRing>
    {
        public string title;
        public Color4 color;
        public Direction direction;
        Node parent;

        public NodeRingHolder(int id, int parentId, string title, Color4 color, Direction direction) : base(parentId, id)
        {
            this.title = title;
            this.color = color;
            this.direction = direction;
        }
        public void SetParent(Node node)
        {
            parent = node;
        }
        public override NodeRing Release(CanvasHolder holder, CanvasHolder.IDGenerator generator)
        {
            if (parent == null) throw new ArgumentNullException("parent must be set using SetParent() before release!");
            NodeRing generatedNodeRing = new NodeRing(parent, title, color, direction);
            generator.Add(id, generatedNodeRing);
            return generatedNodeRing;
        }
    }
}
