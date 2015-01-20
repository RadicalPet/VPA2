using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using VPA2.Models;

namespace VPA2.Controllers
{
    public class DocumentsController : Controller
    {
        private ClientContext db = new ClientContext();

        private string filename { get; set; } 

        // GET: Documents
        public ActionResult Index()
        {
            return View(db.Documents.ToList());
        }

  
        private void GetAllDocs()
        {
            RestClient _Client = new RestClient("http://128.199.53.59");
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/octet-stream");
            request.AddParameter("update", "update");
            var filePath = @"D:\jagenau\ClientDocs\";
            RestResponse response = (RestResponse)_Client.Execute(request);
            
                if (response != null)
                {
                    try
                    {
                        string headContDisp = response.Headers[0].Value.ToString();

                        string filename = headContDisp.Substring(headContDisp.IndexOf("filename=") + 9).Replace("\"", "");
                        int clientId = Int32.Parse(filename.Substring(0, filename.IndexOf("_")));
                        string extension = filename.Substring(filename.LastIndexOf('.') + 1);

                        var responseObj = new Documents();
                        responseObj.documentName = filename;
                        responseObj.documentExtension = extension;
                        responseObj.clientId = clientId;
                           
        
                        byte[] fileBytes = response.RawBytes.ToArray();
                        System.IO.File.WriteAllBytes(filePath + filename, fileBytes);
                        db.Documents.Add(responseObj);
                        db.SaveChanges();

                        GetAllDocs();
                    }
                    catch
                    {
                        return;
                    }
            };
            
        }
        public ActionResult Update()
        {
            GetAllDocs();

            return Redirect("index");
            
        }

        // GET: Documents/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Documents documents = db.Documents.Find(id);
            if (documents == null)
            {
                return HttpNotFound();
            }
            return View(documents);
        }

        // GET: Documents/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Documents/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,clientId,documentName,documentExtension")] Documents documents)
        {
            if (ModelState.IsValid)
            {
                db.Documents.Add(documents);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(documents);
        }

        // GET: Documents/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Documents documents = db.Documents.Find(id);
            if (documents == null)
            {
                return HttpNotFound();
            }
            return View(documents);
        }

        // POST: Documents/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,clientId,documentName,documentExtension")] Documents documents)
        {
            if (ModelState.IsValid)
            {
                db.Entry(documents).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(documents);
        }

        // GET: Documents/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Documents documents = db.Documents.Find(id);
            if (documents == null)
            {
                return HttpNotFound();
            }
            return View(documents);
        }

        // POST: Documents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Documents documents = db.Documents.Find(id);
            db.Documents.Remove(documents);
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
