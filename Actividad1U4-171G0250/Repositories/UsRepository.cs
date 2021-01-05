using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Actividad1U4_171G0250.Models;

namespace Actividad1U4_171G0250.Repositories
{
    public class UsRepository: Repository<Usuario>
    {
        public UsRepository( correosusuariosContext cxc) : base(cxc) { }

    }
}
