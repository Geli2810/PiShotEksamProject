using Microsoft.AspNetCore.Mvc;
using PiShotProject;
using PiShotProject.ClassDB;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PiShotWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PiShotController : ControllerBase
    {
        private PiShotDBRepo _context;
        public PiShotController(PiShotDBRepo dBContext)
        {
            _context = dBContext;
        }
        // GET: api/<PiShotController>
        [HttpGet]
        public IEnumerable<Class1> Get()
        {
            return _context.GetAll();
        }

        // GET api/<PiShotController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<PiShotController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<PiShotController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
