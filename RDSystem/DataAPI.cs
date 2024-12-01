using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace RDSystem
{
    public class OpenSkyClient
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public async Task<string> GetOpenSkyDataAsync(string url)
        {
            try
            {
                // Send the request to the OpenSky API
                HttpResponseMessage response = await httpClient.GetAsync(url);

                // Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else if ((int)response.StatusCode == 429) // Handle rate limiting
                {
                    Console.WriteLine("Rate limit exceeded. Retrying after a delay...");
                    await Task.Delay(5000); // Wait for 5 seconds
                    return await GetOpenSkyDataAsync(url); // Retry once after delay
                }
                else
                {
                    Console.WriteLine($"Unexpected status code: {(int)response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }
    }

    public class OpenSkyApiHandler
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public async Task<string> FetchDataWithRetryAfterAsync(string url)
        {
            while (true) // Loop until a successful response or fatal error
            {
                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                    else if ((int)response.StatusCode == 429) // Rate limit exceeded
                    {
                        Console.WriteLine("Rate limit exceeded.");

                        // Check for Retry-After header
                        if (response.Headers.TryGetValues("Retry-After", out var values))
                        {
                            int retryAfterSeconds;
                            if (int.TryParse(values.FirstOrDefault(), out retryAfterSeconds))
                            {
                                Console.WriteLine($"Retrying after {retryAfterSeconds} seconds...");
                                await Task.Delay(retryAfterSeconds * 1000); // Wait as instructed
                            }
                            else
                            {
                                Console.WriteLine("Retry-After header not found. Retrying in 5 seconds...");
                                await Task.Delay(5000); // Default retry delay
                            }
                        }
                        else
                        {
                            Console.WriteLine("Retry-After header not found. Retrying in 5 seconds...");
                            await Task.Delay(5000); // Default retry delay
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Unexpected response: {(int)response.StatusCode} - {response.ReasonPhrase}");
                        return null;
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"HTTP Request error: {ex.Message}");
                    return null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                    return null;
                }
            }
        }
    }
}
