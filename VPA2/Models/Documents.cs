using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VPA2.Models
{
    public class Documents
    {
        [Key]
        public int ID { get; set; }
        public int clientId { get; set; }
        public string documentName { get; set; }
        public string documentExtension { get; set; }
    }
}