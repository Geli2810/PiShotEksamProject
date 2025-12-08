using PiShotProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiShotProject.Interfaces
{
    public interface IGameRepository
    {
        CurrentGame? GetState();
        void SaveState(CurrentGame game);
        void AddResult(GameResult result);
    }
}
