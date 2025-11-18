using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Core.Interfaces.IRepos;
using Platform.Core.Models;

namespace Platform.Application.IRepos
{
    public interface IVedioRepository:IGenericRepository<Video>
    {
        public Task<IEnumerable<Video>> GetVideosWithCourse();

        public Task<IEnumerable<Video>> GetVideosByInstructorId(int instructorId);
    }
}
