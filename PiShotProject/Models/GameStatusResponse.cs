using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiShotProject.Models
{
    public class GameStatusResponse
    {
        public bool IsActive { get; set; }
        public int P1_Id { get; set; }
        public int P2_Id { get; set; }
        public int? CurrentWinnerId { get; set; }
        public string WinnerName { get; set; }
        public string WinnerImage { get; set; }
    }
}
