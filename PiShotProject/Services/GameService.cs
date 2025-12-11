using System;
using PiShotProject.DTO;
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

        public async Task StartNewGameAsync(StartGameRequestDTO request)
        {
            var game = await _repository.GetStateAsync() ?? new CurrentGame { Id = 1 };

            game.Player1Id = request.Player1Id;
            game.Player2Id = request.Player2Id;
            game.IsActive = true;
            game.StartTime = DateTime.UtcNow;

            game.IsTiebreak = false;
            game.TiebreakOffsetP1 = 0;
            game.TiebreakOffsetP2 = 0;
            game.CurrentWinnerId = null;

            await _repository.SaveStateAsync(game);
        }

        public async Task DeclareWinnerAsync(int winnerId)
        {
            var game = await _repository.GetStateAsync();
            if (game != null && game.IsActive)
            {
                game.CurrentWinnerId = winnerId;
                await _repository.SaveStateAsync(game);
            }
        }

        public async Task<GameStatusResponse> GetCurrentStatusAsync()
        {
            var game = await _repository.GetStateAsync();

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

            if (game.CurrentWinnerId.HasValue)
            {
                if (game.CurrentWinnerId == game.Player1Id && game.Player1 != null)
                {
                    winnerName = game.Player1.Name;
                    winnerImage = game.Player1.ProfileImage;
                }
                if (game.CurrentWinnerId == game.Player2Id && game.Player2 != null)
                {
                    winnerName = game.Player2.Name;
                    winnerImage = game.Player2.ProfileImage;
                }
            }

            return new GameStatusResponse
            {
                IsActive = game.IsActive,
                Player1Id = game.Player1Id ?? 0,
                Player2Id = game.Player2Id ?? 0,
                CurrentWinnerId = game.CurrentWinnerId,
                WinnerName = winnerName,
                WinnerImage = winnerImage
            };
        }

        public async Task RecordGameResultAsync(int winnerId)
        {
            var game = await _repository.GetStateAsync();
            if (game == null || !game.IsActive)
            {
                throw new InvalidOperationException("No active game found");
            }

            if (!game.Player1Id.HasValue || !game.Player2Id.HasValue)
            {
                throw new InvalidOperationException("Game does not have both players set.");
            }

            int loserId = (winnerId == game.Player1Id.Value)
                ? game.Player2Id.Value
                : game.Player1Id.Value;

            var result = new GameResult
            {
                WinnerId = winnerId,
                LoserId = loserId,
                PlayedOn = DateTime.UtcNow
            };

            await _repository.AddResultAsync(result);

            game.IsActive = false;
            game.StartTime = null;
            game.CurrentWinnerId = null;
            game.IsTiebreak = false;
            game.TiebreakOffsetP1 = 0;
            game.TiebreakOffsetP2 = 0;

            await _repository.SaveStateAsync(game);
        }

        public async Task StopCurrentGameAsync()
        {
            var game = await _repository.GetStateAsync();
            if (game == null) return;

            game.IsActive = false;
            game.StartTime = null;
            game.CurrentWinnerId = null;
            game.IsTiebreak = false;
            game.TiebreakOffsetP1 = 0;
            game.TiebreakOffsetP2 = 0;

            await _repository.SaveStateAsync(game);
        }
    }
}
