using Fall2024_Assignment3_separal;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

public class AIService
{
    private readonly HttpClient _httpClient;
    private readonly AIServiceOptions _options;

    public AIService(HttpClient httpClient, IOptions<AIServiceOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<string> GetAIResponseAsync(string userMessage)
    {
        var requestBody = new
        {
            messages = new[]
            {
                new { role = "system", content = "You are an AI assistant that helps people find information." },
                new { role = "user", content = userMessage }
            },
            max_tokens = 150,
            temperature = 0.7,
            top_p = 0.95,
            frequency_penalty = 0,
            presence_penalty = 0,
            stop = (string)null
        };

        var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");

        var response = await _httpClient.PostAsync(_options.EndpointUrl, content);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        return responseBody;
    }
}
