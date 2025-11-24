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
        public Class1Repo()
        {
            _class1Repo = new List<Class1>()
            {
                new Class1(2020, 5, 15),
                new Class1(2021, 6, 20),
                new Class1(2022, 7, 25),
                new Class1(2023, 8, 30), 
                new Class1(2024, 9, 10), 
                new Class1(2025, 10, 15),
                new Class1(2026, 11, 20),
                new Class1(2027, 12, 25), 
                new Class1(2028, 1, 10),
                new Class1(2029, 2, 15),
                new Class1(2030, 3, 20), 
                new Class1(2031, 4, 25)


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
