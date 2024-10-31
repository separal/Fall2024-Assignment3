using Fall2024_Assignment3_separal.Models.Movies;
using Fall2024_Assignment3_separal.Models.Actors;
using System.Collections.Generic;

namespace Fall2024_Assignment3_separal.Models
{
    public class MovieActorsViewModel
    {
        public Movie Movie { get; set; }
        public List<Actor> AvailableActors { get; set; }
    }
}
