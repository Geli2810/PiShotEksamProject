using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PiShotProject.Models
{
    public class ShotAttempt
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Profile))]
        public int ProfileId { get; set; }

        public DateTime AttemptedAt { get; set; }

        // Matcher kolonnen [GameResultId] i SQL
        [ForeignKey(nameof(GameResult))]
        public int? GameResultId { get; set; }

        // Navigation properties
        public virtual Profile? Profile { get; set; }
        public virtual GameResult? GameResult { get; set; }
    }
}
