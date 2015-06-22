using Microsoft.VisualBasic.FileIO;
using ApiFox.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Collections.Specialized;
using System.Xml.Linq;
using ApiFox.Extensions;
using System.Threading.Tasks;
using ApiFox.DAL;
using ApiFox.Models;
using System.Diagnostics;
using  System.Data.Entity;
using System.Net;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;

namespace ApiFox
{
    public class Helpers
    {
        public static DataTable GetDataTabletFromCSVFile(string csv_file_path)
        {
            DataTable csvData = new DataTable();
            //try
            //{
            using (TextFieldParser csvReader = new TextFieldParser(csv_file_path))
            {
                csvReader.SetDelimiters(new string[] { "," });
                csvReader.HasFieldsEnclosedInQuotes = true;
                string[] colFields = csvReader.ReadFields();
                foreach (string column in colFields)
                {
                    DataColumn datacolumn = new DataColumn(column);
                    datacolumn.AllowDBNull = true;
                    var uniqueCol = GetUniqueColumnName(csvData.Columns, datacolumn);
                    csvData.Columns.Add(uniqueCol);
                }
                while (!csvReader.EndOfData)
                {
                    string[] fieldData = csvReader.ReadFields();
                    //Making empty value as null
                    for (int i = 0; i < fieldData.Length; i++)
                    {
                        if (fieldData[i] == "")
                        {
                            fieldData[i] = null;
                        }
                    }
                    csvData.Rows.Add(fieldData);
                }
            }
            //}
            //catch (Exception ex)
            //{
            //}
            return csvData;
        }

        private static DataColumn GetUniqueColumnName(DataColumnCollection columns, DataColumn dataColumn, int count = 1)
        {
            if (count == 1)
            {
                if (!DataColumnExtensions.Exists(columns, dataColumn))
                {
                    return dataColumn;
                }
            }
            else
            {
                DataColumn candidateColumn = dataColumn;
                candidateColumn.ColumnName = dataColumn.ColumnName + count;

                if (!DataColumnExtensions.Exists(columns, dataColumn))
                {
                    return candidateColumn;
                }
            }
            count++;
            return GetUniqueColumnName(columns, dataColumn, count);
        }

        public static DataTable GetDataTableFromJson(string json)
        {
            DataTable table = null;
            try
            {
                table = JsonConvert.DeserializeObject<DataTable>(json); 
            }
            catch (Exception ex)
            {
            }

            return table;
        }
        public static object Deserialize(string jsonText, Type valueType)
        {
            Newtonsoft.Json.JsonSerializer json = new Newtonsoft.Json.JsonSerializer();

            json.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            json.ObjectCreationHandling = Newtonsoft.Json.ObjectCreationHandling.Replace;
            json.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
            json.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            StringReader sr = new StringReader(jsonText);
            Newtonsoft.Json.JsonTextReader reader = new JsonTextReader(sr);
            object result = json.Deserialize(reader, valueType);
            reader.Close();

            return result;
        }

        public static string GetJson(DataTable dt)
        {
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new

            System.Web.Script.Serialization.JavaScriptSerializer();
            List<Dictionary<string, object>> rows =
              new List<Dictionary<string, object>>();
            Dictionary<string, object> row = null;

            foreach (DataRow dr in dt.Rows)
            {
                row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    row.Add(col.ColumnName.Trim(), dr[col]);
                }
                rows.Add(row);
            }
            return serializer.Serialize(rows);
        }

        public static void LogApiRequest(int apiId, string apiUrl, IPAddress ip)
        {
            using (var db = new ApifoxContext())
            {
                db.ApiRequests.Add(new ApiRequests
                {
                    ApiId = apiId,
                    ApiUrl = apiUrl,
                    Ip = ip.ToString()
                });
                db.SaveChanges();
            }
        }

        internal async static void GetJsonFromGSheets(string filePath)
        {
            const string gDocPart = @"docs.google.com/spreadsheets/d/";
            const string gSheetFormat = @"http://gsheet2json.herokuapp.com/spreadsheet/{0}";
            const string gSheetFormatLong = @"http://gsheet2json.herokuapp.com/spreadsheet/{0}/sheet/{1}";
            const string targetPrefix = "URL=";

            var gSheetUrlReader = new StringReader(File.ReadAllText(filePath));
            // read first line
            gSheetUrlReader.ReadLine();
            // read second line
            var gSheetUrl = gSheetUrlReader.ReadLine().Replace(targetPrefix, string.Empty);

            int index1 = gSheetUrl.IndexOf(gDocPart) + gDocPart.Length;
            if (index1 >= 0)
            {
                int index2 = gSheetUrl.IndexOf('/', index1);
                if (index2 >= 0)
                {
                    var gKey = gSheetUrl.Substring(index1, index2-index1);
                    if (!string.IsNullOrEmpty(gKey))
                    {
                        var client = new WebClient();
                        string data = await client.DownloadStringTaskAsync(string.Format(gSheetFormat, gKey));

                        var jobject = new JObject();
                        if (!string.IsNullOrEmpty(data) && data != "[]")
                        {
                            var a = JArray.Parse(data);

                            foreach (var item in a)
                            {
                                var sheetName = item.ToString();
                                string sheetValue = await client.DownloadStringTaskAsync(string.Format(gSheetFormatLong, gKey, sheetName));
                                jobject.Add(sheetName, JToken.Parse(sheetValue));
                            }
                            File.WriteAllText(filePath.Replace(".url", ".json"), jobject.ToString(), new UTF8Encoding());
                        }
                    }
                }
            }
        }
    }
}