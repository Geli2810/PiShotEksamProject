using PiShotProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiShotProject.Interfaces
{
    public interface IGameRepostitory
    {
        void StartGame(int player1Id, int player2Id);
        void UpdateCurrentWinner(int winnerId);
        CurrentGame GetCurrentGame();
        void AddGameResult(int winnerId, int loserId);
        void ResetGame();
    }
}
