using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
                    Id nodeId = idGenerator.Add(node);
                    Drawable[] nodeElements = node.Elements;
                    int[] nodeHolderElements = new int[nodeElements.Length];
                    for (int i = 0; i < nodeElements.Length; i++)
                    {
                        NodeRing nodeRing = (NodeRing)nodeElements[i];
                        Id nodeRingId = idGenerator.Add(nodeRing);
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
                    Id wireId = idGenerator.Add(wire);
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
        public class IDGenerator
        {
            List<Id> idsList = new List<Id>();
            public Id Add(Drawable reference)
            {
                Id id = new Id(idsList.Count, reference);
                idsList.Add(id);
                return id;
            }
            public Id Add(int value, Drawable reference)
            {
                Id id = new Id(value, reference);
                idsList.Add(id);
                return id;
            }
            public void Reset()
            {
                idsList.Clear();
            }
            public Id Find(Drawable reference)
            {
                return idsList.Find(id => id.reference == reference);
            }
            public Id Find(int value)
            {
                return idsList.Find(id => id.value == value);
            }

        }
        public class Id
        {
            public int value;
            public Drawable reference;
            public Id(int value, Drawable reference)
            {
                this.value = value;
                this.reference = reference;
            }
        }
    }
}