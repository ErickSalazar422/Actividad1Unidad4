using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Actividad1U4_171G0250.Models;

namespace Actividad1U4_171G0250.Repositories
{
    public class Repository<T> where T : class
    {
        public correosusuariosContext Context { get; set; }
        public Repository(correosusuariosContext cxc)
        {
            Context = cxc;
        }
        public Usuario GetUsByCorreo(string correo)
        {
            return Context.Usuario.FirstOrDefault(x => x.Correo.ToUpper() == correo.ToUpper());
        }
        public virtual void Insert(Usuario us)
        {
            if (validar(us))
            {
                Context.Add(us);
                Context.SaveChanges();
            }
        }
        public virtual void Update(Usuario us)
        {
            if (validar(us))
            {
                Context.Update(us);
                Context.SaveChanges();
            }
        }
        public virtual void Delete(T us)
        {
            Context.Remove(us);
            Context.SaveChanges();
        }
        public bool validar(Usuario us)
        {
            if (string.IsNullOrWhiteSpace(us.Usuario1))
                throw new Exception("Introduzca el nombre de usuario");
            if (string.IsNullOrWhiteSpace(us.Correo))
                throw new Exception("Introduzca su correo electrónico");
            if (string.IsNullOrWhiteSpace(us.Contrasena))
                throw new Exception("Escriba su contraseña");
            if (Context.Usuario.Any(x => x.Correo.ToUpper() == us.Correo.ToUpper() && x.Id != us.Id))
                throw new Exception("Este correo ya está vinculado a un usuario");
            return true;
        }

    }
}
