using System;

namespace PiShotProject.Interfaces
{
    public interface IScoreRepository
    {
        void AddScore(int profileId, int? gameId);
        void AddAttempt(int profileId, int? gameId);
        (int p1Score, int p2Score) GetGameScore(int gameId);
        (int p1Attempts, int p2Attempts) GetGameAttempts(int gameId);
    }
}
