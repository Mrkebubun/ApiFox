using ApiFox.DAL;
using ApiFox.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ApiFox.Controllers
{
  
    public class ImportedFileController : ApiController
    {
        ApifoxContext dbContext = new ApifoxContext();

        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        [HttpPut]
        public void Put(int id, MyApiDto value)
        {
            var api = dbContext.Apis.Single(p=>p.Id == id);
            if (api != null)
            {
                api.ApiName = value.ApiName;
                api.Owner = User.Identity.Name;
                dbContext.Entry(api).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();
            }

        }
         
        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        } 
    }
}