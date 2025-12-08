using System;

namespace PiShotProject.Models
{
    public class Game
    {
        public int Id { get; set; }
        public int? Profile1Id { get; set; }
        public Profile? Profile1 { get; set; }
        public int? Profile2Id { get; set; }
        public Profile? Profile2 { get; set; }
        public int? WinnerId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime? EndTime { get; set; }
        public void Finish(int winnerId)
        {
            if (!IsActive)
                throw new InvalidOperationException("Game is already finished.");

            if (winnerId != Profile1Id && winnerId != Profile2Id)
                throw new ArgumentException("Winner is not part of this game.");

            IsActive = false;
            WinnerId = winnerId;
            EndTime = DateTime.Now;
        }
        public void Cancel()
        {
            if (!IsActive) return;

            IsActive = false;
            WinnerId = null;
            EndTime = DateTime.Now;
        }
    }
}