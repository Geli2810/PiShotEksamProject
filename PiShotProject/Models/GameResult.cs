using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiShotProject.Models
{
    public class GameResult
    {
        public int Id { get; set; }
        public int WinnerId { get; set; }
        public int LoserId { get; set; }
        public DateTime PlayedOn { get; set; } = DateTime.UtcNow;
    }
}