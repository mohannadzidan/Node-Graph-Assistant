using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGA.ChangesManagement
{
    public class NodeMovement : Change
    {
        Vector2 movementAmount;
        Node[] affectedElements;
        public NodeMovement(Vector2 movementAmount, Node[] affectedElements) {
            this.movementAmount = movementAmount;
            this.affectedElements = affectedElements;
        }

        public override void Apply()
        {
            foreach (Node node in affectedElements) node.Translate(movementAmount);
        }

        public override void Revert()
        {
            foreach (Node node in affectedElements) node.Translate(-movementAmount);
        }
    }
}
