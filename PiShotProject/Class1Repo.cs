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
