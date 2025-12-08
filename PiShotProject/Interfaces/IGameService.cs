using PiShotProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiShotProject.Interfaces
{
    public interface IGameService
    {
        void StartNewGame(StartGameRequest request);
        void DeclareWinner(int winnerId);
        void RecordGameResult(int winnerId);
        GameStatusResponse GetCurrentStatus();
    }
}
