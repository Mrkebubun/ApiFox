using ApiFox.DAL;
using ApiFox.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using ApiFox.Extensions;
using System.Data;
using System.Web.Http.Cors;

namespace ApiFox.Controllers
{
    [EnableCors("*", "*", "*")]
    public class ClientApiController : ApiController
    {
        ApifoxContext dbContext = new ApifoxContext();
 
        // GET api/<controller>
        [Route("api/{apiname?}/{*secondname?}")]
        public HttpResponseMessage Get(string apiname, string secondname)
        {
            return GetApiJson(apiname, secondname);
        }
        // GET api/<controller>
        [Route("api/{apiname?}/row={row}")]
        public HttpResponseMessage Get(string apiname, int row, bool special = true)
        {
            return GetApiJson(apiname, null, row);
        }

        // GET api/<controller>
        [Route("api/{apiname?}")]
        public HttpResponseMessage Get(string apiname)
        {
            return GetApiJson(apiname, null);
        }
       
        private HttpResponseMessage GetApiJson(string apiname, string secondname, int row = 0)
        {
            //TODO check for the owner
            var myApi = dbContext.Apis.SingleOrDefault(
                a => a.ApiUrl.Equals(apiname + (string.IsNullOrEmpty(secondname) ? string.Empty : "/" + secondname)));

            string jsonPath = HttpContext.Current.Server.MapPath("~/App_Data/") + Path.GetFileNameWithoutExtension(myApi!=null && myApi.FileSource!= null ?myApi.FileSource.FileName: string.Empty) + ".json";

            if (!File.Exists(jsonPath))
                return this.Request.CreateResponse(HttpStatusCode.NotFound);
            string yourJson = File.ReadAllText(jsonPath);

            var dt = Helpers.GetDataTableFromJson(yourJson);
            if (dt != null)
            {
                var rows = dt.Rows;
                //TODO skip x rows or select row x
                if (row > 0 && rows.Count >= row)
                {

                    var tempDt = dt.Clone();
                    tempDt.Rows.Add(tempDt.NewRow().ItemArray = rows[row - 1].ItemArray.Clone() as object[]);
                    yourJson = Helpers.GetJson(tempDt);
                }
            }
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(yourJson, Encoding.UTF8, "application/json");

            Helpers.LogApiRequest(myApi.Id, myApi.ApiUrl, Request.GetClientIpAddress());
            return response;
        }

        // GET api/<controller>
        [Route("api/stats")]
        public HttpResponseMessage GetStats()
        {
            var apiRequests = from a in dbContext.ApiRequests
                              join b in dbContext.Apis on a.Api.Id equals b.Id
                              where b.Owner == User.Identity.Name
                              select a;
            var stats = new Stats(apiRequests, null);

            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(System.Web.Helpers.Json.Encode(stats.ToDto()), Encoding.UTF8, "application/json");
            return response;
        }

        // GET api/<controller>
        [Route("api/stats/{apiurl?}")]
        public HttpResponseMessage GetStats(string apiurl)
        {
            var apiRequests = from a in dbContext.ApiRequests
                              join b in dbContext.Apis on a.Api.Id equals b.Id
                              where b.Owner == User.Identity.Name
                              select a;
            var stats = new Stats(apiRequests, apiurl);

            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(System.Web.Helpers.Json.Encode(stats.ToDto()), Encoding.UTF8, "application/json");
            return response;
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
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}