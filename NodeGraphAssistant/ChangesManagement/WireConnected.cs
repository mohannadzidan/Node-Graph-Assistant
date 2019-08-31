using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGA.ChangesManagement
{
    class WireConnected : Change
    {

        Wire wire;
        public WireConnected(Wire wire) {
            this.wire = wire;
        }
        public override void Apply()
        {
            wire.Attach();
        }

        public override void Revert()
        {
            wire.Detach();
        }
    }
}
