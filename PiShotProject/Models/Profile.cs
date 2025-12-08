using System;

namespace PiShotProject.Models
{
    public class Profile
    {
        private string _name;
        private string _profileImage;
        public const string DefaultProfileImagePath = "https://st.depositphotos.com/1536130/60618/v/1600/depositphotos_606180794-stock-illustration-basketball-player-hand-drawn-line.jpg";

        public Profile() : this("Default Name", DefaultProfileImagePath) { }

        public Profile(string name, string? profileImagePath = DefaultProfileImagePath)
        {
            Name = name;
            ProfileImage = profileImagePath;
        }

        public int Id { get; set; }

        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Name cannot be null or whitespace");
                if (value.Length < 2 || value.Length > 50)
                    throw new ArgumentOutOfRangeException("Name must be between 2 and 50 characters");
                _name = value;
            }
        }

        public string ProfileImage
        {
            get => _profileImage;
            set => _profileImage = value ?? DefaultProfileImagePath;
        }

        public override string ToString() => $"Profile: {Name}, Image Path: {ProfileImage}";
    }
}
