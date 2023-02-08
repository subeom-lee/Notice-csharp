using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notice.Data;

namespace Notice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PostController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpDelete]
        [Route("delete")]
        public async Task<IActionResult> Delete(string id)
        {
            string[] splitId = id.Split(",");
            Console.WriteLine(splitId);

            if (_context.Posts == null)
            {
                return BadRequest();
            }

            bool bRemove = false;

            for (int i = 0; i < splitId.Length; i++)
            {
                var intId = int.Parse(splitId[i]);
                var post = await _context.Posts.FindAsync(intId);
                if (post != null)
                {
                    _context.Posts.Remove(post);
                    bRemove = true;
                }
            }

            if (bRemove)
                await _context.SaveChangesAsync();

            return Ok();
        }
    }
}