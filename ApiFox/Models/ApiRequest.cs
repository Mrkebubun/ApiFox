using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ApiFox.Models
{
    public class ApiRequests
    {
         public ApiRequests()
        {
            this.DateCreated = DateTime.UtcNow;
        }
        public int Id { get; set; }
        public int ApiId { get; set; }
        public string ApiUrl { get; set; }
        public string Ip { get; set; }
        public DateTime DateCreated { get; set; }

        public virtual Apis Api { get; set; }
    }
}