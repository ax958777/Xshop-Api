using Api.Data;
using Api.Dto;
using Api.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FileController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("api/[controller]/upload")]
        [RequestSizeLimit(500 * 1024 * 1024)]       //unit is bytes => 500Mb
        [RequestFormLimits(MultipartBodyLengthLimit = 500 * 1024 * 1024)]
        public async Task<ActionResult<List<UploadResultDto>>> UploadFile(List<IFormFile> files)
        {
            List<UploadResultDto> uploadResults = new List<UploadResultDto>();

            foreach (var file in files)
            {
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    var appFile = new AppFile()
                    {
                        Content = ms.ToArray(),
                        FileName = file.FileName,
                        ContentType = file.ContentType,
                    };
                    await _context.files.AddAsync(appFile);
                    await _context.SaveChangesAsync();
                    uploadResults.Add(new UploadResultDto
                    {
                        ContentType = file.ContentType,
                        FileName = file.FileName,
                        Id = appFile.Id
                    });
                }

            }

            return Ok(uploadResults);
        }
        [HttpGet("api/[controller]/{fileId}")]
        public async Task<ActionResult> DownloadFile(string fileId)
        {
            var appFile=await _context.files.FirstOrDefaultAsync(f=>f.Id.Equals(Guid.Parse(fileId)));
            byte[] buffer= null;
            buffer = appFile.Content;
            return File(buffer,appFile.ContentType,appFile.FileName);
        }
    }
}
