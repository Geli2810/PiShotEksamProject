using System;

namespace PiShotProject.Models
{
    public class ShotAttempt
    {
        public int Id { get; set; }
        public int ProfileId { get; set; }
        public Profile? Profile { get; set; }
        public DateTime AttemptedAt { get; set; } = DateTime.Now;
        public int? GameId { get; set; }
    }
}
