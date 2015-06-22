using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ApiFox.Models
{
    public class ImportedFile
    {
        public ImportedFile()
        {
            this.DateCreated = DateTime.Now;
        }
        public int Id { get; set; }
        public string FileName { get; set; }
        public DateTime DateCreated { get; set; }
        public string Ip { get; set; }
        public bool IsRejected { get; set; }
    }
}