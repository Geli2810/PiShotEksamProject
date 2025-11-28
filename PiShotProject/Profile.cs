using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiShotProject
{
    public class Profile
    {
        private int _id;
        private string _name;
        private string _profileImagePath;

        public Profile(string name, string profileImagePath) 
        {
            Name = name;
            ProfileImagePath = profileImagePath;
        }

        public Profile() : this("Default Name", "default/path/to/image.png") { }

        public int Id
        { get; set; }

        public string Name
        {
            get { return _name; }
            set 
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Name cannot be null or whitespace");
                }
                if (value.Length < 2 || value.Length > 50)
                {
                    throw new ArgumentOutOfRangeException("Name must be between 2 and 50 characters");
                }
                _name = value; 

            }
        }
        public string ProfileImagePath
        {
            get { return _profileImagePath; }
            set 
            { 
                if(string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Profile image path cannot be null or empty");
                }
                _profileImagePath = value; 
            }
        }

        public override string ToString()
        {
            return $"Profile: {Name}, Image Path: {ProfileImagePath}";
        }

    }
}
