using System.Collections.Generic;

namespace Fall2024_Assignment3_separal.Models.Movies
{
    public class MovieDetailsViewModel
    {
        public Movie Movie { get; set; }
        public List<string> Actors { get; set; }
        public List<string> Reviews { get; set; }
        public string OverallSentiment { get; set; }
        public List<string> SentimentResults { get; set; }
    }
}
