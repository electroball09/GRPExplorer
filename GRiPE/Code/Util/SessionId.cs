using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GRiPE.Code.Util
{
    public class SessionId
    {
        public Guid Guid { get; }

        public SessionId()
        {
            Guid = Guid.NewGuid();
        }

        public override string ToString() => Guid.ToString();
    }
}
