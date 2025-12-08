using PiShotProject.Interfaces;
using PiShotProject.Models;

namespace PiShotProject.Services
{
    public class GameService : IGameService
    {
        // Hello World
        private readonly string _hello = "Hello World";


        private readonly IGameRepository _repository;

        public GameService(IGameRepository repository)
        {
            _repository = repository;
        }

        public void DeclareWinner(int winnerId)
        {
            var game = _repository.GetState();
            if (game != null)
            {
                game.CurrentWinnerId = winnerId;
                _repository.SaveState(game);
            }
        }

        public GameStatusResponse GetCurrentStatus()
        {
            var game = _repository.GetState();

            return new GameStatusResponse
            {
                IsActive = game?.IsActive ?? false,
                P1_Id = game?.Player1Id ?? 0,
                P2_Id = game?.Player2Id ?? 0,
                CurrentWinnerId = game?.CurrentWinnerId,
                WinnerName = game.WinnerName,
                WinnerImage = game.WinnerImage
            };
        }

        public void RecordGameResult(int winnerId)
        {
            var game = _repository.GetState();
            if (game == null || !game.IsActive)
            {
                throw new Exception("No active game found");
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

            _repository.SaveState(game);
        }

        public void StartNewGame(StartGameRequest request)
        {
            var game = _repository.GetState();
            if (game == null) return;

            game.Player1Id = request.Player1Id;
            game.Player2Id = request.Player2Id;
            game.IsActive = true;
            game.StartTime = DateTime.UtcNow;
            game.CurrentWinnerId = null;

            _repository.SaveState(game);
        }

        public void StopCurrentGame()
        {
            var game = _repository.GetState();
            if (game == null) return;
            game.IsActive = false;
            game.StartTime = null;
            game.CurrentWinnerId = null;

            _repository.SaveState(game);
        }
    }
}
