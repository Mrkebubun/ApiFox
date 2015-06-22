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
    [RoutePrefix("apis")]
    public class ApisController : ApiController
    {
        ApifoxContext dbContext = new ApifoxContext();
        // GET api/<controller>z
        [Route("apilist")]
        public IEnumerable<MyApiDto> Get()
        {
            IOrderedQueryable<Apis> list = dbContext.Apis.Where(a=>a.Owner == User.Identity.Name && !(a.IsDeleted.HasValue && a.IsDeleted.Value)).OrderByDescending(a=>a.DateCreated);
            return list.AsEnumerable().Select(api => new MyApiDto(api));
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        [HttpPost]
        [Route("apilist")]
        public void ConvertGSheet([FromBody]string value)
        {

        }


        // DELETE api/<controller>/5
        [HttpPut]
        [Route("apis/delete/{apiId}")]
        public HttpResponseMessage Delete(int apiId)
        {
            var api = dbContext.Apis.Single(p => p.Id == apiId);
            if (api != null)
            {
                try
                {
                    api.IsDeleted = true;
                    dbContext.Entry(api).State = System.Data.Entity.EntityState.Modified;
                    dbContext.SaveChanges();
                  
                    return Request.CreateResponse(HttpStatusCode.Accepted, apiId);
                }
                catch(Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            return Request.CreateResponse(HttpStatusCode.Unauthorized, apiId);
        }

        // PUT api/<controller>/5
        [HttpPut]
        [Route("apis/{id}")]
        public void Put(int id, MyApiDto value)
        {
            var api = dbContext.Apis.Single(p => p.Id == id);
            if (api != null)
            {
                api.ApiName = value.ApiName;
                api.ApiUrl = value.ApiUrl;

                dbContext.Entry(api).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();
            }

        }

    }
}