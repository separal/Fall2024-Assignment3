using Fall2024_Assignment3_separal.Models.Actors;
using Fall2024_Assignment3_separal.Models.MovieActors;


namespace Fall2024_Assignment3_separal.Models.Movies
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ImdbLink { get; set; }
        public string Genre { get; set; }
        public int Year { get; set; }
        public string PosterUrl { get; set; }
        // public ICollection<Actor> Actors { get; set; } // Navigation property
        // Navigation property for the many-to-many relationship
        public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
    }
}
