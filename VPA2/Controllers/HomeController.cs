using RestSharp;
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
using System.Net.Mail;
using System.Net;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;


namespace VenalPenal.Controllers
{
    public class HomeController : Controller
    {
        private ClientContext db = new ClientContext();
        public void sendMailTo(string emailEmp, string subject, string body)
        {
            MailMessage message = new System.Net.Mail.MailMessage(); 
            string fromEmail = "sahara.braun@gmail.com";
            string fromPW = "M0therF()ckingGoog1e";
            string toEmail = emailEmp;
            message.From = new MailAddress(fromEmail);
            message.To.Add(toEmail);
            message.Subject = subject;
            message.Body = body;
            message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.EnableSsl = true;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(fromEmail, fromPW);

            smtpClient.Send(message.From.ToString(), message.To.ToString(), 
            message.Subject, message.Body);   
        }
        public string retrievePlaintextEmail(string encEmail)
        {

            var client = new RestClient("http://178.62.247.139:9002");
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.RequestFormat = DataFormat.Json;

            request.AddParameter("email", encEmail);

            RestResponse response = (RestResponse)client.Execute(request);
            var responseString = response.Content;
            // raw content as string
            if (!string.IsNullOrEmpty(responseString))
            {
                var responseObject = JsonConvert.DeserializeObject<Clients>(responseString);
                var bytesEmail = Convert.FromBase64String(responseObject.email);

                AsymmetricCipherKeyPair keyPair;

                using (var reader = System.IO.File.OpenText(@"C:\Users\jagenau\Source\Repos\VPA3\VPA2\Assets\pyKey.pem"))
                    // file containing RSA PKCS1 private key
                    keyPair = (AsymmetricCipherKeyPair)new PemReader(reader).ReadObject();

                var decryptEngine = new Pkcs1Encoding(new RsaEngine());
                decryptEngine.Init(false, keyPair.Private);

                return Encoding.UTF8.GetString(decryptEngine.ProcessBlock(bytesEmail, 0, bytesEmail.Length));
            }
            return "failure";
        }
   
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
                    sendMailTo("ominousomnivore@googlemail.com", "New Contact signed up", "Please look into their message at your earliest conveniance");
                    sendMailTo(retrievePlaintextEmail(responseObject.email), "Your Document Upload Token", "You can use this unique token to upload relevant documents to our servers: " + responseObject.uniqueToken);
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
        public ActionResult Documents(HttpPostedFileBase file, DocUpload d)
        {
            if (ModelState.IsValid) 
            {
                if (file == null)
                {
                    ModelState.AddModelError("File", "Please choose a file to upload.");
                }
                if (d.token == null)
                {
                    ModelState.AddModelError("Token", "Please enter your token.");
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

                        var clientId = DocUpload.getUser(d.token);

                        if (clientId == -1)
                        {
                            ModelState.AddModelError("Token", "Token not found.");
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
                            request.AddParameter("clientId", clientId);
                            request.AddParameter("fileName", fileName);
                            request.AddFile("file", input, fileName, "multipart/formdata");
                            RestResponse response = (RestResponse)client.Execute(request);
                            ViewBag.Message = "File uploaded successfully";
                            ModelState.Clear();
                        }
                    }
                }
            }
            return View();
        }

    }
}
