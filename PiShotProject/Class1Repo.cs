using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiShotProject
{
    public class Class1Repo
    {
        private List<Class1> _class1Repo;

        private int _id = 1;
        public Class1Repo()
        {
            _class1Repo = new List<Class1>()
            {
                new Class1(_id++,2020, 5, 15),
                new Class1(_id ++, 2021, 6, 20),
                new Class1(_id ++, 2022, 7, 25),
                new Class1(_id ++, 2023, 8, 30),
                new Class1(_id ++, 2024, 9, 10), 
                new Class1(_id ++, 2025, 10, 15),
                new Class1(_id ++, 2026, 11, 20),
                new Class1(_id ++, 2027, 12, 25), 
                new Class1(_id ++, 2028, 1, 10),
                new Class1(_id ++, 2029, 2, 15),
                new Class1(_id ++, 2030, 3, 20), 
                new Class1( _id ++, 2031, 4, 25)


            };
        }

        public List<Class1> GetAll()
        {
            return _class1Repo;
        }

        public void Add(Class1 class1)
        {
            _class1Repo.Add(class1);
        }
    }
}
