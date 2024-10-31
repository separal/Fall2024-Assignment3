using Fall2024_Assignment3_separal.Models.Movies;
using Fall2024_Assignment3_separal.Models.MovieActors;


namespace Fall2024_Assignment3_separal.Models.Actors
{
    public class Actor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public string ImdbLink { get; set; }
        public string PhotoUrl { get; set; }
        //public List<Movie> Movies { get; set; }
        // Navigation property for the many-to-many relationship
        public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
    }
}
