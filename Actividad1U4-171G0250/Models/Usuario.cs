using System;
using System.Collections.Generic;

namespace Actividad1U4_171G0250.Models
{
    public partial class Usuario
    {
        public int Id { get; set; }
        public string Usuario1 { get; set; }
        public string Correo { get; set; }
        public string Contrasena { get; set; }
        public int? ClaveAct { get; set; }
        public ulong? Activo { get; set; }
    }
}
