using System;

namespace PiShotProject.Models
{
    public class ShotAttempt
    {
        public int Id { get; set; }
        public int ProfileId { get; set; }
        public DateTime? AttemptedAt { get; set; }

        public Profile Profile { get; set; }
    }
}