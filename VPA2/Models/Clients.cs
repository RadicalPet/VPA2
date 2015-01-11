using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace VPA2.Models
{
    public class Clients
    {
        [Key]
        public int clientId { get; set; }
        [Required(ErrorMessage = "First Name is required")]
        public string firstName { get; set; }
        [Required(ErrorMessage = "Last Name is required")]
        public string lastName { get; set; }
        public string email { get; set; }
        public string message { get; set; }
        public string uniqueToken { get; set; }
    }
}