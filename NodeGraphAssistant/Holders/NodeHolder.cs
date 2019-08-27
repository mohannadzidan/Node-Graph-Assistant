using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace NGA.Holders
{
    public class NodeHolder : Holder<Node>
    {
        public string title;
        public int[] elements;
        public float x, y, width;

        public NodeHolder(int id, float x, float y, float width, string title, int[] elements) : base(-1, id)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.title = title;
            this.elements = elements;
        }

        public override Node Release(CanvasHolder canvasHolder, CanvasHolder.IDGenerator generator)
        {
            Node generatedNode = new Node(x, y, width);
            generatedNode.Title = title;
            foreach (NodeRingHolder nodeRingHolder in canvasHolder.nodeRingHolders)
            {
                if (nodeRingHolder.parentId == this.id)
                {
                    nodeRingHolder.SetParent(generatedNode);
                    NodeRing generatedNodeRing = nodeRingHolder.Release(canvasHolder, generator);
                    generatedNode.AddElement(generatedNodeRing);
                }
            }
            generator.Add(id, generatedNode);
            return generatedNode;
        }
    }
}