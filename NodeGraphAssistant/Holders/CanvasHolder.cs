using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NGA.Generators;
using SharpDX;

namespace NGA.Holders
{
    public class CanvasHolder
    {
        public Vector2 translation;
        public NodeHolder[] nodeHolders;
        public NodeRingHolder[] nodeRingHolders;
        public WireHolder[] wireHolders;
        public CanvasHolder() { }
        public CanvasHolder(Canvas canvas)
        {
            var canvasDrawables = canvas.Drawbles;
            translation = canvas.Translation;
            List<NodeHolder> nodeHoldersList = new List<NodeHolder>();
            List<NodeRingHolder> nodeRingHoldersList = new List<NodeRingHolder>();
            List<WireHolder> wireHoldersList = new List<WireHolder>();
            IDGenerator idGenerator = new IDGenerator();
            foreach (Drawable d in canvasDrawables)
            {
                if (d.GetType() == typeof(Node))
                {
                    Node node = (Node)d;
                    ID nodeId = idGenerator.Add(node);
                    List< Drawable> nodeElements = node.Elements;
                    int[] nodeHolderElements = new int[nodeElements.Count];
                    for (int i = 0; i < nodeElements.Count; i++)
                    {
                        NodeRing nodeRing = (NodeRing)nodeElements[i];
                        ID nodeRingId = idGenerator.Add(nodeRing);
                        nodeHolderElements[i] = nodeRingId.value;
                        nodeRingHoldersList.Add(new NodeRingHolder(nodeRingId.value, nodeId.value, nodeRing.Title, nodeRing.Color, nodeRing.Direction));
                    }
                    nodeHoldersList.Add(new NodeHolder(nodeId.value, node.BoundingBox.X, node.BoundingBox.Y, node.BoundingBox.Width, node.Title, nodeHolderElements));
                }
               
            }
            foreach (Drawable d in canvasDrawables) {
                if (d.GetType() == typeof(Wire))
                {
                    Wire wire = (Wire)d;
                    ID wireId = idGenerator.Add(wire);
                    wireHoldersList.Add(new WireHolder(wireId.value, idGenerator.Find(wire.Start.Drawable).value, idGenerator.Find(wire.End.Drawable).value));
                }
            }
            nodeHolders = nodeHoldersList.ToArray();
            nodeRingHolders = nodeRingHoldersList.ToArray();
            wireHolders = wireHoldersList.ToArray();

        }
        public List<Drawable> Release(Canvas canvas) {
            canvas.Translation = translation;
            List<Drawable> drawables = new List<Drawable>();
            IDGenerator generator = new IDGenerator();
            foreach(NodeHolder nodeHolder in nodeHolders)
            {
                drawables.Add(nodeHolder.Release(this, generator));
            }
            foreach (WireHolder wireHolder in wireHolders)
            {
                drawables.Add(wireHolder.Release(this, generator));
            }
            canvas.Drawbles.Clear();
            canvas.Drawbles.AddRange(drawables);
            Program.MarkCanvasDirty();
            return drawables;
        }
        public static CanvasHolder FromJson(string json) {
            return JsonConvert.DeserializeObject<CanvasHolder>(json);
        }
        public string ToJson() {
            return JsonConvert.SerializeObject(this);
        }
       
    }
}