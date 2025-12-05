using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiShotProject.Models
{
    public class Game
    {
        private int id;
        private Profile? profile1 = null;
        private Profile? profile2 = null;
        private int gameWinner = 0;

        public Game() { }

        public Game(int id, Profile? profile1, Profile? profile2, int gameWinner)
        {
            this.id = id;
            this.profile1 = profile1;
            this.profile2 = profile2;
            this.gameWinner = gameWinner;
        }

        public int Id { get { return id; } set { id = value; } }
        public Profile? Profile1 { get { return profile1; } set { profile1 = value; } }
        public Profile? Profile2 { get { return profile2; } set { profile2 = value; } }
        public int GameWinner { get { return gameWinner; } set { gameWinner = value; } }

    }
}
