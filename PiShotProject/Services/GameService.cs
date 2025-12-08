using PiShotProject.Interfaces;
using PiShotProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiShotProject.Services
{
    public class GameService : IGameService
    {
        private readonly IGameRepostitory _repository;

        public GameService(IGameRepostitory repository)
        {
            _repository = repository;
        }

        public void DeclareWinner(int winnerId)
        {
            _repository.UpdateCurrentWinner(winnerId);
        }

        public GameStatusResponse GetCurrentStatus()
        {
            var game = _repository.GetCurrentGame();

            if (game == null) return new GameStatusResponse { IsActive = false };

            return new GameStatusResponse
            {
                IsActive = game.IsActive,
                P1_Id = game.Player1Id,
                P2_Id = game.Player2Id,
                CurrentWinnerId = game.CurrentWinnerId,
                WinnerName = game.WinnerName,
                WinnerImage = game.WinnerImage
            };
        }

        public void RecordGameResult(int winnerId)
        {
            var currentGame = _repository.GetCurrentGame();
            if (currentGame == null || !currentGame.IsActive)
            {
                throw new InvalidOperationException("No active game found.");
            }

            int loserId = (winnerId == currentGame.Player1Id) ? currentGame.Player2Id : currentGame.Player1Id;

            _repository.AddGameResult(winnerId, loserId);

            _repository.ResetGame();
        }

        public void StartNewGame(StartGameRequest request)
        {
            _repository.StartGame(request.Player1Id, request.Player2Id);
        }
    }
}
