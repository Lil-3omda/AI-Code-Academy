using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Platform.Application.ServiceInterfaces
{
    public interface IFileService
    {
        Task<string> UploadVideoAsync(IFormFile file, string folderName = "videos");
        void DeleteFile(string fileUrl);

        Task<int> GetVideoDurationAsync(string filePath);
    }
}
