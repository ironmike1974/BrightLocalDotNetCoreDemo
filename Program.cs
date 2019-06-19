using System;
using BrightLocal;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace demo
{
    class Program
    {

        private static void LogResults(string content)
        {
            System.IO.File.WriteAllText("out.txt", "\n======\n" + content);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("BrightLocal Demo");

            List<string> localDirectories = new List<string>();
            localDirectories.Add("google");
            localDirectories.Add("yahoo");
            //localDirectories.Add("facebook");


            api Api = new api("api key", "secret");
            batchApi batchRequest = new batchApi(Api);

            // Create a new batch
            int batchId = batchRequest.Create();

            Console.WriteLine($"Batch created: id {batchId}");

            var model = new {
                BusinessNames = "Foo",
                City = "New York",
                PostalCode = "90210",
                Telephone = "2140000000"
            };

            // Add jobs to batch
            foreach (var item in localDirectories)
            {
                var parameters = new api.Parameters();
                parameters.Add("batch-id", batchId);
                parameters.Add("business-names", model.BusinessNames);
                parameters.Add("city", model.City);
                parameters.Add("postcode", model.PostalCode);
                parameters.Add("local-directory", item);
                parameters.Add("country", "USA");

                var jobId = Api.Post("/v4/ld/fetch-reviews-by-business-data", parameters);

                Console.WriteLine($"Job created with id {jobId}");

                // NOTE: ResponseStatus.Completed doesn't exist
                if (jobId.ResponseStatus == jobId.ResponseStatus) //ResponseStatus.Completed)
                {
                    dynamic job = JsonConvert.DeserializeObject(jobId.Content);
                    if (!job.success)
                    {
                        string message = "Error adding job";
                        var batchException = new ApplicationException(message + job.errors, job.ErrorException);
                        throw batchException;
                    }
                }
                else
                {
                    throw new ApplicationException(jobId.ErrorMessage);
                }
            }
            // Commit the batch, resturns true or false
            bool commit = batchRequest.Commit(batchId);

            // Poll for results. In a real world example you should do this in a backgroud process, such as HangFire,  or use the Task Parallel Library to create a BackGroundWorker Task.
            // It is bad practice to use Thread.Sleep(). This is only for the example and will actually freeze the UI until the while loop is finished. 

            var results = batchRequest.GetResults(batchId);
            var resultsContent = results.Content;
            LogResults(resultsContent);
            dynamic reviewResults = JsonConvert.DeserializeObject(resultsContent);

            if (reviewResults.success)
            {
                while (reviewResults.status != "Stopped" || reviewResults.status != "Finished")
                {
                    Thread.Sleep(10000);
                    results = batchRequest.GetResults(batchId);
                    resultsContent = results.Content;
                    LogResults(resultsContent);
                    reviewResults = JsonConvert.DeserializeObject(resultsContent);
                }
                Console.WriteLine($"Results: {reviewResults.ToString()}");
                //return reviewResults;
            }
            else
            {
                const string message = "Error Retrieving batch results ";
                var batchException = new ApplicationException(message + reviewResults.errors, results.ErrorException);
                Console.WriteLine("Error: " + batchException.ToString());
                throw batchException;
            }

            Console.WriteLine("Done!");
        }


    }
}
