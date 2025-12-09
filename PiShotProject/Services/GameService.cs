using System;
using PiShotProject.Interfaces;
using PiShotProject.Models;

namespace PiShotProject.Services
{
    public class GameService : IGameService
    {
        private readonly IGameRepository _repository;

        public GameService(IGameRepository repository)
        {
            _repository = repository;
        }

        public void StartNewGame(StartGameRequest request)
        {
            var game = _repository.GetState() ?? new CurrentGame { Id = 1 };

            game.Player1Id = request.Player1Id;
            game.Player2Id = request.Player2Id;
            game.IsActive = true;
            game.StartTime = DateTime.UtcNow;

            game.IsTiebreak = false;
            game.TiebreakOffsetP1 = 0;
            game.TiebreakOffsetP2 = 0;
            game.CurrentWinnerId = null;

            _repository.SaveState(game);
        }

        public void DeclareWinner(int winnerId)
        {
            var game = _repository.GetState();
            if (game != null && game.IsActive)
            {
                game.CurrentWinnerId = winnerId;
                _repository.SaveState(game);
            }
        }

        public GameStatusResponse GetCurrentStatus()
        {
            var game = _repository.GetState();

            if (game == null)
            {
                return new GameStatusResponse
                {
                    IsActive = false,
                    Player1Id = 0,
                    Player2Id = 0,
                    CurrentWinnerId = null,
                    WinnerName = null,
                    WinnerImage = null
                };
            }

            string? winnerName = null;
            string? winnerImage = null;

            // Her finder vi winner-navn og -billede ud fra WinnerId + Profile
            if (game.CurrentWinnerId.HasValue)
            {
                if (game.CurrentWinnerId.Value == game.Player1Id && game.Player1 != null)
                {
                    winnerName = game.Player1.Name;
                    winnerImage = game.Player1.ProfileImage;
                }
                else if (game.CurrentWinnerId.Value == game.Player2Id && game.Player2 != null)
                {
                    winnerName = game.Player2.Name;
                    winnerImage = game.Player2.ProfileImage;
                }
            }

            return new GameStatusResponse
            {
                IsActive = game.IsActive,
                Player1Id = game.Player1Id,
                Player2Id = game.Player2Id,
                CurrentWinnerId = game.CurrentWinnerId,
                WinnerName = winnerName,
                WinnerImage = winnerImage
            };
        }

        public void RecordGameResult(int winnerId)
        {
            var game = _repository.GetState();
            if (game == null || !game.IsActive)
            {
                throw new InvalidOperationException("No active game found");
            }

            int loserId = (winnerId == game.Player1Id) ? game.Player2Id : game.Player1Id;

            var result = new GameResult
            {
                WinnerId = winnerId,
                LoserId = loserId,
                PlayedOn = DateTime.UtcNow
            };

            _repository.AddResult(result);

            game.IsActive = false;
            game.StartTime = null;
            game.CurrentWinnerId = null;
            game.IsTiebreak = false;
            game.TiebreakOffsetP1 = 0;
            game.TiebreakOffsetP2 = 0;

            _repository.SaveState(game);
        }

        public void StopCurrentGame()
        {
            var game = _repository.GetState();
            if (game == null) return;

            game.IsActive = false;
            game.StartTime = null;
            game.CurrentWinnerId = null;
            game.IsTiebreak = false;
            game.TiebreakOffsetP1 = 0;
            game.TiebreakOffsetP2 = 0;

            _repository.SaveState(game);
        }
    }
}
