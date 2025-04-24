using System.Text.Json;

namespace AutoTf.TabletOS.Models;

public static class HttpHelper
{
    /// <summary>
    /// Sends a GET request to the given endpoint and returns it's content as a string.
    /// </summary>
    public static async Task<T?> SendGet<T>(string endpoint, bool reThrow = true, int timeoutSeconds = 5)
    {
        try
        {
            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            client.DefaultRequestHeaders.Add("macAddr", Statics.MacAddress);
			
            HttpResponseMessage response = await client.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync());
        }
        catch
        {
            if(reThrow)
                throw;

            return default;
        }
    }

    public static async Task SendPost(string endpoint, HttpContent content, bool reThrow)
    {
        try
        {
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("macAddr", Statics.MacAddress);
			
            HttpResponseMessage response = await client.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception)
        {
            if(reThrow)
                throw;
        }
    }

    public static async Task<bool> SendPost(string endpoint, HttpContent content)
    {
        try
        {
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("macAddr", Statics.MacAddress);
			
            HttpResponseMessage response = await client.PostAsync(endpoint, content);
            
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}