using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiShotProject.ClassDB
{
    public class PiShotDBContext : DbContext
    {
        public PiShotDBContext(DbContextOptions<PiShotDBContext> options) : base(options)
        {
        }

        public DbSet<Class1> Classes { get; set; }

        public DbSet<Profile> Profiles { get; set; }
    }
}
