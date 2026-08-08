using SchoolManagement.BLL.DTOs.Student;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.BLL.Interfaces;

public interface IStudentService
{
    Task<StudentListResponseDto> GetStudentsAsync(StudentSearchFilter filter, CancellationToken cancellationToken = default);
    Task<StudentResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<StudentResponseDto> CreateAdmissionAsync(CreateAdmissionDto dto, CancellationToken cancellationToken = default);
    Task<StudentResponseDto> UpdateAdmissionAsync(Guid id, UpdateAdmissionDto dto, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<StudentResponseDto> UploadProfilePictureAsync(Guid id, Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task<StudentResponseDto> UploadGuardianPictureAsync(Guid studentId, Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> GetAcademicYearsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdmissionLookupItemDto>> GetClassesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdmissionLookupItemDto>> GetSectionsAsync(Guid classId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdmissionLookupItemDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdmissionLookupItemDto>> GetTransportRoutesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdmissionLookupItemDto>> GetHostelsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdmissionLookupItemDto>> GetHostelRoomsAsync(Guid hostelId, CancellationToken cancellationToken = default);
    Task<NextRegisterNoDto> GetNextRegisterNoAsync(int? academicYear = null, CancellationToken cancellationToken = default);
}
