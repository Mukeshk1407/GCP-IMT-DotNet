using Google.Apis.Auth.OAuth2;
using Google.Apis.Compute.v1;
using Google.Cloud.Compute.V1;
using Grpc.Auth;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace GCP_IMT.Controllers
{
    [ApiController]
    [Route("api/instancelist")]
    public class InstancesController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetInstanceList()
        {
            try
            {
                // Load the service account key from the stored file
                //var directoryPath = "D:/Key/Key1.json";
                //var fileName = HttpContext.Session.GetString("ServiceAccountKeyName");
                var keyFilePath = "D:\\Keys\\key1.json";
                var jsonKey = System.IO.File.ReadAllText(keyFilePath);

                // Deserialize the content into a class representing the expected structure
                var serviceAccountKey = JsonConvert.DeserializeObject<ServiceAccountKey>(jsonKey);

                if (serviceAccountKey == null)
                {
                    return BadRequest(new { Message = "Invalid service account key file format" });
                }

                // Access the project_id property
                var projectId = serviceAccountKey.project_id;

                // Authenticate with Google Cloud using the service account key
                // Authenticate with Google Cloud using the service account key
                var credential = GoogleCredential.FromJson(jsonKey).CreateScoped(ComputeService.Scope.CloudPlatform);

                // Create a Compute Engine client
                var instancesClient = new InstancesClientBuilder
                {
                    ChannelCredentials = credential.ToChannelCredentials()
                }.Build();

                // Fetch the list of instances in the desired project
                var instances = ListInstances(instancesClient, projectId);

                // Return the list of instances
                return Ok(new { Message = "Instance list retrieved successfully", Instances = instances });
            }
            catch (Exception ex)
            {
                // Handle authentication or API call errors
                return BadRequest(new { Message = $"Error retrieving instance list: {ex.Message}" });
            }
        }

        private List<string> ListInstances(InstancesClient instancesClient, string projectId)
        {
            // Create a ListInstancesRequest
            var listInstancesRequest = new ListInstancesRequest
            {
                Project = projectId,
                Zone = "us-west4-b",
            };

            // List instances in the specified project and zone
            var instanceList = instancesClient.List(listInstancesRequest);

            // Extract instance names
            var instances = new List<string>();
            foreach (var page in instanceList.AsRawResponses())
            {
                // Extract instance names from the page
                foreach (var instance in page.Items)
                {
                    instances.Add(instance.Name);
                }
            }

            return instances;
        }

        // Define a class representing the expected structure of the service account key
        private class ServiceAccountKey
        {
            // Define properties matching the structure of the service account key JSON
            public string type { get; set; }
            public string project_id { get; set; }
            public string private_key_id { get; set; }
            public string private_key { get; set; }
            public string client_email { get; set; }
            public string client_id { get; set; }
            public string auth_uri { get; set; }
            public string token_uri { get; set; }
            public string auth_provider_x509_cert_url { get; set; }
            public string client_x509_cert_url { get; set; }
            public string universe_domain { get; set; }
        }
    }
}
