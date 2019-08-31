using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGA.ChangesManagement
{
    class WireDisconnected : Change
    {
        Wire wire;
        public WireDisconnected(Wire wire)
        {
            this.wire = wire;
        }
        public override void Apply()
        {
            wire.Detach();

        }

        public override void Revert()
        {
            wire.Attach();
        }
    }
}
