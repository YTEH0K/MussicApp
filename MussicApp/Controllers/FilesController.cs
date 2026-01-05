using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MussicApp.Services;

namespace MussicApp.Controllers
{
    [ApiController]
    [Route("files")]
    public class FilesController : ControllerBase
    {
        private readonly IFileStorageService _fileStorage;

        public FilesController(IFileStorageService fileStorage)
        {
            _fileStorage = fileStorage;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFile(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return BadRequest("Invalid ID format");
            }

            try
            {
                var (stream, contentType) = await _fileStorage.DownloadAsync(objectId);

                return File(stream, contentType);
            }
            catch (FileNotFoundException)
            {
                return NotFound("File not found in GridFS");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
