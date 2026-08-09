using SchoolManagement.BLL.Interfaces;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class BiometricPunchProcessor(IUnitOfWork uow) : IBiometricPunchProcessor
{
    public async Task<BiometricPunchLog> ProcessPunchAsync(
        Guid? deviceId,
        string deviceSn,
        int graceMinutesBefore,
        int graceMinutesAfter,
        string devicePin,
        DateTime punchTime,
        string? rawLine,
        CancellationToken cancellationToken = default)
    {
        var log = new BiometricPunchLog
        {
            Id = Guid.NewGuid(),
            DeviceId = deviceId,
            DeviceSn = deviceSn,
            DevicePin = devicePin,
            PunchTime = punchTime,
            PunchKind = "Unmapped",
            StatusApplied = "Present",
            RawLine = rawLine,
            CreatedAt = DateTime.UtcNow
        };

        var map = await uow.BiometricUserMaps.GetByPinAsync(devicePin, activeOnly: true, cancellationToken);
        if (map is null)
        {
            await uow.BiometricPunchLogs.AddAsync(log, cancellationToken);
            await uow.SaveTenantChangesAsync(cancellationToken);
            return log;
        }

        var remarks = $"ZKTeco {deviceSn}";

        if (string.Equals(map.PersonType, "Student", StringComparison.OrdinalIgnoreCase) && map.StudentId.HasValue)
        {
            await ProcessStudentPunchAsync(log, map.StudentId.Value, graceMinutesBefore, graceMinutesAfter, punchTime, remarks, cancellationToken);
        }
        else if (string.Equals(map.PersonType, "Employee", StringComparison.OrdinalIgnoreCase) && map.EmployeeId.HasValue)
        {
            await ProcessEmployeePunchAsync(log, map.EmployeeId.Value, punchTime, remarks, cancellationToken);
        }

        await uow.BiometricPunchLogs.AddAsync(log, cancellationToken);
        await uow.SaveTenantChangesAsync(cancellationToken);
        return log;
    }

    private async Task ProcessStudentPunchAsync(
        BiometricPunchLog log, Guid studentId, int graceBefore, int graceAfter,
        DateTime punchTime, string remarks, CancellationToken cancellationToken)
    {
        var student = await uow.Students.GetByIdAsync(studentId, cancellationToken);
        if (student is null || !student.ClassId.HasValue || !student.SectionId.HasValue)
            return;

        log.StudentId = student.Id;

        var subject = await uow.ExamSchedules.FindExamSubjectForPunchAsync(
            student.ClassId.Value, student.SectionId.Value, punchTime.Date, punchTime.TimeOfDay,
            graceBefore, graceAfter, cancellationToken);

        if (subject is not null)
        {
            log.PunchKind = "Exam";
            log.ExamId = subject.Schedule.ExamId;
            log.SubjectId = subject.SubjectId;

            await uow.ExamAttendances.UpsertBatchAsync(
            [
                new ExamAttendance
                {
                    Id = Guid.NewGuid(),
                    ExamId = subject.Schedule.ExamId,
                    SubjectId = subject.SubjectId,
                    StudentId = student.Id,
                    ClassId = student.ClassId.Value,
                    SectionId = student.SectionId.Value,
                    Status = "Present",
                    Remarks = remarks,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            ], cancellationToken);
        }
        else
        {
            log.PunchKind = "StudentDaily";
        }

        await uow.StudentAttendances.UpsertBatchAsync(
        [
            new StudentAttendance
            {
                Id = Guid.NewGuid(),
                StudentId = student.Id,
                ClassId = student.ClassId.Value,
                SectionId = student.SectionId.Value,
                AttendanceDate = punchTime.Date,
                Status = "Present",
                Remarks = remarks,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        ], cancellationToken);
    }

    private async Task ProcessEmployeePunchAsync(
        BiometricPunchLog log, Guid employeeId, DateTime punchTime, string remarks, CancellationToken cancellationToken)
    {
        var employee = await uow.Employees.GetByIdAsync(employeeId, cancellationToken);
        if (employee is null)
            return;

        log.EmployeeId = employee.Id;
        log.PunchKind = "EmployeeDaily";

        await uow.EmployeeAttendances.UpsertBatchAsync(
        [
            new EmployeeAttendance
            {
                Id = Guid.NewGuid(),
                EmployeeId = employee.Id,
                AttendanceDate = punchTime.Date,
                Status = "Present",
                Remarks = remarks,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        ], cancellationToken);
    }
}
