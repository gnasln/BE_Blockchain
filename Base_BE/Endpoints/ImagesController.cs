using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Base_BE.Endpoints
{
	[Route("api/[controller]")]
	[ApiController]
	public class ImagesController : ControllerBase
	{
		
		private readonly IConfiguration _configuration;

		public ImagesController(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		[HttpPost("upload")]
		public async Task<IActionResult> UploadImage(IFormFile file)
		{
			if (file == null || file.Length == 0)
			{
				return BadRequest("No file uploaded.");
			}

			var uploadsFolder = _configuration["storageMount:path"];
			if (string.IsNullOrEmpty(uploadsFolder))
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "Uploads folder is not configured.");
			}

			var filePath = Path.Combine(uploadsFolder, file.FileName);

			try
			{
				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await file.CopyToAsync(stream);
				}
				return Ok(new { FilePath = filePath });
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
			}
		}
	}
}

