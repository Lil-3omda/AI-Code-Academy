using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Platform.Application.DTOs;
using Platform.Core.Models;

namespace Platform.Application.ServiceInterfaces
{
    public interface IVideoService
    {
        Task<IEnumerable<VideoGetAllDto>> GetAllVideosAsync();
        Task<Video?> GetVideoByIdAsync(int id);
        Task AddVideoAsync(IFormFile file, string title, int moduleId,int videoarrang);
        Task UpdateVideoAsync(VideoUpdateDto dto);
        Task DeleteVideoAsync(int id);
        Task<IEnumerable<VideoGetAllDto>> GetByInsId(int insId);
    }
}
