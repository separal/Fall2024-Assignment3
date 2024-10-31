using System.Collections.Generic;

namespace Fall2024_Assignment3_separal.Models.Actors
{
    public class ActorDetailsViewModel
    {
        public Actor Actor { get; set; }
        public List<string> Movies { get; set; }
        public List<string> Tweets { get; set; }
        public string OverallSentiment { get; set; } // The overall sentiment as a label (e.g., Positive, Negative, Neutral)
    }
}
