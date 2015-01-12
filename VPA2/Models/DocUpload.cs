using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using VPA2.Models;

namespace VPA2.Models
{
    public class DocUpload
    {
       
        public string token { get; set; }
        public string clientId { get; set; }

        public static int getUser(string token)
        {
            ClientContext db = new ClientContext();
            int clientId = -1;
            var query = db.Clients.Where(a => a.uniqueToken == token).FirstOrDefault();
            if (query != null)
            {
                clientId = query.clientId;
            }
            return clientId;
        }
    }
}