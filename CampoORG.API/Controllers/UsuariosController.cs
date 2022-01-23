using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Description;
using CampoORG.API.Models;

namespace CampoORG.API.Controllers
{
    public class UsuariosController : ApiController
    {
        private CampoOrgConnection db = new CampoOrgConnection();

        // GET: api/Usuarios
        public IQueryable<Usuario> GetUsuarios()
        {
            //hilo
            Thread thread1 = new Thread(() => ContadoConultas(1));
            thread1.Start();
            thread1.Join();
            return db.Usuarios;
        }

        private void ContadoConultas(int id)
        {
            ContadorConsulta contadorConsulta = new ContadorConsulta();
            int conteo = 1;
            var query = (from consultas in db.ContadorConsultas where consultas.IdContadorConsultas == id select consultas).FirstOrDefault();
            query.Conteo += conteo;
            db.SaveChanges();

            //string desc = "una consulta";
            //contadorConsulta.Conteo = conteo;
            //contadorConsulta.Descripcion = desc;
            //db.ContadorConsultas.Add(contadorConsulta);
            //db.SaveChanges();
        }

        // GET: api/Usuarios/5
        [ResponseType(typeof(Usuario))]
        public IHttpActionResult GetUsuario(int id)
        {
            Usuario usuario = db.Usuarios.Find(id);
            if (usuario == null)
            {
                return NotFound();
            }
            Thread tconsultas = new Thread(() => ContadoConultas(2));
            tconsultas.Start();
            tconsultas.Join();
            Thread thread = new Thread(ReGeolocalizacio);
            thread.Start();
            thread.Join();
            Thread thread1 = new Thread(ContadorRoles);
            thread1.Start();
            return Ok(usuario);
        }

        // PUT: api/Usuarios/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutUsuario(int id, Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != usuario.Id)
            {
                return BadRequest();
            }

            db.Entry(usuario).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            Thread tconsultas = new Thread(() => ContadoConultas(4));
            tconsultas.Start();
            tconsultas.Join();

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Usuarios
        [ResponseType(typeof(Usuario))]
        public IHttpActionResult PostUsuario(Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Usuarios.Add(usuario);
            db.SaveChanges();
            Thread thread = new Thread(ReGeolocalizacio);
            thread.Start();
            thread.Join();
            Thread thread1 = new Thread(ContadorRoles);
            thread1.Start();
            Thread tconsultas = new Thread(() => ContadoConultas(6));
            tconsultas.Start();
            tconsultas.Join();

            return CreatedAtRoute("DefaultApi", new { id = usuario.Id }, usuario);
        }

        private void ContadorRoles()
        {
            ContadorRole contadorRole = new ContadorRole();
            int conteo = 1;
            contadorRole.Conteo = conteo;
            db.ContadorRoles.Add(contadorRole);
            db.SaveChanges();

        }

        private void ReGeolocalizacio()
        {
            Geolocalizacion geolocalizacion = new Geolocalizacion();
            var dispositivo = Dns.GetHostName();
            //IPAddress.Loopback.ToString();//Dns.GetHostAddresses(dispositivo).ToString();
            geolocalizacion.Dispositivo = dispositivo;
            geolocalizacion.DicrecionIP = Dns.GetHostByName(dispositivo).AddressList[0].ToString(); ;
            geolocalizacion.Hora = DateTime.Now.ToString();
            db.Geolocalizacions.Add(geolocalizacion);
            db.SaveChanges();
        }

        // DELETE: api/Usuarios/5
        [ResponseType(typeof(Usuario))]
        public IHttpActionResult DeleteUsuario(int id)
        {
            Usuario usuario = db.Usuarios.Find(id);
            if (usuario == null)
            {
                return NotFound();
            }

            db.Usuarios.Remove(usuario);
            db.SaveChanges();
            Thread tconsultas = new Thread(() => ContadoConultas(5));
            tconsultas.Start();
            tconsultas.Join();

            return Ok(usuario);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UsuarioExists(int id)
        {
            return db.Usuarios.Count(e => e.Id == id) > 0;
        }
    }
}