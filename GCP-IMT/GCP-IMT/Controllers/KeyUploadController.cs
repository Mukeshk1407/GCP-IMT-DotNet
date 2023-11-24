using Google.Apis.Auth.OAuth2;
using Google.Cloud.Compute.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace GCP_IMT.Controllers
{
    [ApiController]
    [Route("api/uploadkey")]
    public class KeyUploadController : ControllerBase
    {
        [HttpPost]
        public IActionResult UploadKey(IFormFile keyFile)
        {
            try
            {
                // Ensure the directory exists, create it if not
                var directoryPath = "D:/Keys";
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Save the file to the specified directory
                var filePath = Path.Combine(directoryPath, "key1.json");
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    keyFile.CopyTo(stream);
                }

                // Validate the content of the uploaded file
                var content = System.IO.File.ReadAllText(filePath);
                var isValid = ValidateServiceAccountKey(content);

                if (!isValid)
                {
                    // If validation fails, delete the file and return an error
                    System.IO.File.Delete(filePath);
                    return BadRequest(new { Message = "Invalid service account key file format" });
                }

                // Store the file name in session
         //       HttpContext.Session.SetString("ServiceAccountKeyName", "key1.json");

                return Ok(new { Message = "Service account key uploaded successfully" });
            }
            catch (Exception ex)
            {
                // Handle file upload or other errors
                return BadRequest(new { Message = $"Error uploading service account key: {ex.Message}" });
            }
        }

        private bool ValidateServiceAccountKey(string content)
        {
            try
            {
                // Deserialize the content into a class representing the expected structure
                var serviceAccountKey = JsonConvert.DeserializeObject<ServiceAccountKey>(content);

                // Perform necessary checks for required properties
                if (string.IsNullOrEmpty(serviceAccountKey?.type) ||
                    string.IsNullOrEmpty(serviceAccountKey?.project_id) ||
                    string.IsNullOrEmpty(serviceAccountKey?.private_key_id) ||
                    string.IsNullOrEmpty(serviceAccountKey?.private_key) ||
                    string.IsNullOrEmpty(serviceAccountKey?.client_email) ||
                    string.IsNullOrEmpty(serviceAccountKey?.client_id) ||
                    string.IsNullOrEmpty(serviceAccountKey?.auth_uri) ||
                    string.IsNullOrEmpty(serviceAccountKey?.token_uri) ||
                    string.IsNullOrEmpty(serviceAccountKey?.auth_provider_x509_cert_url) ||
                    string.IsNullOrEmpty(serviceAccountKey?.client_x509_cert_url) ||
                    string.IsNullOrEmpty(serviceAccountKey?.universe_domain))
                {
                    return false;
                }

                // Additional validation logic if needed

                return true;
            }
            catch (JsonException)
            {
                return false; // JSON deserialization failed
            }
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
