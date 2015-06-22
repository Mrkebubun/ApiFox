using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ApiFox.Models
{
    public class Apis
    {
        public Apis()
        {
            this.DateCreated = DateTime.UtcNow;
        }
        public int Id { get; set; }
        public string ApiName { get; set; }
        public string ApiUrl { get; set; }
        public string Owner { get; set; }
        public DateTime DateCreated { get; set; }

        public bool? IsDeleted { get; set; }

        public virtual ImportedFile FileSource { get; set; }
    }
}