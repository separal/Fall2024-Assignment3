using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Fall2024_Assignment3_separal.Models;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Fall2024_Assignment3_separal.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;

        public HomeController(ILogger<HomeController> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            string apiUrl = "https://fall2024-separal-openai.openai.azure.com/openai/deployments/gpt-35-turbo/chat/completions?api-version=2024-08-01-preview";
            string apiKey = "39mfFXTFm8ygmMOYKgB07VmcGSozIjpcR3ttnmXhWVw3cg0wIywEJQQJ99AJACYeBjFXJ3w3AAABACOGbigP";

            var requestBody = new
            {
                messages = new[]
                {
                    new { role = "system", content = "You are an AI assistant that helps people find information." },
                    new { role = "user", content = "List 4 popular movies." }
                },
                max_tokens = 800,
                temperature = 0.7,
                frequency_penalty = 0,
                presence_penalty = 0,
                top_p = 0.95,
                stop = (string)null
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Add("api-key", apiKey);

            var response = await _httpClient.PostAsync(apiUrl, content);
            string responseContent = await response.Content.ReadAsStringAsync();

            dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);
            string apiMessage = jsonResponse.choices[0].message.content;

            ViewBag.ApiMessage = apiMessage;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
