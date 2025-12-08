namespace BasketballApi.Models
{
    public class ProfileDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string ProfileImage { get; set; } = "";
        public int Goals { get; set; }
        public int Attempts { get; set; }
        public double Accuracy { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public double WinLossRatio { get; set; }
        public int Rank { get; set; }
    }

    public class CreateProfileRequest
    {
        public string Name { get; set; } = "";
        public string? ProfileImage { get; set; }
    }
}
