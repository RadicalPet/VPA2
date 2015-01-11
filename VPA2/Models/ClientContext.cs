using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace VPA2.Models
{
    public class ClientContext : DbContext
    {
        public ClientContext() : base("clients")
        {

        }
        public DbSet<Clients> Clients { get; set; }
        public DbSet<Documents> Documents { get; set; }
     }
}