using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fall2024_Assignment3_separal.Data;
using Fall2024_Assignment3_separal.Models; // Ensure this matches where MovieActorsViewModel is defined
using Fall2024_Assignment3_separal.Models.Movies;
using Fall2024_Assignment3_separal.Models.Actors;
using Fall2024_Assignment3_separal.Models.MovieActors; // If you have a separate folder for MovieActors
using System.Linq;
using System.Threading.Tasks;

namespace Fall2024_Assignment3_separal.Controllers
{
    public class BothController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BothController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Display Movies with Linked Actors
        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies
                .Include(m => m.MovieActors)
                .ThenInclude(ma => ma.Actor)
                .ToListAsync();
            return View(movies);
        }

        // GET: Link Actors to a Movie
        public async Task<IActionResult> LinkActors(int movieId)
        {
            var movie = await _context.Movies
                .Include(m => m.MovieActors)
                .ThenInclude(ma => ma.Actor)
                .FirstOrDefaultAsync(m => m.Id == movieId);

            if (movie == null)
            {
                return NotFound();
            }

            // Get list of actors not already linked to the movie
            var linkedActorIds = movie.MovieActors.Select(ma => ma.ActorId).ToList();

            var availableActors = await _context.Actors
                .Where(a => !linkedActorIds.Contains(a.Id))
                .ToListAsync();

            var viewModel = new MovieActorsViewModel
            {
                Movie = movie,
                AvailableActors = availableActors
            };

            return View(viewModel);
        }

        // POST: Add an Actor to a Movie
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddActorToMovie(int movieId, int actorId)
        {
            var movie = await _context.Movies.FindAsync(movieId);
            var actor = await _context.Actors.FindAsync(actorId);

            if (movie == null || actor == null)
            {
                return NotFound();
            }

            // Create and add new link entry
            var movieActor = new MovieActor { MovieId = movieId, ActorId = actorId };
            _context.MovieActors.Add(movieActor);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(LinkActors), new { movieId });
        }

        // POST: Remove Actor from a Movie
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveActorFromMovie(int movieId, int actorId)
        {
            var movieActor = await _context.MovieActors
                .FirstOrDefaultAsync(ma => ma.MovieId == movieId && ma.ActorId == actorId);

            if (movieActor == null)
            {
                return NotFound();
            }

            _context.MovieActors.Remove(movieActor);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
