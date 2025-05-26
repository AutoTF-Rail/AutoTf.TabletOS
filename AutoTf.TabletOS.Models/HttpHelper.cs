using System.Text.Json;
using AutoTf.CentralBridge.Shared.Models;
using AutoTf.CentralBridge.Shared.Models.Enums;

namespace AutoTf.TabletOS.Models;

public static class HttpHelper
{
    /// <summary>
    /// Sends a GET request to the given endpoint and returns it's content as a string.
    /// </summary>
    public static async Task<Result<T>> SendGet<T>(string endpoint, int timeoutSeconds = 5)
    {
        using HttpClient client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        client.DefaultRequestHeaders.Add("macAddr", Statics.MacAddress);
        
        try
        {
            HttpResponseMessage response = await client.GetAsync(endpoint);

            string content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                T? value = JsonSerializer.Deserialize<T>(content);

                if (value == null)
                    return Result<T>.Fail(ResultCode.InternalServerError, "Deserialization returned null.");

                return Result<T>.Ok(value);
            }

            return Result<T>.Fail(ResultBase.MapStatusToResultCode(response.StatusCode), content);
        }
        catch (Exception ex)
        {
            return Result<T>.Fail(ResultCode.InternalServerError, $"Exception occurred: {ex.Message}");
        }
    }
    
    public static async Task<Result> SendGet(string endpoint, int timeoutSeconds = 5)
    {
        using HttpClient client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        client.DefaultRequestHeaders.Add("macAddr", Statics.MacAddress);
        
        try
        {
            HttpResponseMessage response = await client.GetAsync(endpoint);
            
            string result = await response.Content.ReadAsStringAsync();
            
            return response.IsSuccessStatusCode ? Result.Ok() : Result.Fail(ResultBase.MapStatusToResultCode(response.StatusCode), result);
        }
        catch (Exception ex)
        {
            return Result.Fail(ResultCode.InternalServerError, $"Exception occurred: {ex.Message}");
        }
    }

    public static async Task<Result<T>> SendPost<T>(string endpoint, HttpContent content, int timeoutSeconds = 5)
    {
        using HttpClient client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        client.DefaultRequestHeaders.Add("macAddr", Statics.MacAddress);
        
        try
        {
            HttpResponseMessage response = await client.PostAsync(endpoint, content);
            
            string result = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                T? value = JsonSerializer.Deserialize<T>(result);

                if (value == null)
                    return Result<T>.Fail(ResultCode.InternalServerError, "Deserialization returned null.");

                return Result<T>.Ok(value);
            }

            return Result<T>.Fail(ResultBase.MapStatusToResultCode(response.StatusCode), result);
        }
        catch (Exception ex)
        {
            return Result<T>.Fail(ResultCode.InternalServerError, $"Exception occurred: {ex.Message}");
        }
    }
    public static async Task<Result> SendPost(string endpoint, HttpContent content, int timeoutSeconds = 5)
    {
        using HttpClient client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        client.DefaultRequestHeaders.Add("macAddr", Statics.MacAddress);
        
        try
        {
            HttpResponseMessage response = await client.PostAsync(endpoint, content);
            
            string result = await response.Content.ReadAsStringAsync();
            
            return response.IsSuccessStatusCode ? Result.Ok() : Result.Fail(ResultBase.MapStatusToResultCode(response.StatusCode), result);
        }
        catch (Exception ex)
        {
            return Result.Fail(ResultCode.InternalServerError, $"Exception occurred: {ex.Message}");
        }
    }
}