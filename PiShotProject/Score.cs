using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiShotProject
{
    public class Score
    {
        public int Id { get; set; }
        public string PlayerName { get; set; } // "Player1" or "Player2"
        public DateTime ScoredAt { get; set; }
    }
}
