using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiShotProject.ClassDB
{
    public class PiShotDBRepo
    {
        private PiShotDBContext _context;
        public PiShotDBRepo(PiShotDBContext context) 
        {
            _context = context;
        }

        public List<Class1> GetAll()
        {
            return _context.Classes.ToList();
        }


        public void Add(Class1 class1)
        {
            _context.Classes.Add(class1);
            _context.SaveChanges();
        }
    }
}
