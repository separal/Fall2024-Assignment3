
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fall2024_Assignment3_separal.Data;
using Fall2024_Assignment3_separal.Models.Movies;

namespace Fall2024_Assignment3_separal.Controllers
{
    public class MoviesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _endpointUrl;
        private readonly ILogger<MoviesController> _logger;
        private readonly ApplicationDbContext _context;

        public MoviesController(IConfiguration configuration, HttpClient httpClient, ILogger<MoviesController> logger, ApplicationDbContext context)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = configuration["AIService:ApiKey"];
            _endpointUrl = configuration["AIService:EndpointUrl"];
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies.ToListAsync();
            foreach (var movie in movies)
            {
                _logger.LogInformation("Poster URL: {PosterUrl}", movie.PosterUrl);
            }
            return View(movies);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,ImdbLink,Genre,Year,PosterUrl")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ImdbLink,Genre,Year,PosterUrl")] Movie movie)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

       

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }

        // Fetches a list of actors starring in a movie
        private async Task<List<string>> GetActorsForMovie(string movieTitle)
        {
            var requestBody = new
            {
                messages = new[]
                {
                    new { role = "system", content = "You are an AI assistant that helps people find information." },
                    new { role = "user", content = $"List the main actors in the movie titled {movieTitle}." }
                },
                max_tokens = 800,
                temperature = 0.7,
                top_p = 0.95
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);

            var response = await _httpClient.PostAsync(_endpointUrl, content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            var result = JsonDocument.Parse(responseContent);
            if (result.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var actors = choices[0].GetProperty("message").GetProperty("content").GetString();
                return actors.Split('\n').Select(a => a.Trim()).Where(a => !string.IsNullOrEmpty(a)).ToList();
            }
            return new List<string> { "No actors found" };
        }

        // Generates AI-based reviews for a movie
        private async Task<List<string>> GetReviewsForMovie(string movieTitle)
        {
            var requestBody = new
            {
                messages = new[]
                {
                    new { role = "system", content = "You are an AI assistant that writes reviews about movies." },
                    new { role = "user", content = $"Write ten short reviews about the movie {movieTitle}." }
                },
                max_tokens = 800,
                temperature = 0.7,
                top_p = 0.95
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);

            var response = await _httpClient.PostAsync(_endpointUrl, content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            var result = JsonDocument.Parse(responseContent);
            if (result.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var reviews = choices[0].GetProperty("message").GetProperty("content").GetString();
                return reviews.Split('\n').Where(r => !string.IsNullOrEmpty(r)).Take(10).ToList();
            }
            return new List<string> { "No reviews found" };
        }

        // Analyzes the sentiment of a list of reviews and returns the overall sentiment
        private async Task<string> GetOverallSentimentForReviews(List<string> reviews)
        {
            var concatenatedReviews = string.Join("\n", reviews);

            var requestBody = new
            {
                messages = new[]
                {
                    new { role = "system", content = "You are an AI assistant that helps people analyze text sentiment." },
                    new { role = "user", content = $"Analyze the overall sentiment (positive, negative, or neutral) of the following reviews: {concatenatedReviews}" }
                },
                max_tokens = 800,
                temperature = 0.7,
                top_p = 0.95
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);

            var response = await _httpClient.PostAsync(_endpointUrl, content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            var result = JsonDocument.Parse(responseContent);
            if (result.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var sentiment = choices[0].GetProperty("message").GetProperty("content").GetString();
                return sentiment.Trim().ToLower().Contains("positive") ? "Positive" :
                       sentiment.Contains("negative") ? "Negative" :
                       sentiment.Contains("neutral") ? "Neutral" : "No clear sentiment found";
            }
            return "No overall sentiment found";
        }

        // Analyzes the sentiment of each review
        private async Task<List<string>> AnalyzeSentimentForEachReview(List<string> reviews)
        {
            var sentimentResults = new List<string>();

            foreach (var review in reviews)
            {
                var requestBody = new
                {
                    messages = new[]
                    {
                        new { role = "system", content = "You are an AI assistant that analyzes text sentiment." },
                        new { role = "user", content = $"Analyze the sentiment (positive, negative, or neutral) of the following review: {review}" }
                    },
                    max_tokens = 800,
                    temperature = 0.7,
                    top_p = 0.95
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);

                var response = await _httpClient.PostAsync(_endpointUrl, content);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();

                var result = JsonDocument.Parse(responseContent);
                if (result.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    var sentiment = choices[0].GetProperty("message").GetProperty("content").GetString();
                    sentimentResults.Add(sentiment.Contains("positive") ? "Positive" :
                                         sentiment.Contains("negative") ? "Negative" :
                                         sentiment.Contains("neutral") ? "Neutral" : "Unknown");
                }
                else
                {
                    sentimentResults.Add("Unknown");
                }
            }

            return sentimentResults;
        }

        // GET: Movies/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }

            // Fetch actors, reviews, and sentiment analysis
            var actors = await GetActorsForMovie(movie.Title);
            var reviews = await GetReviewsForMovie(movie.Title);
            var overallSentiment = await GetOverallSentimentForReviews(reviews);
            var sentimentResults = await AnalyzeSentimentForEachReview(reviews);

            // Create a view model to pass data to the view
            var viewModel = new MovieDetailsViewModel
            {
                Movie = movie,
                Actors = actors,
                Reviews = reviews,
                OverallSentiment = overallSentiment,
                SentimentResults = sentimentResults
            };

            return View(viewModel);
        }
    }
}
