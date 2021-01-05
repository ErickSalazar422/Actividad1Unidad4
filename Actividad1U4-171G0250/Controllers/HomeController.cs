using Actividad1U4_171G0250.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Actividad1U4_171G0250.Repositories;
using Actividad1U4_171G0250.Helpers;
using System.Net.Mail;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace Actividad1U4_171G0250.Controllers
{
    public class HomeController : Controller
    {
      
        public IWebHostEnvironment Environment { get; set; }
        public HomeController(IWebHostEnvironment env)
        {
            Environment = env;
        }
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Registro(Usuario us, string contra, string confcontra)
        {
            correosusuariosContext Context = new correosusuariosContext();
            Repository<Usuario> repos = new Repository<Usuario>(Context);
            try
            {
                if (Context.Usuario.Any(x => x.Correo == us.Correo))
                {
                    ModelState.AddModelError("", "Este correo se encuentra registrado");
                    return View(us);
                }
                else
                {
                    if (contra == confcontra)
                    {
                        us.Contrasena = HashinHelper.GetHash(contra);
                        us.ClaveAct = ClaveHelper.ClaveActivacion();
                        us.Activo = 0;
                        repos.Insert(us);

                        MailMessage message = new MailMessage();
                        message.From = new MailAddress("sistemascomputacionales7g@gmail.com", "PelisPlus");
                        message.To.Add(us.Correo);
                        message.Subject = "Correo de activación envíado";

                        string mensaje = System.IO.File.ReadAllText(Environment.WebRootPath + "/Clave.html");
                        message.Body = mensaje.Replace("##Clave##", us.ClaveAct.ToString());
                        message.IsBodyHtml = true;

                        SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                        client.EnableSsl = true;
                        client.UseDefaultCredentials = false;
                        client.Credentials = new NetworkCredential("sistemascomputacionales7g@gmail.com", "sistemas7g");
                        client.Send(message);
                        return RedirectToAction("ActivacionDeCuenta");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Las contraseñas no coinciden");
                        return View(us);
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(us);
            }
        }


        public IActionResult ActivacionDeCuenta()
        {
            return View();
        }
        [HttpPost]
        public IActionResult ActivacionDeCuenta(int clave)
        {
           correosusuariosContext Context  = new correosusuariosContext();
            UsRepository repos = new UsRepository(Context);
            var usuario = Context.Usuario.FirstOrDefault(x => x.ClaveAct == clave);
            if (usuario != null && usuario.Activo == 0)
            {
                var cla = usuario.ClaveAct;
                if (clave== cla)
                {
                    usuario.Activo = 1;
                    repos.Update(usuario);
                    return RedirectToAction("InicioDeSesion");
                }
                else
                {
                    ModelState.AddModelError("", "No ha introducido la clave correcta.");
                    return View();
                }

            }
            else
            {
                ModelState.AddModelError("", "Usuario no encontrado.");
                return View();
            }
        }




        [AllowAnonymous]
        public IActionResult InicioDeSesion()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> InicioDeSesion(Usuario us, bool recordar)
        {
            correosusuariosContext Context = new correosusuariosContext();
            UsRepository repos = new UsRepository(Context);
            var usuario = repos.GetUsByCorreo(us.Correo);
            if (usuario != null && HashinHelper.GetHash(us.Contrasena) == usuario.Contrasena)
            {
                if (usuario.Activo == 1)
                {
                    List<Claim> info = new List<Claim>();
                    info.Add(new Claim(ClaimTypes.Name, $"{usuario.Usuario1}"));
                    info.Add(new Claim(ClaimTypes.Role, "UsuarioActivo"));
                    info.Add(new Claim("Nombre", usuario.Usuario1));
                    info.Add(new Claim("Correo electronico", usuario.Correo));
                    var claimidentity = new ClaimsIdentity(info, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimprincipal = new ClaimsPrincipal(claimidentity);
                    if (recordar == true)
                    {
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimprincipal, new AuthenticationProperties
                        { IsPersistent = true });
                    }
                    else
                    {
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimprincipal, new AuthenticationProperties
                        { IsPersistent = false });
                    }
                    return RedirectToAction("SesionIniciada");
                }
                else
                {
                    ModelState.AddModelError("", "Correo electronico y/o  contraseña erroneos");
                    return View();
                }
            }
            else
            {
                ModelState.AddModelError("", "Usuario no registrado");
                return View();
            }
        }


        [Authorize]
        public IActionResult SesionIniciada()
        {
            return View();
        }



        [AllowAnonymous]
        public async Task<IActionResult> CierreDeSesion()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index");
        }



        [Authorize]
        public IActionResult CambioDeContraseña()
        {
            return View();
        }



        [HttpPost]
        [Authorize]
        public IActionResult CambioDeContraseña(string correo, string contra, string confcontra)
        {
            correosusuariosContext Context = new correosusuariosContext();
            UsRepository repos = new UsRepository(Context);
            var us = repos.GetUsByCorreo(correo);
            try
            {
                if (contra == confcontra)
                {
                    us.Contrasena = HashinHelper.GetHash(contra);
                    if (us.Contrasena == contra)
                    {
                        ModelState.AddModelError("", "La nueva contraseña debe ser distinta a la actúal");
                        return View(contra);
                    }
                    else
                    {
                        repos.Update(us);
                        return RedirectToAction("SesionIniciada");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Las contraseñas no coinciden.");
                    return View();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(contra, confcontra);
            }
        }




        public IActionResult RecuperarContraseña()
        {
            return View();
        }
        [HttpPost]
        public IActionResult RecuperarContraseña(string correo)
        {
            try
            {
                correosusuariosContext Context = new correosusuariosContext();
                UsRepository repos = new UsRepository(Context);
                var us = repos.GetUsByCorreo(correo);
                if (us != null)
                {
                    var contra = ClaveHelper.ClaveActivacion();
                    MailMessage message = new MailMessage();
                    message.From = new MailAddress("sistemascomputacionales7g@gmail.com", "PelisPlus");
                    message.To.Add(correo);
                    message.Subject = "Se ha enviado un correo con una contraseña temporal para iniciar sesión";
                    message.Body = $"Esta contraseña solo sirve una única vez: {contra}";

                    SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential("sistemascomputacionales7g@gmail.com", "sistemas7g");
                    client.Send(message);
                    us.Contrasena = HashinHelper.GetHash(contra.ToString());
                    repos.Update(us);
                    return RedirectToAction("InicioDeSesion");
                }
                else
                {
                    ModelState.AddModelError("", "Este correo no se encuentra registrado.");
                    return View();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View((object)correo);
            }
        }



        [HttpPost]
        public IActionResult Eliminar(string correo)
        {
            correosusuariosContext Context = new correosusuariosContext();
            UsRepository repos = new UsRepository(Context);
            var us = repos.GetUsByCorreo(correo);

            if (us != null)
            {
                HttpContext.SignOutAsync();
                repos.Delete(us);
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "El usuario no se ha podido eliminar.");
                return RedirectToAction("SesionIniciada");
            }
        }


        public IActionResult Privacy()
        {
            return View();
        }

       
    }
}
