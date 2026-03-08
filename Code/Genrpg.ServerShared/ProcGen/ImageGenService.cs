

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Genrpg.ServerShared.Config;
using Genrpg.Shared.Config.Constants;
using Genrpg.Shared.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Genrpg.ServerShared.ProcGen
{

    public interface IImageGenService : IInjectable
    {

    }
    public class ImageGenService : IImageGenService
    {

        private IServerConfig _serverConfig = null;
        public async Task GenerateImage(string prompt, string outputPath)
        {
            using (HttpClient client = new HttpClient())
            {
                // Construct the JSON payload for Nano Banana 2
                var payload = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = prompt } } }
                    },
                    generationConfig = new
                    {
                        responseModalities = new[] { "IMAGE" }
                    }
                };

                string BaseUrl = _serverConfig.GetSecret(AppConfigKeys.GoogleApiURL);
                string ApiKey = _serverConfig.GetSecret(AppConfigKeys.GoogleApiKey);

                string jsonPayload = JsonConvert.SerializeObject(payload);
                StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // Make the request
                HttpResponseMessage response = await client.PostAsync($"{BaseUrl}?key={ApiKey}", content);
                string responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    JObject jsonResponse = JObject.Parse(responseString);
                    // Extract the Base64 image data
                    string base64Image = jsonResponse["candidates"][0]["content"]["parts"][0]["inlineData"]["data"].ToString();

                    byte[] imageBytes = Convert.FromBase64String(base64Image);
                    System.IO.File.WriteAllBytes(outputPath, imageBytes);

                    Console.WriteLine("Image successfully saved to " + outputPath);
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}\n{responseString}");
                }
            }
        }
    }
}