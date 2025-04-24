using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GeminiChatBot
{
    class Program
    {
        static readonly HttpClient client = new HttpClient();
        static string geminiApiKey = ""; // 🔐 Replace with actual key

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Welcome to Gemini Chatbot ===");
            Console.WriteLine("Type 'exit' to stop chatting.\n");

            while (true)
            {
                Console.Write("You: ");
                string userInput = Console.ReadLine();

                if (userInput.ToLower() == "exit") break;

                string response = await GetGeminiReply(userInput);
                Console.WriteLine("Gemini: " + response + "\n");
            }
        }

        static async Task<string> GetGeminiReply(string userInput)
        {
            try
            {
                string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={geminiApiKey}";

                // Adjusted request body structure based on the curl command
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = userInput } // Using user input as the text
                            }
                        }
                    }
                };

                string json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Send the request and get the response
                var response = await client.PostAsync(url, content);
                response.EnsureSuccessStatusCode(); // Throws an exception if the status code is not 2xx

                string responseBody = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(responseBody);

                // Check if the response contains candidates
                if (result?.candidates != null && result.candidates.Count > 0 && result.candidates[0].content.parts.Count > 0)
                {
                    return result.candidates[0].content.parts[0].text.ToString(); // Extract the text correctly
                }
                else if (result?.error != null)
                {
                    return "API Error: " + result.error.message;
                }
                else
                {
                    return "No valid response from Gemini.";
                }
            }
            catch (HttpRequestException httpEx)
            {
                return "HTTP Error: " + httpEx.Message;
            }
            catch (JsonException jsonEx)
            {
                return "JSON Parsing Error: " + jsonEx.Message;
            }
            catch (Exception ex)
            {
                return "Exception: " + ex.Message;
            }
        }
    }
}