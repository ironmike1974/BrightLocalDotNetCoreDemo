using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BrightLocal
{
    public class batchApi
    {
        // Declare private variable
        private api Api;

        // Methods for creating, commiting, getting, deleting, stopping a batch request

        // On success reurns int
        public int Create()
        {
            Method method = Method.POST;
            var parameters = new api.Parameters();
            var response =  this.Api.Call(method, "/v4/batch", parameters);
            dynamic obj = JsonConvert.DeserializeObject(response.Content);
            if (obj.success != "true")
            {
                const string message = "Error creating Batch " ;
                var batchException = new ApplicationException(message + obj.errors, obj.ErrorException);
                throw batchException;
            }
            return obj["batch-id"];
        }

        // On success returns true
        public bool Commit(int batchId)
        {
            Method method = Method.PUT;
            var parameters = new api.Parameters();
            parameters.Add("batch-id", batchId);
            var request = this.Api.Call(method, "/v4/batch", parameters);
            dynamic obj = JsonConvert.DeserializeObject(request.Content);
            if (obj.success != "true")
            {
                const string message = "Error ccommiting Batch ";
                var batchException = new ApplicationException(message + obj.errors, obj.ErrorException);
                throw batchException;
            }
            return true;                
        }

        //Returns json
        public IRestResponse GetResults(int batchId)
        {
            Method method = Method.GET;
            var parameters = new api.Parameters();
            parameters.Add("batch-id", batchId);
            var response =  this.Api.Call(method, "/v4/batch", parameters);
            dynamic obj = JsonConvert.DeserializeObject(response.Content);
            return response;
        }

        public IRestResponse Delete(string batchId)
        {
            Method method = Method.DELETE;
            var parameters = new api.Parameters();
            parameters.Add("batch-id", batchId);
            var response =  this.Api.Call(method, "/v4/batch", parameters);
            dynamic obj = JsonConvert.DeserializeObject(response.Content);
            if (obj.success != "true")
            {
                const string message = "Error deleting Batch ";
                var batchException = new ApplicationException(message + obj.errors, obj.ErrorException);
                throw batchException;
            }
            return response;
        }

        public IRestResponse Stop(string batchId)
        {
            Method method = Method.PUT;
            var parameters = new api.Parameters();
            parameters.Add("batch-id", batchId);
            var response = this.Api.Call(method, "/v4/batch/stop", parameters);
            dynamic obj = JsonConvert.DeserializeObject(response.Content);
            if (obj.success != "true")
            {
                const string message = "Error stoping Batch ";
                var batchException = new ApplicationException(message + obj.errors, obj.ErrorException);
                throw batchException;
            }
            return response;
        }

        //batchApi class contructor
        public batchApi(api Api)
        {
            this.Api = Api;
        }

        public api.Parameters convertListToParameters(RankingsSearch item)
        {
            var parameters = new api.Parameters();
            foreach (var directoryinfo in item.GetType().GetProperties())
            {
                foreach (CustomAttributeData att in directoryinfo.CustomAttributes)
                {
                    foreach (CustomAttributeTypedArgument arg in att.ConstructorArguments)
                    {
                        parameters.Add(arg.Value.ToString(), directoryinfo.GetValue(item, null));

                    }
                }
            }
            return parameters;
        }
    }

  

    public class RankingsSearch 
    {
        public RankingsSearch()
        {
            urls = new List<string>();
        }
        [JsonProperty("search-engine")]
        public string search_engine { get; set; }
        [JsonProperty("country")]
        public string country { get; set; }
        [JsonProperty("google-location")]
        public string google_location { get; set; }
        [JsonProperty("search-term")]
        public string search_term { get; set; }
        [JsonProperty("urls")]
        public List<string> urls  { get; set; }
        [JsonProperty("business-names")]
        public List<string> business_names { get; set; }
    }
}