using PiShotProject.Models; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiShotProject.Interfaces
{
    public interface IScoreRepository
    {
        void AddScore(int profileId);
        void AddAttempt(int profileId);

        CurrentGame GetCurrentGameEntity();

        (int P1Score, int P2Score) GetScoresSinceGameStart(int player1Id, int player2Id, DateTime gameStartTime);

        void UpdateTiebreakStatus(int p1Id, int p2Id);
    }
}