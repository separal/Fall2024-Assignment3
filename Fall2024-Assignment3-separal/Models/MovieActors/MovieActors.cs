using Fall2024_Assignment3_separal.Models.Movies;
using Fall2024_Assignment3_separal.Models.Actors;

namespace Fall2024_Assignment3_separal.Models.MovieActors
{
    public class MovieActor
    {
        public int MovieId { get; set; }
        public Movie Movie { get; set; }

        public int ActorId { get; set; }
        public Actor Actor { get; set; }
    }
}
