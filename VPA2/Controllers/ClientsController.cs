  using Newtonsoft.Json;
using RestSharp;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using VPA2.Models;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Encodings;



namespace VPA2.Controllers
{
    public class ClientsController : Controller
    {
        private ClientContext db = new ClientContext();

        // GET: Clients
        public ActionResult Index()
        {
            return View(db.Clients.ToList());
        }

        // GET: Clients/Details/5
        public ActionResult Details(int? id)
        {
           
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Clients clients  = db.Clients.Find(id);
            if (clients == null)
            {
                return HttpNotFound();
            }
            var client = new RestClient("http://178.62.247.139:9002");
            var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");
                request.RequestFormat = DataFormat.Json;

                request.AddParameter("firstname", clients.firstName);
                request.AddParameter("lastname", clients.lastName);
                request.AddParameter("email", clients.email);
                request.AddParameter("message", clients.message);

                RestResponse response = (RestResponse)client.Execute(request);
                var responseString = response.Content; 
                // raw content as string
                if (!string.IsNullOrEmpty(responseString))
                {
                    var responseObject = JsonConvert.DeserializeObject<Clients>(responseString);

                    var bytesFirstName = Convert.FromBase64String(responseObject.firstName);
                    var bytesLastName = Convert.FromBase64String(responseObject.lastName);
                    var bytesEmail = Convert.FromBase64String(responseObject.email);
                    var bytesMessage = Convert.FromBase64String(responseObject.message);

                    AsymmetricCipherKeyPair keyPair;

                    using (var reader = System.IO.File.OpenText(@"C:\Users\jagenau\Source\Repos\VPA3\VPA2\Assets\pyKey.pem")) 
                    // file containing RSA PKCS1 private key
                    keyPair = (AsymmetricCipherKeyPair)new PemReader(reader).ReadObject();

                    var decryptEngine = new Pkcs1Encoding(new RsaEngine());
                    decryptEngine.Init(false, keyPair.Private);

                    clients.firstName =  Encoding.UTF8.GetString(decryptEngine.ProcessBlock(bytesFirstName, 0, bytesFirstName.Length));
                    clients.lastName = Encoding.UTF8.GetString(decryptEngine.ProcessBlock(bytesLastName, 0, bytesLastName.Length));
                    clients.email = Encoding.UTF8.GetString(decryptEngine.ProcessBlock(bytesEmail, 0, bytesEmail.Length));
                    clients.message = Encoding.UTF8.GetString(decryptEngine.ProcessBlock(bytesMessage, 0, bytesMessage.Length));                    
                }
               
            return View(clients);
        }

        // GET: Clients/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Clients/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "clientId,firstName,lastName,email,message,uniqueToken")] Clients clients)
        {
            if (ModelState.IsValid)
            {
                db.Clients.Add(clients);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(clients);
        }

        // GET: Clients/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Clients clients = db.Clients.Find(id);
            if (clients == null)
            {
                return HttpNotFound();
            }
            return View(clients);
        }

        // POST: Clients/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "clientId,firstName,lastName,email,message,uniqueToken")] Clients clients)
        {
            if (ModelState.IsValid)
            {
                db.Entry(clients).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(clients);
        }

        // GET: Clients/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Clients clients = db.Clients.Find(id);
            if (clients == null)
            {
                return HttpNotFound();
            }
            return View(clients);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Clients clients = db.Clients.Find(id);
            db.Clients.Remove(clients);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
