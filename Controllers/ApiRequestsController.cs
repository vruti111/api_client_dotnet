using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_CLIENT.Models;

namespace API_CLIENT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiRequestsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ApiRequestsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ApiRequests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApiRequest>>> GetApiRequests()
        {
            return await _context.ApiRequests.ToListAsync();
        }

        // GET: api/ApiRequests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiRequest>> GetApiRequest(Guid id)
        {
            var apiRequest = await _context.ApiRequests.FindAsync(id);

            if (apiRequest == null)
            {
                return NotFound();
            }

            return apiRequest;
        }

        // POST: api/ApiRequests
        [HttpPost]
        public async Task<ActionResult<ApiRequest>> PostApiRequest(ApiRequest apiRequest)
        {
            _context.ApiRequests.Add(apiRequest);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetApiRequest), new { id = apiRequest.Id }, apiRequest);
        }

        // DELETE: api/ApiRequests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApiRequest(Guid id)
        {
            var apiRequest = await _context.ApiRequests.FindAsync(id);
            if (apiRequest == null)
            {
                return NotFound();
            }

            _context.ApiRequests.Remove(apiRequest);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
