using Platform.Application.DTOs;
using Platform.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Core.Interfaces
{
    public interface IInstructorService
    {
        Task<(bool Succeeded, string Errors)> RegisterInstructor2Async(RegisterInstructorIfAccountExistsDto dto);
        Task<InstructorReadDto> RegisterInstructorAsync(InstructorRegisterDto dto);
        Task<InstructorDto> GetInstructorByIdAsync(int id);
        Task<IEnumerable<InstructorDto>> GetAllInstructorsAsync();
        Task<(bool Succeeded, string Errors)> VerifyInstructorAsync(int id);
    }
}
