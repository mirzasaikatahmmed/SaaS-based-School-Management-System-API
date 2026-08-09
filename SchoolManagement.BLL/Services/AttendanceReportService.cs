using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.Reports;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class AttendanceReportService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IStorageService storage,
    IHttpContextAccessor http) : IAttendanceReportService
{
    private static readonly AttendanceLegendItemDto[] StudentLegend =
    [
        new() { Code = "W", Label = "Weekends" },
        new() { Code = "P", Label = "Present" },
        new() { Code = "A", Label = "Absent" },
        new() { Code = "H", Label = "Holiday" },
        new() { Code = "L", Label = "Late" },
        new() { Code = "HD", Label = "Half Day" }
    ];

    private static readonly AttendanceLegendItemDto[] SubjectLegend =
    [
        new() { Code = "P", Label = "Present" },
        new() { Code = "A", Label = "Absent" },
        new() { Code = "L", Label = "Late" },
        new() { Code = "HD", Label = "Half Day" }
    ];

    public async Task<MonthlyAttendanceGridDto> GetStudentMonthlyAsync(
        Guid classId, Guid sectionId, int year, int month, CancellationToken ct = default)
    {
        await Ready(ct);
        ManageOrTeacher();
        ValidateMonth(year, month);

        var (from, to) = MonthRange(year, month);
        var students = await uow.StudentAttendances.GetActiveStudentsAsync(classId, sectionId, ct);
        var records = await uow.StudentAttendances.GetReportAsync(classId, sectionId, null, from, to, ct);
        var byStudentDate = records
            .GroupBy(r => r.StudentId)
            .ToDictionary(g => g.Key, g => g.ToDictionary(x => x.AttendanceDate.Date, x => x.Status));

        var (className, sectionName) = await ResolveClassSectionAsync(classId, sectionId, ct);
        var weekends = await ResolveWeekendsAsync(ct);
        var holidays = (await uow.Events.GetHolidayDatesAsync(from, to, ct)).ToHashSet();
        var dayColumns = BuildDayColumns(year, month, weekends, holidays);

        var rows = students.Select(s =>
        {
            byStudentDate.TryGetValue(s.Id, out var map);
            map ??= new Dictionary<DateTime, string>();
            return BuildPersonRow(s.Id, StudentName(s), s.RegisterNo, s.Roll, year, month, dayColumns, map, weekends, holidays);
        }).ToList();

        return new MonthlyAttendanceGridDto
        {
            Title = $"Attendance Report — {CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month)} {year}",
            Year = year,
            Month = month,
            ClassId = classId,
            SectionId = sectionId,
            ClassName = className,
            SectionName = sectionName,
            Legend = StudentLegend,
            DayColumns = dayColumns,
            Rows = rows
        };
    }

    public async Task<StudentDailyClassReportDto> GetStudentDailyAsync(DateTime date, CancellationToken ct = default)
    {
        await Ready(ct);
        ManageOrTeacher();

        var day = date.Date;
        var classes = await uow.ClassControls.GetAllWithSectionsAsync(ct);
        var records = await uow.StudentAttendances.GetReportAsync(null, null, null, day, day, ct);
        var byClassSection = records
            .GroupBy(r => (r.ClassId, r.SectionId))
            .ToDictionary(g => g.Key, g => g.ToList());

        var rows = new List<StudentDailyClassReportRowDto>();
        var sl = 1;
        var grandPresent = 0;
        var grandAbsent = 0;

        foreach (var cls in classes.OrderBy(c => c.Name))
        {
            var sections = cls.ClassSections?
                .Select(cs => cs.Section)
                .Where(s => s is not null)
                .Cast<Section>()
                .ToList() ?? [];

            if (sections.Count == 0) continue;

            foreach (var section in sections.OrderBy(s => s.Name))
            {
                byClassSection.TryGetValue((cls.Id, section.Id), out var list);
                list ??= [];
                if (list.Count == 0)
                {
                    var enrolled = await uow.StudentAttendances.GetActiveStudentsAsync(cls.Id, section.Id, ct);
                    if (enrolled.Count == 0) continue;
                }

                var (p, a, row) = BuildDailyRow(sl++, cls.Id, section.Id, cls.Name, section.Name, list);
                grandPresent += p;
                grandAbsent += a;
                rows.Add(row);
            }
        }

        var total = grandPresent + grandAbsent;
        return new StudentDailyClassReportDto
        {
            Date = day,
            Rows = rows,
            TotalPresent = grandPresent,
            TotalAbsent = grandAbsent,
            PresentPercent = Pct(grandPresent, total),
            AbsentPercent = Pct(grandAbsent, total)
        };
    }

    public async Task<StudentOverviewReportDto> GetStudentOverviewAsync(
        Guid classId, Guid sectionId, string attendanceType, DateTime fromDate, DateTime toDate,
        CancellationToken ct = default)
    {
        await Ready(ct);
        ManageOrTeacher();

        var status = NormalizeStatus(attendanceType);
        var from = fromDate.Date;
        var to = toDate.Date;
        if (to < from) throw new AppException("toDate must be on or after fromDate.", 400);

        var students = await uow.StudentAttendances.GetActiveStudentsAsync(classId, sectionId, ct);
        var records = await uow.StudentAttendances.GetReportAsync(classId, sectionId, null, from, to, ct);
        var counts = records
            .Where(r => status.Equals(r.Status, StringComparison.OrdinalIgnoreCase))
            .GroupBy(r => r.StudentId)
            .ToDictionary(g => g.Key, g => g.Count());

        var (className, sectionName) = await ResolveClassSectionAsync(classId, sectionId, ct);
        var classLabel = string.IsNullOrWhiteSpace(sectionName) ? className : $"{className} ({sectionName})";

        // Enrich category names when available on navigation
        var detailed = await uow.Students.SearchAsync(new DAL.Repositories.Interfaces.StudentSearchFilter
        {
            ClassId = classId,
            SectionId = sectionId,
            IsActive = true,
            Page = 1,
            PageSize = 5000
        }, ct);

        var byId = detailed.Items.ToDictionary(s => s.Id);

        var rows = students.Select(s =>
        {
            byId.TryGetValue(s.Id, out var full);
            counts.TryGetValue(s.Id, out var count);
            return new StudentOverviewReportRowDto
            {
                StudentId = s.Id,
                StudentName = StudentName(full ?? s),
                RegisterNo = (full ?? s).RegisterNo,
                AdmissionDate = (full ?? s).AdmissionDate,
                Category = full?.Category?.Name,
                ClassName = classLabel,
                Gender = (full ?? s).Gender,
                MobileNo = (full ?? s).MobileNo,
                Count = count
            };
        }).OrderBy(r => r.StudentName).ToList();

        return new StudentOverviewReportDto
        {
            AttendanceType = status,
            FromDate = from,
            ToDate = to,
            Rows = rows
        };
    }

    public async Task<SubjectWiseByDateReportDto> GetSubjectWiseAsync(
        Guid classId, Guid sectionId, Guid subjectId, DateTime date, CancellationToken ct = default)
    {
        await Ready(ct);
        ManageOrTeacher();

        var day = date.Date;
        var students = await uow.StudentAttendances.GetActiveStudentsAsync(classId, sectionId, ct);
        var existing = await uow.StudentSubjectAttendances.GetForDateAsync(classId, sectionId, subjectId, day, ct);
        var byStudent = existing.ToDictionary(a => a.StudentId);
        var (className, sectionName) = await ResolveClassSectionAsync(classId, sectionId, ct);
        var subject = await uow.Subjects.GetByIdAsync(subjectId, ct)
            ?? throw new NotFoundException("Subject not found.");

        var rows = students.Select((s, i) =>
        {
            byStudent.TryGetValue(s.Id, out var a);
            return new SubjectWiseByDateRowDto
            {
                Sl = i + 1,
                StudentId = s.Id,
                StudentName = StudentName(s),
                RegisterNo = s.RegisterNo,
                Roll = s.Roll,
                SubjectName = SubjectLabel(subject),
                Status = a?.Status ?? string.Empty,
                Remarks = a?.Remarks
            };
        }).ToList();

        return new SubjectWiseByDateReportDto
        {
            Date = day,
            ClassId = classId,
            SectionId = sectionId,
            SubjectId = subjectId,
            ClassName = className,
            SectionName = sectionName,
            SubjectName = SubjectLabel(subject),
            Legend = SubjectLegend,
            Rows = rows
        };
    }

    public async Task<SubjectWiseDayReportDto> GetSubjectWiseByDayAsync(
        Guid classId, Guid sectionId, DateTime date, CancellationToken ct = default)
    {
        await Ready(ct);
        ManageOrTeacher();

        var day = date.Date;
        var students = await uow.StudentAttendances.GetActiveStudentsAsync(classId, sectionId, ct);
        var existing = await uow.StudentSubjectAttendances.GetForDateAsync(classId, sectionId, null, day, ct);
        var subjects = existing
            .Select(a => a.Subject)
            .Where(s => s is not null)
            .DistinctBy(s => s!.Id)
            .OrderBy(s => s!.Name)
            .Select(s => new SubjectColumnDto
            {
                SubjectId = s!.Id,
                Name = s.Name,
                Code = s.Code
            }).ToList();

        // Also include assigned class subjects so empty day still has columns when configured
        if (subjects.Count == 0)
        {
            var assigned = await uow.ClassSubjectAssignments.GetAllAsync(ct);
            subjects = assigned
                .Where(a => a.ClassId == classId && a.SectionId == sectionId)
                .SelectMany(a => a.Items)
                .Select(i => i.Subject)
                .Where(s => s is not null)
                .DistinctBy(s => s!.Id)
                .OrderBy(s => s!.Name)
                .Select(s => new SubjectColumnDto
                {
                    SubjectId = s!.Id,
                    Name = s.Name,
                    Code = s.Code
                }).ToList();
        }

        var lookup = existing
            .GroupBy(a => a.StudentId)
            .ToDictionary(
                g => g.Key,
                g => g.ToDictionary(x => x.SubjectId.ToString(), x => (string?)ToCode(x.Status)));

        var (className, sectionName) = await ResolveClassSectionAsync(classId, sectionId, ct);

        var rows = students.Select(s =>
        {
            lookup.TryGetValue(s.Id, out var map);
            map ??= new Dictionary<string, string?>();
            foreach (var sub in subjects)
                map.TryAdd(sub.SubjectId.ToString(), null);
            return new SubjectWiseDayReportRowDto
            {
                StudentId = s.Id,
                StudentName = StudentName(s),
                RegisterNo = s.RegisterNo,
                Roll = s.Roll,
                SubjectStatuses = map
            };
        }).ToList();

        return new SubjectWiseDayReportDto
        {
            Date = day,
            ClassId = classId,
            SectionId = sectionId,
            ClassName = className,
            SectionName = sectionName,
            Legend = SubjectLegend,
            Subjects = subjects,
            Rows = rows
        };
    }

    public async Task<MonthlyAttendanceGridDto> GetSubjectWiseByMonthAsync(
        Guid classId, Guid sectionId, Guid subjectId, int year, int month, CancellationToken ct = default)
    {
        await Ready(ct);
        ManageOrTeacher();
        ValidateMonth(year, month);

        var (from, to) = MonthRange(year, month);
        var students = await uow.StudentAttendances.GetActiveStudentsAsync(classId, sectionId, ct);
        var records = await uow.StudentSubjectAttendances.GetRangeAsync(classId, sectionId, subjectId, from, to, ct);
        var byStudentDate = records
            .GroupBy(r => r.StudentId)
            .ToDictionary(g => g.Key, g => g.ToDictionary(x => x.AttendanceDate.Date, x => x.Status));

        var (className, sectionName) = await ResolveClassSectionAsync(classId, sectionId, ct);
        var subject = await uow.Subjects.GetByIdAsync(subjectId, ct)
            ?? throw new NotFoundException("Subject not found.");
        var weekends = await ResolveWeekendsAsync(ct);
        var holidays = (await uow.Events.GetHolidayDatesAsync(from, to, ct)).ToHashSet();
        var dayColumns = BuildDayColumns(year, month, weekends, holidays);

        var rows = students.Select(s =>
        {
            byStudentDate.TryGetValue(s.Id, out var map);
            map ??= new Dictionary<DateTime, string>();
            return BuildPersonRow(s.Id, StudentName(s), s.RegisterNo, s.Roll, year, month, dayColumns, map, weekends, holidays);
        }).ToList();

        return new MonthlyAttendanceGridDto
        {
            Title = $"Period Attendance Sheet of {CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month)} {year}",
            Year = year,
            Month = month,
            ClassId = classId,
            SectionId = sectionId,
            SubjectId = subjectId,
            ClassName = className,
            SectionName = sectionName,
            SubjectName = SubjectLabel(subject),
            Legend = StudentLegend,
            DayColumns = dayColumns,
            Rows = rows
        };
    }

    public async Task<MonthlyAttendanceGridDto> GetEmployeeMonthlyAsync(
        string? role, int year, int month, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        ValidateMonth(year, month);

        var (from, to) = MonthRange(year, month);
        var employees = await uow.EmployeeAttendances.GetActiveEmployeesByRoleAsync(role, ct);
        var records = await uow.EmployeeAttendances.GetReportAsync(role, null, from, to, ct);
        var byEmpDate = records
            .GroupBy(r => r.EmployeeId)
            .ToDictionary(g => g.Key, g => g.Where(x => x.Status is not null)
                .ToDictionary(x => x.AttendanceDate.Date, x => x.Status!));

        var weekends = await ResolveWeekendsAsync(ct);
        var holidays = (await uow.Events.GetHolidayDatesAsync(from, to, ct)).ToHashSet();
        var dayColumns = BuildDayColumns(year, month, weekends, holidays);

        var rows = employees.Select(e =>
        {
            byEmpDate.TryGetValue(e.Id, out var map);
            map ??= new Dictionary<DateTime, string>();
            return BuildPersonRow(e.Id, e.Name, e.StaffId, null, year, month, dayColumns, map, weekends, holidays);
        }).ToList();

        return new MonthlyAttendanceGridDto
        {
            Title = $"Employee Attendance — {CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month)} {year}",
            Year = year,
            Month = month,
            Role = role,
            Legend = StudentLegend,
            DayColumns = dayColumns,
            Rows = rows
        };
    }

    public async Task<ExamAttendanceReportDto> GetExamReportAsync(
        Guid examId, Guid classId, Guid sectionId, Guid subjectId, CancellationToken ct = default)
    {
        await Ready(ct);
        ManageOrTeacher();

        var students = await uow.ExamAttendances.GetActiveStudentsAsync(classId, sectionId, ct);
        var existing = await uow.ExamAttendances.GetForFilterAsync(examId, classId, sectionId, subjectId, ct);
        var byStudent = existing.ToDictionary(a => a.StudentId);
        var (className, sectionName) = await ResolveClassSectionAsync(classId, sectionId, ct);
        var subject = await uow.Subjects.GetByIdAsync(subjectId, ct)
            ?? throw new NotFoundException("Subject not found.");
        var exam = await uow.Exams.GetByIdAsync(examId, ct)
            ?? throw new NotFoundException("Exam not found.");

        var rows = students.Select((s, i) =>
        {
            byStudent.TryGetValue(s.Id, out var a);
            return new ExamAttendanceReportRowDto
            {
                Sl = i + 1,
                StudentId = s.Id,
                Name = StudentName(s),
                RegisterNo = s.RegisterNo,
                Roll = s.Roll,
                Subject = SubjectLabel(subject),
                Remarks = a?.Remarks,
                Status = a?.Status ?? string.Empty
            };
        }).ToList();

        return new ExamAttendanceReportDto
        {
            ExamId = examId,
            ClassId = classId,
            SectionId = sectionId,
            SubjectId = subjectId,
            ExamName = exam.Name,
            ClassName = className,
            SectionName = sectionName,
            SubjectName = SubjectLabel(subject),
            Rows = rows
        };
    }

    public async Task<FingerprintLogReportDto> GetFingerprintLogsAsync(
        FingerprintLogFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        ManageOrTeacher();

        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = string.Equals(filter.Export, "csv", StringComparison.OrdinalIgnoreCase)
            ? Math.Clamp(filter.PageSize is < 1 ? 5000 : filter.PageSize, 1, 5000)
            : Math.Clamp(filter.PageSize is < 1 ? 100 : filter.PageSize, 1, 5000);

        var from = NormalizeFrom(filter.From);
        var to = NormalizeTo(filter.To);

        var (items, total) = await uow.BiometricPunchLogs.GetFilteredAsync(new DAL.Repositories.Interfaces.BiometricPunchLogFilter
        {
            From = from,
            To = to,
            DeviceId = filter.DeviceId,
            Kind = filter.Kind,
            StudentId = filter.StudentId,
            EmployeeId = filter.EmployeeId,
            DevicePin = filter.DevicePin,
            Search = filter.Search,
            Role = filter.Role,
            ClassId = filter.ClassId,
            SectionId = filter.SectionId,
            Page = page,
            PageSize = size
        }, ct);

        var rows = new List<FingerprintLogRowDto>(items.Count);
        for (var i = 0; i < items.Count; i++)
        {
            var p = items[i];
            string name = "Unmapped";
            string? roll = null;
            string? admissionNo = null;
            string role = "Unmapped";
            string? className = null;
            string? sectionName = null;
            string? photo = null;

            if (p.Student is not null)
            {
                name = StudentName(p.Student);
                roll = p.Student.Roll;
                admissionNo = p.Student.RegisterNo;
                role = "Student";
                className = p.Student.Class?.Name;
                sectionName = p.Student.Section?.Name;
                photo = await ResolvePhotoAsync(p.Student.ProfilePictureUrl, ct);
            }
            else if (p.Employee is not null)
            {
                name = p.Employee.Name;
                admissionNo = p.Employee.StaffId;
                role = p.Employee.Role;
                photo = await ResolvePhotoAsync(p.Employee.ProfilePictureUrl, ct);
            }

            rows.Add(new FingerprintLogRowDto
            {
                Sl = (page - 1) * size + i + 1,
                Id = p.Id,
                PhotoUrl = photo,
                Name = name,
                Roll = roll,
                AdmissionNo = admissionNo,
                RegisterId = p.DevicePin,
                PunchTime = p.PunchTime,
                PunchTimeIso = p.PunchTime.ToString("O"),
                PunchDate = p.PunchTime.ToString("yyyy-MM-dd"),
                PunchClock = p.PunchTime.ToString("HH:mm:ss"),
                TerminalId = p.DeviceSn,
                DeviceName = p.Device?.Name,
                ReceivedAt = p.CreatedAt,
                ReceivedAtIso = p.CreatedAt.ToString("O"),
                Role = role,
                ClassName = className,
                SectionName = sectionName,
                StudentId = p.StudentId,
                EmployeeId = p.EmployeeId,
                DeviceId = p.DeviceId,
                PunchKind = p.PunchKind,
                StatusApplied = p.StatusApplied,
                RawLine = p.RawLine
            });
        }

        return new FingerprintLogReportDto
        {
            Role = filter.Role,
            ClassId = filter.ClassId,
            SectionId = filter.SectionId,
            From = from,
            To = to,
            Rows = rows,
            TotalCount = total,
            Page = page,
            PageSize = size,
            TotalPages = (int)Math.Ceiling(total / (double)size)
        };
    }

    private async Task<string?> ResolvePhotoAsync(string? objectKey, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(objectKey) || string.IsNullOrEmpty(tenant.TenantSlug))
            return objectKey;
        try { return await storage.GetPresignedUrlAsync(tenant.TenantSlug!, objectKey, ct); }
        catch { return objectKey; }
    }

    /// <summary>If time is midnight, treat as start of that calendar day (inclusive).</summary>
    private static DateTime? NormalizeFrom(DateTime? value)
    {
        if (!value.HasValue) return null;
        var v = value.Value;
        return v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime();
    }

    /// <summary>If time is midnight, treat as end of that calendar day (inclusive of all punches that day).</summary>
    private static DateTime? NormalizeTo(DateTime? value)
    {
        if (!value.HasValue) return null;
        var v = value.Value;
        if (v.Kind == DateTimeKind.Unspecified) v = DateTime.SpecifyKind(v, DateTimeKind.Utc);
        else v = v.ToUniversalTime();
        if (v.TimeOfDay == TimeSpan.Zero)
            v = v.Date.AddDays(1).AddTicks(-1);
        return v;
    }

    private static (int Present, int Absent, StudentDailyClassReportRowDto Row) BuildDailyRow(
        int sl, Guid classId, Guid? sectionId, string className, string? sectionName,
        IReadOnlyList<StudentAttendance> list)
    {
        var present = list.Count(a =>
            a.Status.Equals("Present", StringComparison.OrdinalIgnoreCase) ||
            a.Status.Equals("Late", StringComparison.OrdinalIgnoreCase) ||
            a.Status.Equals("HalfDay", StringComparison.OrdinalIgnoreCase));
        var absent = list.Count(a => a.Status.Equals("Absent", StringComparison.OrdinalIgnoreCase));
        var total = present + absent;
        var label = string.IsNullOrWhiteSpace(sectionName) ? className : $"{className} ({sectionName})";
        return (present, absent, new StudentDailyClassReportRowDto
        {
            Sl = sl,
            ClassId = classId,
            SectionId = sectionId,
            ClassName = label,
            SectionName = sectionName,
            Present = present,
            TotalPresent = present,
            TotalAbsent = absent,
            PresentPercent = Pct(present, total),
            AbsentPercent = Pct(absent, total)
        });
    }

    private static MonthlyAttendancePersonRowDto BuildPersonRow(
        Guid id, string name, string? registerNo, string? roll,
        int year, int month, IReadOnlyList<DayColumnDto> dayColumns,
        IReadOnlyDictionary<DateTime, string> statusByDate,
        HashSet<DayOfWeek> weekends, HashSet<DateTime> holidays)
    {
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var cells = new List<MonthlyDayCellDto>(daysInMonth);
        var p = 0; var a = 0; var l = 0; var hd = 0; var w = 0;
        var working = 0;

        foreach (var col in dayColumns)
        {
            var date = new DateTime(year, month, col.Day);
            string? code;
            if (col.IsWeekend)
            {
                code = "W";
                w++;
            }
            else if (col.IsHoliday)
            {
                code = "H";
            }
            else
            {
                working++;
                if (statusByDate.TryGetValue(date, out var status))
                {
                    code = ToCode(status);
                    switch (code)
                    {
                        case "P": p++; break;
                        case "A": a++; break;
                        case "L": l++; break;
                        case "HD": hd++; break;
                    }
                }
                else code = null;
            }

            cells.Add(new MonthlyDayCellDto { Day = col.Day, Key = col.Key, Code = code });
        }

        var attended = p + l + hd;
        return new MonthlyAttendancePersonRowDto
        {
            Id = id,
            Name = name,
            RegisterNo = registerNo,
            Roll = roll,
            Days = cells,
            Percentage = Pct(attended, working),
            WeekendCount = w,
            PresentCount = p,
            AbsentCount = a,
            LateCount = l,
            HalfDayCount = hd
        };
    }

    private static List<DayColumnDto> BuildDayColumns(
        int year, int month, HashSet<DayOfWeek> weekends, HashSet<DateTime> holidays)
    {
        var days = DateTime.DaysInMonth(year, month);
        var cols = new List<DayColumnDto>(days);
        for (var d = 1; d <= days; d++)
        {
            var date = new DateTime(year, month, d);
            cols.Add(new DayColumnDto
            {
                Day = d,
                Key = d.ToString("00"),
                DayName = date.ToString("ddd", CultureInfo.InvariantCulture),
                IsWeekend = weekends.Contains(date.DayOfWeek),
                IsHoliday = holidays.Contains(date.Date)
            });
        }
        return cols;
    }

    private async Task<HashSet<DayOfWeek>> ResolveWeekendsAsync(CancellationToken ct)
    {
        var settings = await uow.SchoolSettings.GetAsync(ct);
        var raw = settings?.WeekendDays;
        if (string.IsNullOrWhiteSpace(raw)) raw = "5,6";
        var set = new HashSet<DayOfWeek>();
        foreach (var part in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (int.TryParse(part, out var n) && n is >= 0 and <= 6)
                set.Add((DayOfWeek)n);
        }
        if (set.Count == 0)
        {
            set.Add(DayOfWeek.Friday);
            set.Add(DayOfWeek.Saturday);
        }
        return set;
    }

    private async Task<(string ClassName, string? SectionName)> ResolveClassSectionAsync(
        Guid classId, Guid sectionId, CancellationToken ct)
    {
        var cls = await uow.ClassControls.GetByIdWithSectionsAsync(classId, ct);
        var className = cls?.Name ?? classId.ToString();
        var section = cls?.ClassSections?.Select(cs => cs.Section).FirstOrDefault(s => s?.Id == sectionId)
                      ?? cls?.Sections?.FirstOrDefault(s => s.Id == sectionId);
        if (section is null)
        {
            var sections = await uow.SectionControls.GetAllAsync(ct);
            section = sections.FirstOrDefault(s => s.Id == sectionId);
        }
        return (className, section?.Name);
    }

    private static string ToCode(string status) => status.Trim().ToLowerInvariant() switch
    {
        "present" => "P",
        "absent" => "A",
        "late" => "L",
        "halfday" or "half day" or "half_day" => "HD",
        "holiday" => "H",
        "weekend" => "W",
        _ => status.Length <= 2 ? status.ToUpperInvariant() : status
    };

    private static string NormalizeStatus(string? type)
    {
        var t = (type ?? "Present").Trim();
        return t.ToLowerInvariant() switch
        {
            "present" or "p" => "Present",
            "absent" or "a" => "Absent",
            "late" or "l" => "Late",
            "halfday" or "half day" or "hd" or "half_day" => "HalfDay",
            _ => throw new AppException("attendanceType must be Present, Absent, Late, or HalfDay.", 400)
        };
    }

    private static (DateTime From, DateTime To) MonthRange(int year, int month)
        => (new DateTime(year, month, 1), new DateTime(year, month, DateTime.DaysInMonth(year, month)));

    private static void ValidateMonth(int year, int month)
    {
        if (year is < 2000 or > 2100) throw new AppException("Invalid year.", 400);
        if (month is < 1 or > 12) throw new AppException("Invalid month.", 400);
    }

    private static decimal Pct(int part, int total)
        => total <= 0 ? 0 : Math.Round((decimal)part * 100m / total, 2);

    private static string StudentName(Student s)
        => string.IsNullOrWhiteSpace(s.LastName) ? s.FirstName.Trim() : $"{s.FirstName.Trim()} {s.LastName.Trim()}";

    private static string SubjectLabel(Subject s)
        => string.IsNullOrWhiteSpace(s.Code) ? s.Name : $"{s.Name} ({s.Code})";

    private async Task Ready(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tenant.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
        await provisioner.EnsureSettingsModuleAsync(tenant.SchemaName!, ct);
    }

    private HashSet<string> Roles() =>
        http.HttpContext?.User.FindAll("role").Concat(http.HttpContext.User.FindAll(ClaimTypes.Role))
            .Select(x => x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

    private void Manage()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin))
            throw new ForbiddenException("Only Super Admin or School Admin can access this report.");
    }

    private void ManageOrTeacher()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin)
            && !r.Contains(AppConstants.Roles.Teacher))
            throw new ForbiddenException("Only Super Admin, School Admin, or Teacher can access this report.");
    }
}
