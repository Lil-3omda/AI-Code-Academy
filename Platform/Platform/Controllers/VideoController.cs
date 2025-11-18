using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Platform.Application.DTOs;
using Platform.Application.ServiceInterfaces;
using Platform.Core.Models;

namespace Platform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideoController : ControllerBase
    {
        private readonly IVideoService _videoService;
        public VideoController(IVideoService videoService)
        {
            _videoService = videoService;
        }
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        //[RequestFormLimits(
        //    ValueLengthLimit = int.MaxValue,
        //    MultipartBodyLengthLimit = 1024L * 1024L * 1024L,
        //    MultipartHeadersLengthLimit = int.MaxValue,
        //    KeyLengthLimit = int.MaxValue)]
        //[RequestSizeLimit(1024L * 1024L * 1024L)]
        public async Task<IActionResult> Upload([FromForm] VideoUploadDto dto)
        {
            await _videoService.AddVideoAsync(dto.File, dto.Title, dto.ModuleId,dto.VideoArrangement);
            return Ok(new { message = "Video uploaded successfully" });
        }

        [HttpGet("GetAllVideos")]

        public async Task<IActionResult> GetAllVideos()
        {
            var videos = await _videoService.GetAllVideosAsync();
            return Ok(videos);
        }

        [HttpGet("GetVideoById/{id}")]
        public async Task<IActionResult> GetVideoById(int id)
        {
            var video = await _videoService.GetVideoByIdAsync(id);
            if (video == null) return NotFound(new { message = "Video not found" });
            return Ok(video);
        }

        [HttpDelete("DeleteVideo/{id}")]
        public async Task<IActionResult> DeleteVideo(int id)
        {
            await _videoService.DeleteVideoAsync(id);
            return Ok(new { message = "Video deleted successfully" });
        }

        [HttpPut("UpdateVideo")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateVideo([FromForm] VideoUpdateDto dto)
        {
            await _videoService.UpdateVideoAsync(dto);
            return Ok(new { message = "Video updated successfully" });
        }

        [HttpGet("GetByInsId/{insId}")]
        public async Task<IActionResult> GetByInsId(int insId)
        {
            var videos = await _videoService.GetByInsId(insId);
            return Ok(videos);
        }




    }
}
