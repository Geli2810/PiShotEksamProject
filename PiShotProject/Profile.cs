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
        {
            get { return _id; }
        }

        public string Name
        {
            get { return _name; }
            set 
            {
                if (_name == null)
                {
                    throw new ArgumentNullException("Name cannot be null");
                }
                _name = value; 
            }
        }
        public string ProfileImagePath
        {
            get { return _profileImagePath; }
            set { _profileImagePath = value; }
        }

        public override string ToString()
        {
            return $"Profile: {Name}, Image Path: {ProfileImagePath}";
        }

    }
}
