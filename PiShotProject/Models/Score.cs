using System;

namespace PiShotProject.Models
{
    public class Score
    {
        public int Id { get; set; }
        public int ProfileId { get; set; }
        public Profile? Profile { get; set; }
        public DateTime ScoredAt { get; set; } = DateTime.Now;
        public int? GameId { get; set; }
    }
}
