using Api.Data;
using Api.Dto;
using Api.Model;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModelingController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        public ModelingController(AppDbContext appContext, UserManager<AppUser> userManager)
        {

            _context = appContext;
            _userManager = userManager;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ModelingResponseDto>>> GetuserModelings()
        {
            var user = await GetCuurentUser();

            if (user is null)
            {
                return NotFound();
            }
            var modeling = _context.models.Include(m => m.Owner).Where(m => m.Owner == user).OrderByDescending(m => m.UpdatedDate).Adapt<List<ModelingResponseDto>>();
            return Ok(modeling);
        }
 
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<ModelingResponseDto>>> GetAllModelings()
        {
           
            var modelings = _context.models.OrderByDescending(m => m.UpdatedDate).Adapt<List<ModelingResponseDto>>();
            return Ok(modelings);
        }
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var modeling = await _context.models.FindAsync(id);
            if (modeling is null)
            {
                return NotFound();
            }

            _context.models.Remove(modeling);
            await _context.SaveChangesAsync();
            return Ok(modeling);
        }

        [Authorize]
        [HttpPost] 
        public async Task<ActionResult<ModelingResponseDto>> Post([FromBody] ModelingRequestDto request)
        {
            var user=await GetCuurentUser();
            if (user is null)
            {
                return NotFound();
            }
            var newModel = new Modeling
            {
                Owner = user,
                Category = request.Category,    
                Description = request.Description,
                Name = request.Name,
                Price = request.Price,
                Models = request.Models,
            };
            _context.models.Add(newModel);
            await _context.SaveChangesAsync();
            return Ok();
        }


        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<ModelingResponseDto>> Put(Guid id,[FromBody] ModelingRequestDto request)
        {
            var modeling = await _context.models.FindAsync(id);
            if(modeling is null)
            {
                return NotFound();
            }

        
            modeling.Price = request.Price;
            modeling.UpdatedDate = DateTime.UtcNow;
            modeling.Category = request.Category;   
            modeling.Description = request.Description; 
            modeling.Name = request.Name;
            modeling.Models = request.Models;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<AppUser> GetCuurentUser()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(currentUserId!);
            return user;
        }
    }

}
