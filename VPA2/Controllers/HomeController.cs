﻿using RestSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VPA2.Models;
using System.IO;
using System.Web.Caching;
using System.Text;
using System.Drawing;


namespace VenalPenal.Controllers
{
    public class HomeController : Controller
    {
        private ClientContext db = new ClientContext();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Hurr - we lawyers durr";

            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Contact(Clients c)
        {
            if (ModelState.IsValid)
            {
                var client = new RestClient("http://128.199.53.59");
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");
                request.RequestFormat = DataFormat.Json;

                request.AddParameter("firstname", c.firstName);
                request.AddParameter("lastname", c.lastName);
                request.AddParameter("email", c.email);
                request.AddParameter("message", c.message);

                RestResponse response = (RestResponse)client.Execute(request);
                var responseString = response.Content; // raw content as string
                if (!string.IsNullOrEmpty(responseString))
                {
                    var responseObject = JsonConvert.DeserializeObject<Clients>(responseString);
                    db.Clients.Add(responseObject);
                    db.SaveChanges();
                    @ViewBag.Success = response.Content;
                }
                return View();
            }
            return View();
        }
        public ActionResult Documents()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Documents(HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                if (file == null)
                {
                    ModelState.AddModelError("File", "Please Upload Your file");
                }
                else if (file.ContentLength > 0)
                {
                    string[] AllowedFileExtensions = new string[] { ".jpg", ".gif", ".png", ".pdf", ".txt", ".docx", ".pptx", ".zip", ".tar", ".tar.gz", ".odt", ".odp" };

                    if (!AllowedFileExtensions.Contains(file.FileName.Substring(file.FileName.LastIndexOf('.'))))
                    {
                        ModelState.AddModelError("File", "Please file of type: " + string.Join(", ", AllowedFileExtensions));
                    }
                    else
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var fileStream = new Byte[Request.Files[0].ContentLength];
                        var inputStream = file.InputStream;
                        System.IO.Stream MyStream;
                        int FileLen = file.ContentLength;
                        byte[] input = new byte[FileLen];
                        // Initialize the stream.
                        MyStream = file.InputStream;
                        // Read the file into the byte array.
                        MyStream.Read(input, 0, FileLen);

                        var client = new RestClient("http://128.199.53.59");
                        var request = new RestRequest(Method.POST);
                        var contentDisposition = "attachment; filename=\"" + fileName + "\"; filename*=UTF-8''" + Uri.EscapeDataString(fileName);
                        request.AddHeader("Content-Type", "multipart/formdata");
                        request.AddHeader("Content-Disposition", contentDisposition);
                        request.AddFile("file", input, fileName);
                        RestResponse response = (RestResponse)client.Execute(request);
                        ViewBag.Message = "File uploaded successfully";
                        ModelState.Clear();
                    }
                }
            }
            return View();
        }

    }
}