using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.ServiceModel.Channels;

namespace ApiFox.Extensions
{
    public static class DataColumnExtensions
    {
        public static bool Exists(this DataColumnCollection dataColumnCollection, DataColumn dataColumn)
        {
            bool exists = false;
            foreach (DataColumn item in dataColumnCollection)
            {
                if (item.ColumnName.Equals(dataColumn.ColumnName, StringComparison.InvariantCultureIgnoreCase))
                {
                    exists = true;
                    break;
                }
            }
            return exists;
        }
    }

   public static class Common
   {
       public static IPAddress GetClientIpAddress(this HttpRequestMessage request)
       {
           if (request.Properties.ContainsKey("MS_HttpContext"))
           {
               return IPAddress.Parse(((HttpContextBase)request.Properties["MS_HttpContext"]).Request.UserHostAddress);
           }
           if (request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
           {
               return IPAddress.Parse(((RemoteEndpointMessageProperty)request.Properties[RemoteEndpointMessageProperty.Name]).Address);
           }
           throw new Exception("Client IP Address Not Found in HttpRequest");
       }

   }
}