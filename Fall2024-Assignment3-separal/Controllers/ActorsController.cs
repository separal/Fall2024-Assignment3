using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Fall2024_Assignment3_separal.Data;
using Fall2024_Assignment3_separal.Models.Actors;
using Newtonsoft.Json;

namespace Fall2024_Assignment3_separal.Controllers
{
    public class ActorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<ActorsController> _logger;
        private readonly string _apiKey;
        private readonly string _endpointUrl;

        public ActorsController(ApplicationDbContext context, IHttpClientFactory clientFactory, ILogger<ActorsController> logger, IConfiguration configuration)
        {
            _context = context;
            _clientFactory = clientFactory;
            _logger = logger;
            _apiKey = configuration["AIService:ApiKey"];
            _endpointUrl = configuration["AIService:EndpointUrl"];
        }

        // Index
        public async Task<IActionResult> Index()
        {
            var actors = await _context.Actors.ToListAsync();
            return View(actors); // Ensure this is a list, not a single Actor
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actors.FirstOrDefaultAsync(m => m.Id == id);

            if (actor == null)
            {
                return NotFound();
            }

            // Use AI service to get movies instead of fetching from MovieActors table
            var movies = await GetMoviesForActor(actor.Name);

            // Get the list of tweets for the actor
            var tweets = await GetTweetsForActor(actor.Name);

            // Get the overall sentiment of all tweets
            var overallSentiment = await GetOverallSentimentForTweets(tweets);

            var viewModel = new ActorDetailsViewModel
            {
                Actor = actor,
                Movies = movies,                 // Use AI-generated movies
                Tweets = tweets,
                OverallSentiment = overallSentiment
            };

            return View(viewModel);
        }


        // GET: Actors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Actors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Gender,Age,ImdbLink,PhotoUrl")] Actor actor)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(actor);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Log or handle the exception as needed
                    ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                }
            }
            // If ModelState is invalid or save fails, re-render the Create view
            return View(actor);
        }

        // Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actors.FindAsync(id);
            if (actor == null)
            {
                return NotFound();
            }
            return View(actor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Gender,Age,ImdbLink,PhotoUrl")] Actor actor)
        {
            if (id != actor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Mark the actor as modified
                    _context.Update(actor);

                    // Save changes to the database
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                  
                }
                return RedirectToAction(nameof(Index));
            }

            return View(actor);
        }


        // Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            return View(actor);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actor = await _context.Actors.FindAsync(id);
            if (actor != null)
            {
                _context.Actors.Remove(actor);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }



        // AI Get tweets function
        private async Task<List<string>> GetTweetsForActor(string actorName)
        {
            var requestBody = new
            {
                messages = new[]
                {
                    new { role = "system", content = "You are an AI assistant that helps people find information." },
                    new { role = "user", content = $"Generate a list of 20 tweets about {actorName}." }
                },
                max_tokens = 800,
                temperature = 0.7,
                top_p = 0.95,
                frequency_penalty = 0,
                presence_penalty = 0,
                stop = (string)null
            };

            var client = _clientFactory.CreateClient(); // Create a new client instance
            client.DefaultRequestHeaders.Add("api-key", _apiKey);

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(_endpointUrl, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonDocument.Parse(responseContent);

                if (result.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    var tweets = choices[0].GetProperty("message").GetProperty("content").GetString();
                    return tweets.Split('\n').ToList();
                }

                return new List<string> { "No tweets found" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error calling AI service for tweets: {ex.Message}");
                return new List<string> { "Error retrieving tweets" };
            }
        }

        // AI Get sentiment analysis function
        private async Task<string> GetOverallSentimentForTweets(List<string> tweets)
        {
            var concatenatedTweets = string.Join("\n", tweets);

            var requestBody = new
            {
                messages = new[]
                {
            new { role = "system", content = "You are an AI assistant that helps people analyze text sentiment." },
            new { role = "user", content = $"Analyze the overall sentiment (positive, negative, or neutral) of the following tweets: {concatenatedTweets}" }
        },
                max_tokens = 800,
                temperature = 0.7,
                top_p = 0.95
            };

            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("api-key", _apiKey);

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(_endpointUrl, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();

                // Log the response to help debug
                _logger.LogInformation("Sentiment Analysis Response: {Response}", responseContent);

                var result = JsonDocument.Parse(responseContent);
                if (result.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    var sentiment = choices[0].GetProperty("message").GetProperty("content").GetString();
                    if (sentiment != null)
                    {
                        // Ensure sentiment is properly categorized
                        return sentiment.Trim().ToLower().Contains("positive") ? "Positive" :
                               sentiment.Contains("negative") ? "Negative" :
                               sentiment.Contains("neutral") ? "Neutral" : "No clear sentiment found";
                    }
                }

                return "No overall sentiment found";
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error calling AI service for sentiment analysis: {ex.Message}");
                return "Error retrieving sentiment";
            }
           
        }

        // AI Get movies function
        private async Task<List<string>> GetMoviesForActor(string actorName)
        {
            var requestBody = new
            {
                messages = new[]
                {
                    new { role = "system", content = "You are an AI assistant that helps people find information." },
                    new { role = "user", content = $"List only the titles of movies starring {actorName}." }
                },
                max_tokens = 800,
                temperature = 0.7,
                top_p = 0.95,
                frequency_penalty = 0,
                presence_penalty = 0,
                stop = (string)null
            };

            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("api-key", _apiKey);

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(_endpointUrl, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonDocument.Parse(responseContent);

                if (result.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    var movies = choices[0].GetProperty("message").GetProperty("content").GetString();
                    return movies.Split('\n').Select(m => m.Trim()).Where(m => !string.IsNullOrEmpty(m)).ToList();
                }

                return new List<string> { "No movies found" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Error calling AI service for movies: {Message}", ex.Message);
                return new List<string> { "Error fetching movies" };
            }
        }
    }
}
