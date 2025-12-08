using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiShotProject.Models
{
    public class CurrentGame
    {
        public int Id { get; set; }
        public int Player1Id { get; set; }
        public int Player2Id { get; set; }
        public bool IsActive { get; set; }
        public DateTime? StartTime { get; set; }

        public bool IsTiebreak { get; set; }
        public int TiebreakOffsetP1 { get; set; }
        public int TiebreakOffsetP2 { get; set; }

        public int? CurrentWinnerId { get; set; }

        public Profile Player1 { get; set; }
        public Profile Player2 { get; set; }

        public string WinnerName { get; set; }
        public string WinnerImage {  get; set; }


    }
}