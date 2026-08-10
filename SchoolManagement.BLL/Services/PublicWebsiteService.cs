using System.Text.Json;
using SchoolManagement.BLL.DTOs.Reports;
using SchoolManagement.BLL.DTOs.Website;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class PublicWebsiteService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IStorageService storage,
    IExaminationReportService examinationReports) : IPublicWebsiteService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public async Task<SiteContactDto> GetSettingsAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        var school = await uow.SchoolSettings.GetAsync(ct);
        var cms = await uow.Website.GetCmsSettingsAsync(ct);
        return new SiteContactDto
        {
            SchoolName = school?.SchoolName ?? "School",
            SchoolNameBn = cms?.SchoolNameBn,
            Phone = school?.Phone ?? "",
            Email = school?.Email ?? "",
            Address = school?.Address ?? "",
            LogoUrl = await Presign(school?.SystemLogoUrl ?? school?.TextLogoUrl, ct),
            FacebookUrl = cms?.FacebookUrl,
            YoutubeUrl = cms?.YoutubeUrl,
            PortalUrl = cms?.PortalUrl ?? "/portal",
            CopyrightText = cms?.CopyrightText
        };
    }

    public async Task<IReadOnlyList<MenuItemDto>> GetMenuAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        var items = await uow.Website.GetMenuAsync(ct);
        if (items.Count == 0) return DefaultMenu();

        var roots = items.Where(x => x.ParentId is null).OrderBy(x => x.SortOrder).ToList();
        return roots.Select(r => MapMenu(r, items)).ToList();
    }

    public async Task<IReadOnlyList<FooterColumnDto>> GetFooterAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        var links = await uow.Website.GetFooterLinksAsync(ct);
        if (links.Count == 0) return DefaultFooter();

        return links
            .GroupBy(x => x.ColumnKey)
            .Select(g =>
            {
                var first = g.First();
                return new FooterColumnDto
                {
                    Title = first.ColumnTitle,
                    TitleBn = first.ColumnTitleBn,
                    Links = g.OrderBy(x => x.SortOrder).Select(x => new FooterLinkDto
                    {
                        Label = x.Label,
                        LabelBn = x.LabelBn,
                        Path = x.Path,
                        External = x.IsExternal
                    }).ToList()
                };
            }).ToList();
    }

    public async Task<VisitorStatsDto> GetVisitorsAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        var (today, last7, total) = await uow.Website.GetVisitorStatsAsync(ct);
        return new VisitorStatsDto
        {
            ViewsToday = today,
            ViewsLast7Days = last7,
            TotalViews = total,
            ServerTime = DateTime.UtcNow
        };
    }

    public async Task<VisitorStatsDto> HitVisitorsAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        var (today, last7, total) = await uow.Website.HitVisitorAsync(ct);
        return new VisitorStatsDto
        {
            ViewsToday = today,
            ViewsLast7Days = last7,
            TotalViews = total,
            ServerTime = DateTime.UtcNow
        };
    }

    public async Task<HomePageDto> GetHomeAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        var cms = await uow.Website.GetCmsSettingsAsync(ct);
        var sliders = await uow.Website.GetSlidersAsync(ct);
        var notices = await uow.Website.GetNoticesAsync(10, null, ct);
        var gallery = await uow.Website.GetGalleryAsync(null, 4, ct);
        var links = await uow.Website.GetImportantLinksAsync(ct);
        var president = await uow.Website.GetSpeechAsync(WebsiteSpeechRoles.President, ct);
        var headmaster = await uow.Website.GetSpeechAsync(WebsiteSpeechRoles.Headmaster, ct);
        var visitors = await GetVisitorsAsync(ct);

        return new HomePageDto
        {
            Slider = (await Task.WhenAll(sliders.Select(async s => new SliderItemDto
            {
                ImageUrl = await Presign(s.ImageUrl, ct) ?? s.ImageUrl,
                Caption = s.Caption,
                ButtonText = s.ButtonText,
                ButtonUrl = s.ButtonUrl
            }))).ToList(),
            PresidentPreview = president is null ? null : new SpeechPreviewDto
            {
                Name = president.Name,
                NameBn = president.NameBn,
                Designation = president.Designation,
                DesignationBn = president.DesignationBn,
                PhotoUrl = await Presign(president.PhotoUrl, ct),
                MessageHtml = Excerpt(president.MessageHtml),
                ReadMorePath = "/about/speeches/president"
            },
            HeadmasterPreview = headmaster is null ? null : new SpeechPreviewDto
            {
                Name = headmaster.Name,
                NameBn = headmaster.NameBn,
                Designation = headmaster.Designation,
                DesignationBn = headmaster.DesignationBn,
                PhotoUrl = await Presign(headmaster.PhotoUrl, ct),
                MessageHtml = Excerpt(headmaster.MessageHtml),
                ReadMorePath = "/about/speeches/headmaster"
            },
            Notices = await MapNotices(notices, ct),
            GalleryPreview = await MapGallery(gallery, ct),
            ImportantLinks = links.Select(l => new ImportantLinkDto
            {
                Id = l.Id,
                Label = l.Label,
                Url = l.Url,
                SortOrder = l.SortOrder
            }).ToList(),
            VisitorStats = visitors,
            FacebookPageUrl = cms?.FacebookPageUrl ?? cms?.FacebookUrl,
            OnlineAdmissionEnabled = cms?.OnlineAdmissionEnabled ?? true
        };
    }

    public async Task<HistoryPageDto> GetHistoryAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        var school = await uow.SchoolSettings.GetAsync(ct);
        var cms = await uow.Website.GetCmsSettingsAsync(ct);

        var sections = DeserializeList<HistorySectionDto>(cms?.HistorySectionsJson);
        var founding = DeserializeList<FoundingCommitteeRowDto>(cms?.FoundingCommitteeJson);

        return new HistoryPageDto
        {
            Title = cms?.HistoryTitle ?? "History",
            TitleBn = cms?.HistoryTitleBn,
            Profile = new HistoryProfileDto
            {
                Eiin = cms?.Eiin,
                EstablishedYear = cms?.EstablishedYear,
                SchoolType = cms?.SchoolType,
                ClassesOffered = cms?.ClassesOffered,
                TotalStudentsLabel = cms?.TotalStudentsLabel,
                Website = school?.Website,
                Address = school?.Address,
                ImageUrl = await Presign(cms?.HistoryImageUrl, ct)
            },
            Sections = sections,
            FoundingCommittee = founding.Count == 0 ? null : founding
        };
    }

    public async Task<PersonSpeechDto> GetSpeechAsync(string role, CancellationToken ct = default)
    {
        await Ready(ct);
        var normalized = NormalizeSpeechRole(role);
        var speech = await uow.Website.GetSpeechAsync(normalized, ct)
            ?? throw new NotFoundException($"{normalized} speech not found.");
        return new PersonSpeechDto
        {
            Title = speech.Title,
            TitleBn = speech.TitleBn,
            Name = speech.Name,
            NameBn = speech.NameBn,
            Designation = speech.Designation,
            DesignationBn = speech.DesignationBn,
            PhotoUrl = await Presign(speech.PhotoUrl, ct),
            MessageHtml = speech.MessageHtml,
            Phone = speech.Phone,
            Email = speech.Email,
            FacebookUrl = speech.FacebookUrl
        };
    }

    public async Task<IReadOnlyList<TenurePersonDto>> GetPresidentsAsync(string? search, CancellationToken ct = default)
    {
        await Ready(ct);
        var rows = await uow.Website.GetTenureAsync(WebsiteSpeechRoles.President, search, ct);
        return rows.Select((r, i) => new TenurePersonDto
        {
            Id = r.Id,
            Sl = i + 1,
            Name = r.Name,
            Designation = r.Designation,
            JoinedOn = r.JoinedOn,
            LeftOn = r.LeftOn
        }).ToList();
    }

    public async Task<IReadOnlyList<TenurePersonDto>> GetHeadmastersAsync(string? search, CancellationToken ct = default)
    {
        await Ready(ct);
        var rows = await uow.Website.GetTenureAsync(WebsiteSpeechRoles.Headmaster, search, ct);
        return rows.Select((r, i) => new TenurePersonDto
        {
            Id = r.Id,
            Sl = i + 1,
            Name = r.Name,
            Designation = r.Designation,
            JoinedOn = r.JoinedOn,
            LeftOn = r.LeftOn
        }).ToList();
    }

    public async Task<CommitteeResponseDto> GetCommitteeAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        var members = await uow.Website.GetCommitteeAsync(ct);
        var order = new[]
        {
            WebsiteCommitteeCategories.President,
            WebsiteCommitteeCategories.GuardianRepresentative,
            WebsiteCommitteeCategories.TeacherRepresentative,
            WebsiteCommitteeCategories.MemberSecretary
        };

        var categories = new List<CommitteeCategoryDto>();
        foreach (var key in order)
        {
            var group = members.Where(m => m.Category == key).OrderBy(m => m.SortOrder).ToList();
            if (group.Count == 0 && members.All(m => order.Contains(m.Category))) continue;
            if (group.Count == 0) continue;

            var titleBn = group.First().CategoryBn;
            categories.Add(new CommitteeCategoryDto
            {
                Key = key,
                Title = key,
                TitleBn = titleBn,
                Members = (await Task.WhenAll(group.Select(async (m, i) => new CommitteeMemberDto
                {
                    Id = m.Id,
                    Sl = i + 1,
                    Name = m.Name,
                    Designation = m.Designation,
                    Category = m.Category,
                    CategoryBn = m.CategoryBn,
                    PhotoUrl = await Presign(m.PhotoUrl, ct),
                    MobileNo = m.MobileNo
                }))).ToList()
            });
        }

        // any other categories
        foreach (var g in members.Where(m => !order.Contains(m.Category)).GroupBy(m => m.Category))
        {
            var list = g.OrderBy(m => m.SortOrder).ToList();
            categories.Add(new CommitteeCategoryDto
            {
                Key = g.Key,
                Title = g.Key,
                TitleBn = list.First().CategoryBn,
                Members = (await Task.WhenAll(list.Select(async (m, i) => new CommitteeMemberDto
                {
                    Id = m.Id,
                    Sl = i + 1,
                    Name = m.Name,
                    Designation = m.Designation,
                    Category = m.Category,
                    CategoryBn = m.CategoryBn,
                    PhotoUrl = await Presign(m.PhotoUrl, ct),
                    MobileNo = m.MobileNo
                }))).ToList()
            });
        }

        return new CommitteeResponseDto { Categories = categories };
    }

    public Task<IReadOnlyList<PublicStaffMemberDto>> GetTeachersAsync(string? search, CancellationToken ct = default)
        => GetStaffByRoleAsync(EmployeeRoles.Teacher, search, ct);

    public Task<IReadOnlyList<PublicStaffMemberDto>> GetOfficeStaffAsync(string? search, CancellationToken ct = default)
        => GetStaffByRoleAsync(EmployeeRoles.Staff, search, ct);

    public async Task<IReadOnlyList<NoticeItemDto>> GetNoticesAsync(int? limit, string? search, CancellationToken ct = default)
    {
        await Ready(ct);
        var items = await uow.Website.GetNoticesAsync(limit, search, ct);
        return await MapNotices(items, ct);
    }

    public async Task<NoticeItemDto> GetNoticeAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        var n = await uow.Website.GetNoticeAsync(id, ct) ?? throw new NotFoundException("Notice not found.");
        return new NoticeItemDto
        {
            Id = n.Id,
            Sl = 1,
            PublishedOn = n.PublishedOn,
            Subject = n.Subject,
            ViewUrl = $"/notices/{n.Id}",
            FileUrl = await Presign(n.FileUrl, ct),
            BodyHtml = n.BodyHtml
        };
    }

    public async Task<IReadOnlyList<GalleryCategoryDto>> GetGalleryCategoriesAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        var cats = await uow.Website.GetGalleryCategoriesAsync(ct);
        return cats.Select(c => new GalleryCategoryDto { Id = c.Id, Name = c.Name }).ToList();
    }

    public async Task<IReadOnlyList<GalleryItemDto>> GetGalleryAsync(Guid? categoryId, int? limit, CancellationToken ct = default)
    {
        await Ready(ct);
        var items = await uow.Website.GetGalleryAsync(categoryId, limit, ct);
        return await MapGallery(items, ct);
    }

    public async Task<GalleryItemDto> GetGalleryItemAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        var g = await uow.Website.GetGalleryItemAsync(id, ct) ?? throw new NotFoundException("Gallery item not found.");
        var extras = DeserializeList<string>(g.ExtraImagesJson);
        var images = new List<string>();
        var main = await Presign(g.ImageUrl, ct);
        if (!string.IsNullOrEmpty(main)) images.Add(main);
        foreach (var e in extras)
        {
            var u = await Presign(e, ct);
            if (!string.IsNullOrEmpty(u)) images.Add(u);
        }

        return new GalleryItemDto
        {
            Id = g.Id,
            Title = g.Title,
            ThumbUrl = await Presign(g.ThumbUrl, ct) ?? g.ThumbUrl,
            ImageUrl = main ?? g.ImageUrl,
            CategoryId = g.CategoryId,
            CategoryName = g.Category?.Name,
            Date = g.EventDate,
            Description = g.Description,
            Images = images
        };
    }

    public async Task<IReadOnlyList<DocumentItemDto>> GetDocumentsAsync(string? category, string? search, CancellationToken ct = default)
    {
        await Ready(ct);
        var docs = await uow.Website.GetDocumentsAsync(category, search, ct);
        var result = new List<DocumentItemDto>();
        foreach (var d in docs)
        {
            result.Add(new DocumentItemDto
            {
                Id = d.Id,
                Title = d.Title,
                TitleBn = d.TitleBn,
                Category = d.Category,
                FileUrl = await Presign(d.FileUrl, ct) ?? d.FileUrl,
                PublishedOn = d.PublishedOn
            });
        }
        return result;
    }

    public async Task<DocumentItemDto> GetDocumentAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        var d = await uow.Website.GetDocumentAsync(id, ct) ?? throw new NotFoundException("Document not found.");
        return new DocumentItemDto
        {
            Id = d.Id,
            Title = d.Title,
            TitleBn = d.TitleBn,
            Category = d.Category,
            FileUrl = await Presign(d.FileUrl, ct) ?? d.FileUrl,
            PublishedOn = d.PublishedOn
        };
    }

    public async Task<AcademicPageDto> GetAcademicPageAsync(string slug, CancellationToken ct = default)
    {
        await Ready(ct);
        var key = slug.Trim().ToLowerInvariant();
        if (!WebsiteDocumentCategories.AcademicPageSlugs.Contains(key)
            && !WebsiteDocumentCategories.RoutineSlugs.Contains(key))
        {
            // allow any content page slug
        }

        var page = await uow.Website.GetContentPageAsync(key, ct);
        var docs = await uow.Website.GetDocumentsAsync(key, null, ct);
        var docDtos = new List<DocumentItemDto>();
        foreach (var d in docs)
        {
            docDtos.Add(new DocumentItemDto
            {
                Id = d.Id,
                Title = d.Title,
                TitleBn = d.TitleBn,
                Category = d.Category,
                FileUrl = await Presign(d.FileUrl, ct) ?? d.FileUrl,
                PublishedOn = d.PublishedOn
            });
        }

        if (page is null && docDtos.Count == 0)
            throw new NotFoundException($"Academic page '{slug}' not found.");

        var primaryFile = page?.FileUrl;
        if (string.IsNullOrWhiteSpace(primaryFile) && docDtos.Count > 0)
            primaryFile = docDtos[0].FileUrl;
        else if (!string.IsNullOrWhiteSpace(primaryFile))
            primaryFile = await Presign(primaryFile, ct);

        return new AcademicPageDto
        {
            Slug = key,
            Title = page?.Title ?? TitleFromSlug(key),
            TitleBn = page?.TitleBn ?? TitleBnFromSlug(key),
            BodyHtml = page?.BodyHtml,
            FileUrl = primaryFile,
            Documents = docDtos
        };
    }

    public Task<AcademicPageDto> GetAcademicRoutineAsync(string type, CancellationToken ct = default)
    {
        var key = type.Trim().ToLowerInvariant();
        if (!WebsiteDocumentCategories.RoutineSlugs.Contains(key))
            throw new AppException(
                "Routine type must be one of: class-routine, school-exam-routine, ssc-exam-routine, ssc-vocational-exam-routine.",
                400);
        return GetAcademicPageAsync(key, ct);
    }

    public async Task<IReadOnlyList<HandnoteItemDto>> GetHandnotesAsync(string? className, string? search, CancellationToken ct = default)
    {
        await Ready(ct);
        var items = await uow.Website.GetHandnotesAsync(className, search, ct);
        var list = new List<HandnoteItemDto>();
        var i = 0;
        foreach (var h in items)
        {
            list.Add(new HandnoteItemDto
            {
                Id = h.Id,
                Sl = ++i,
                PublishedOn = h.PublishedOn,
                ClassName = h.ClassName,
                Title = h.Title,
                TeacherName = h.TeacherName,
                DownloadUrl = await Presign(h.FileUrl, ct) ?? h.FileUrl
            });
        }
        return list;
    }

    public async Task<IReadOnlyList<OnlineClassGroupDto>> GetOnlineClassesAsync(string? className, CancellationToken ct = default)
    {
        await Ready(ct);
        var videos = await uow.Website.GetOnlineClassVideosAsync(className, ct);
        return videos
            .GroupBy(v => v.ClassName)
            .OrderBy(g => g.Key)
            .Select(g => new OnlineClassGroupDto
            {
                ClassName = g.Key,
                Videos = g.Select(v =>
                {
                    var videoId = v.YoutubeVideoId ?? ExtractYoutubeId(v.YoutubeUrl);
                    return new OnlineClassVideoDto
                    {
                        Id = v.Id,
                        ClassName = v.ClassName,
                        Title = v.Title,
                        Subject = v.Subject,
                        TeacherName = v.TeacherName,
                        YoutubeUrl = v.YoutubeUrl,
                        YoutubeVideoId = videoId,
                        EmbedUrl = string.IsNullOrEmpty(videoId) ? null : $"https://www.youtube.com/embed/{videoId}",
                        ClassDate = v.ClassDate
                    };
                }).ToList()
            }).ToList();
    }

    public async Task<ResultAnalyticsPageDto> GetResultAnalyticsAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        var rows = await uow.Website.GetResultAnalyticsAsync(ct);

        ResultExamAnalyticsDto Map(string type, string title)
        {
            var subset = rows.Where(r => r.ExamType == type).OrderByDescending(r => r.Year).ToList();
            return new ResultExamAnalyticsDto
            {
                ExamType = type,
                Title = title,
                PassFailStats = subset.Select(r => new ResultPassFailRowDto
                {
                    Year = r.Year,
                    Appeared = r.Appeared,
                    Passed = r.Passed,
                    NotPassed = r.NotPassed,
                    PassPercent = r.PassPercent,
                    Gpa5 = r.Gpa5,
                    Gpa5Percent = r.Gpa5Percent
                }).ToList(),
                GpaDistribution = subset.Select(r => new ResultGpaDistributionRowDto
                {
                    Year = r.Year,
                    Gpa5 = r.Gpa5,
                    Gpa4x = r.Gpa4x,
                    Gpa3x = r.Gpa3x,
                    Gpa2x = r.Gpa2x,
                    Gpa1x = r.Gpa1x
                }).ToList()
            };
        }

        return new ResultAnalyticsPageDto
        {
            SscExam = Map(WebsiteExamTypes.Ssc, "SSC Exam"),
            SscVocational = Map(WebsiteExamTypes.SscVocational, "SSC Vocational Exam")
        };
    }

    public async Task<IReadOnlyList<PublishedResultItemDto>> GetPublishedResultsAsync(string? examType, CancellationToken ct = default)
    {
        await Ready(ct);
        string? type = null;
        if (!string.IsNullOrWhiteSpace(examType))
        {
            type = examType.Equals("vocational", StringComparison.OrdinalIgnoreCase)
                   || examType.Equals("sscvocational", StringComparison.OrdinalIgnoreCase)
                ? WebsiteExamTypes.SscVocational
                : examType.Equals("ssc", StringComparison.OrdinalIgnoreCase)
                    ? WebsiteExamTypes.Ssc
                    : examType.Trim();
        }

        var items = await uow.Website.GetPublishedResultsAsync(type, ct);
        var list = new List<PublishedResultItemDto>();
        foreach (var r in items)
        {
            list.Add(new PublishedResultItemDto
            {
                Id = r.Id,
                Title = r.Title,
                TitleBn = r.TitleBn,
                ExamType = r.ExamType,
                Year = r.Year,
                DetailUrl = r.DetailUrl,
                FileUrl = await Presign(r.FileUrl, ct)
            });
        }
        return list;
    }

    public async Task<IReadOnlyList<OnlineExamOptionDto>> GetOnlineResultExamsAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        var exams = await uow.Exams.GetResultPublishedAsync(ct);
        return exams.Select(e => new OnlineExamOptionDto
        {
            Id = e.Id,
            Name = e.Name,
            ExamType = e.ExamType,
            TermName = e.ExamTerm?.Name
        }).ToList();
    }

    public async Task<ReportCardDto> SearchOnlineResultAsync(string registerNo, Guid examId, CancellationToken ct = default)
    {
        await Ready(ct);
        return await examinationReports.GetOnlineStudentResultAsync(registerNo, examId, ct);
    }

    public async Task<StudentStatisticsDto> GetStudentStatisticsAsync(int? academicYear, CancellationToken ct = default)
    {
        await Ready(ct);
        var (students, _) = await uow.Students.SearchAsync(new StudentSearchFilter
        {
            IsActive = true,
            AcademicYear = academicYear,
            Page = 1,
            PageSize = 50_000,
            SortBy = "class"
        }, ct);

        var rows = students
            .Where(s => s.ClassId.HasValue)
            .GroupBy(s => new
            {
                ClassId = s.ClassId,
                SectionId = s.SectionId,
                ClassName = s.Class?.Name ?? "—",
                SectionName = s.Section?.Name ?? "—"
            })
            .Select(g =>
            {
                var male = g.Count(s => IsMale(s.Gender));
                var female = g.Count(s => IsFemale(s.Gender));
                return new StudentStatRowDto
                {
                    ClassId = g.Key.ClassId,
                    SectionId = g.Key.SectionId,
                    ClassName = g.Key.ClassName,
                    SectionName = g.Key.SectionName,
                    Male = male,
                    Female = female,
                    Total = g.Count()
                };
            })
            .OrderBy(r => r.ClassName).ThenBy(r => r.SectionName)
            .ToList();

        return new StudentStatisticsDto
        {
            Rows = rows,
            MaleTotal = rows.Sum(r => r.Male),
            FemaleTotal = rows.Sum(r => r.Female),
            GrandTotal = rows.Sum(r => r.Total)
        };
    }

    public async Task<PublicStudentListDto> GetPublicStudentsAsync(
        Guid? classId, Guid? sectionId, string? className, string? sectionName,
        string? search, int page, int pageSize, int? academicYear, CancellationToken ct = default)
    {
        await Ready(ct);
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 200 ? 60 : pageSize;

        if (!classId.HasValue && !string.IsNullOrWhiteSpace(className))
        {
            var classes = await uow.ClassControls.GetAllWithSectionsAsync(ct);
            var match = classes.FirstOrDefault(c =>
                c.Name.Equals(className.Trim(), StringComparison.OrdinalIgnoreCase)
                || NormalizeClassToken(c.Name) == NormalizeClassToken(className));
            classId = match?.Id;
        }

        if (!sectionId.HasValue && !string.IsNullOrWhiteSpace(sectionName))
        {
            var sections = await uow.SectionControls.GetAllAsync(ct);
            var match = sections.FirstOrDefault(s =>
                s.Name.Equals(sectionName.Trim(), StringComparison.OrdinalIgnoreCase));
            sectionId = match?.Id;
        }

        if (!classId.HasValue)
            throw new AppException("classId or className is required.", 400);

        var (items, total) = await uow.Students.SearchAsync(new StudentSearchFilter
        {
            ClassId = classId,
            SectionId = sectionId,
            Search = search,
            AcademicYear = academicYear,
            IsActive = true,
            Page = page,
            PageSize = pageSize,
            SortBy = "roll",
            SortDir = "asc"
        }, ct);

        var rows = new List<PublicStudentRowDto>();
        var i = (page - 1) * pageSize;
        foreach (var s in items)
        {
            var father = s.Guardians.FirstOrDefault(g =>
                g.Relation.Contains("Father", StringComparison.OrdinalIgnoreCase))?.Name
                ?? s.Guardians.FirstOrDefault()?.FatherName;
            var mother = s.Guardians.FirstOrDefault(g =>
                g.Relation.Contains("Mother", StringComparison.OrdinalIgnoreCase))?.Name
                ?? s.Guardians.FirstOrDefault()?.MotherName;

            rows.Add(new PublicStudentRowDto
            {
                Id = s.Id,
                Sl = ++i,
                PhotoUrl = await Presign(s.ProfilePictureUrl, ct),
                Name = string.IsNullOrWhiteSpace(s.LastName) ? s.FirstName.Trim() : $"{s.FirstName.Trim()} {s.LastName.Trim()}",
                ClassName = s.Class?.Name,
                SectionName = s.Section?.Name,
                RegisterNo = s.RegisterNo,
                Roll = s.Roll,
                FatherName = father,
                MotherName = mother
            });
        }

        return new PublicStudentListDto
        {
            Students = rows,
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            ClassName = items.FirstOrDefault()?.Class?.Name ?? className,
            SectionName = items.FirstOrDefault()?.Section?.Name ?? sectionName
        };
    }

    public async Task<ContactPageDto> GetContactAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        var school = await uow.SchoolSettings.GetAsync(ct);
        var cms = await uow.Website.GetCmsSettingsAsync(ct);
        return new ContactPageDto
        {
            PageTitle = cms?.ContactPageTitle ?? "Contact Us",
            BoxTitle = cms?.ContactBoxTitle,
            BoxDescription = cms?.ContactBoxDescription,
            Address = school?.Address ?? "",
            Phone = school?.Phone ?? "",
            Email = school?.Email ?? "",
            MapIframeHtml = cms?.ContactMapIframeHtml,
            SubmitButtonText = cms?.ContactSubmitButtonText ?? "Send"
        };
    }

    public async Task<ContactMessageResultDto> SubmitContactAsync(ContactMessagePayloadDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Message))
            throw new AppException("Name, email, and message are required.", 400);

        var entity = new WebsiteContactMessage
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Email = dto.Email.Trim(),
            Phone = dto.Phone?.Trim(),
            Subject = dto.Subject?.Trim(),
            Message = dto.Message.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        await uow.Website.AddContactMessageAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        return new ContactMessageResultDto { Id = entity.Id };
    }

    private async Task<IReadOnlyList<PublicStaffMemberDto>> GetStaffByRoleAsync(string role, string? search, CancellationToken ct)
    {
        await Ready(ct);
        var (items, _) = await uow.Employees.SearchAsync(new EmployeeSearchFilter
        {
            Role = role,
            Search = search,
            IsActive = true,
            Page = 1,
            PageSize = 500,
            SortBy = "name",
            SortDir = "asc"
        }, ct);

        var result = new List<PublicStaffMemberDto>();
        var i = 0;
        foreach (var e in items)
        {
            result.Add(new PublicStaffMemberDto
            {
                Id = e.Id,
                Sl = ++i,
                Name = e.Name,
                IndexNo = e.StaffId,
                Designation = e.Designation?.Name ?? e.Role,
                Subject = e.Department?.Name,
                PhotoUrl = await Presign(e.ProfilePictureUrl, ct),
                MobileNos = string.IsNullOrWhiteSpace(e.MobileNo) ? [] : [e.MobileNo],
                Email = e.Email,
                Qualifications = ParseQualifications(e.Qualification),
                FirstJoiningDate = e.JoiningDate,
                MpoDate = null,
                PresentJoiningDate = e.JoiningDate,
                DateOfBirth = e.DateOfBirth
            });
        }
        return result;
    }

    private static List<StaffQualificationDto> ParseQualifications(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return [];
        return raw.Split(['\n', ';', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => new StaffQualificationDto { Degree = line })
            .ToList();
    }

    private async Task<List<NoticeItemDto>> MapNotices(IReadOnlyList<WebsiteNotice> notices, CancellationToken ct)
    {
        var list = new List<NoticeItemDto>();
        var i = 0;
        foreach (var n in notices)
        {
            list.Add(new NoticeItemDto
            {
                Id = n.Id,
                Sl = ++i,
                PublishedOn = n.PublishedOn,
                Subject = n.Subject,
                ViewUrl = $"/notices/{n.Id}",
                FileUrl = await Presign(n.FileUrl, ct)
            });
        }
        return list;
    }

    private async Task<List<GalleryItemDto>> MapGallery(IReadOnlyList<WebsiteGalleryItem> items, CancellationToken ct)
    {
        var list = new List<GalleryItemDto>();
        foreach (var g in items)
        {
            list.Add(new GalleryItemDto
            {
                Id = g.Id,
                Title = g.Title,
                ThumbUrl = await Presign(g.ThumbUrl, ct) ?? g.ThumbUrl,
                ImageUrl = await Presign(g.ImageUrl, ct) ?? g.ImageUrl,
                CategoryId = g.CategoryId,
                CategoryName = g.Category?.Name,
                Date = g.EventDate
            });
        }
        return list;
    }

    private static MenuItemDto MapMenu(WebsiteMenuItem item, IReadOnlyList<WebsiteMenuItem> all)
        => new()
        {
            Id = item.Id,
            Title = item.Title,
            TitleBn = item.TitleBn,
            Path = item.Path,
            OpenInNewTab = item.OpenInNewTab,
            Children = all.Where(c => c.ParentId == item.Id).OrderBy(c => c.SortOrder)
                .Select(c => MapMenu(c, all)).ToList()
        };

    private static string NormalizeSpeechRole(string role)
    {
        var r = role.Trim();
        if (r.Equals("president", StringComparison.OrdinalIgnoreCase)) return WebsiteSpeechRoles.President;
        if (r.Equals("headmaster", StringComparison.OrdinalIgnoreCase)) return WebsiteSpeechRoles.Headmaster;
        throw new AppException("Speech role must be president or headmaster.", 400);
    }

    private static string Excerpt(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return "";
        var plain = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", " ");
        plain = System.Text.RegularExpressions.Regex.Replace(plain, @"\s+", " ").Trim();
        return plain.Length <= 280 ? $"<p>{plain}</p>" : $"<p>{plain[..280]}…</p>";
    }

    private static List<T> DeserializeList<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "[]") return [];
        try { return JsonSerializer.Deserialize<List<T>>(json, JsonOpts) ?? []; }
        catch { return []; }
    }

    private static IReadOnlyList<MenuItemDto> DefaultMenu() =>
    [
        new() { Id = Guid.Empty, Title = "Home", TitleBn = "হোম", Path = "/" },
        new() { Id = Guid.Empty, Title = "About", TitleBn = "আমাদের কথা", Path = "/about", Children =
        [
            new() { Title = "History", Path = "/history" },
            new() { Title = "President Speech", Path = "/president-speech" },
            new() { Title = "Headmaster Speech", Path = "/headmaster-speech" },
            new() { Title = "Managing Committee", Path = "/administration" },
            new() { Title = "Teachers", Path = "/teachers" },
            new() { Title = "Office Staff", Path = "/office-staff" }
        ]},
        new() { Id = Guid.Empty, Title = "Gallery", TitleBn = "ফটো গ্যালারী", Path = "/gallery" },
        new() { Id = Guid.Empty, Title = "Contact", TitleBn = "যোগাযোগ", Path = "/contact" }
    ];

    private static IReadOnlyList<FooterColumnDto> DefaultFooter() =>
    [
        new()
        {
            Title = "Institution Info",
            TitleBn = "প্রতিষ্ঠানের তথ্য",
            Links =
            [
                new() { Label = "History", Path = "/history" },
                new() { Label = "Managing Committee", Path = "/administration" },
                new() { Label = "Teachers", Path = "/teachers" },
                new() { Label = "Office Staff", Path = "/office-staff" }
            ]
        },
        new()
        {
            Title = "Other Info",
            TitleBn = "অন্যান্য তথ্য",
            Links =
            [
                new() { Label = "Photo Gallery", Path = "/gallery" },
                new() { Label = "Notice Board", Path = "/notices" },
                new() { Label = "Documents", Path = "/documents" }
            ]
        }
    ];

    private static bool IsMale(string? gender)
        => !string.IsNullOrWhiteSpace(gender) &&
           (gender.Equals("Male", StringComparison.OrdinalIgnoreCase)
            || gender.Equals("M", StringComparison.OrdinalIgnoreCase)
            || gender.Contains("পুরুষ", StringComparison.OrdinalIgnoreCase));

    private static bool IsFemale(string? gender)
        => !string.IsNullOrWhiteSpace(gender) &&
           (gender.Equals("Female", StringComparison.OrdinalIgnoreCase)
            || gender.Equals("F", StringComparison.OrdinalIgnoreCase)
            || gender.Contains("মহিলা", StringComparison.OrdinalIgnoreCase)
            || gender.Contains("নারী", StringComparison.OrdinalIgnoreCase));

    private static string NormalizeClassToken(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "";
        var n = name.Trim().ToLowerInvariant().Replace(" ", "").Replace("-", "");
        return n switch
        {
            "6" or "vi" or "class6" or "classvi" or "six" => "six",
            "7" or "vii" or "class7" or "classvii" or "seven" => "seven",
            "8" or "viii" or "class8" or "classviii" or "eight" => "eight",
            "9" or "ix" or "class9" or "classix" or "nine" => "nine",
            "10" or "x" or "class10" or "classx" or "ten" => "ten",
            _ => n
        };
    }

    private static string TitleFromSlug(string slug) => slug switch
    {
        WebsiteDocumentCategories.ClassRoutine => "Class Routine",
        WebsiteDocumentCategories.SchoolExamRoutine => "School Exam Routine",
        WebsiteDocumentCategories.SscExamRoutine => "SSC Exam Routine",
        WebsiteDocumentCategories.SscVocationalExamRoutine => "SSC Vocational Exam Routine",
        WebsiteDocumentCategories.Prospectus => "Prospectus",
        WebsiteDocumentCategories.AdmissionProcess => "Admission Process",
        WebsiteDocumentCategories.AdmissionTest => "Admission Test",
        WebsiteDocumentCategories.AdmissionForm => "Admission Form",
        WebsiteDocumentCategories.LessonPlanning => "Lesson Planning",
        WebsiteDocumentCategories.Library => "Library",
        WebsiteDocumentCategories.Laboratory => "Laboratory",
        _ => slug
    };

    private static string? TitleBnFromSlug(string slug) => slug switch
    {
        WebsiteDocumentCategories.ClassRoutine => "ক্লাস রুটিন",
        WebsiteDocumentCategories.SchoolExamRoutine => "স্কুল পরীক্ষার রুটিন",
        WebsiteDocumentCategories.SscExamRoutine => "এসএসসি পরীক্ষার রুটিন",
        WebsiteDocumentCategories.SscVocationalExamRoutine => "এসএসসি ভোকেশনাল পরীক্ষার রুটিন",
        WebsiteDocumentCategories.Prospectus => "প্রসপেক্টাস",
        WebsiteDocumentCategories.AdmissionProcess => "ভর্তি প্রক্রিয়া",
        WebsiteDocumentCategories.AdmissionTest => "ভর্তি পরীক্ষা",
        WebsiteDocumentCategories.AdmissionForm => "ভর্তি ফরম",
        WebsiteDocumentCategories.LessonPlanning => "পাঠ পরিকল্পনা",
        WebsiteDocumentCategories.Library => "লাইব্রেরি",
        WebsiteDocumentCategories.Laboratory => "ল্যাবরেটরি",
        _ => null
    };

    private static string? ExtractYoutubeId(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        try
        {
            var u = new Uri(url);
            if (u.Host.Contains("youtu.be", StringComparison.OrdinalIgnoreCase))
                return u.AbsolutePath.Trim('/');
            // ?v=
            var query = u.Query.TrimStart('?');
            foreach (var part in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var kv = part.Split('=', 2);
                if (kv.Length == 2 && kv[0].Equals("v", StringComparison.OrdinalIgnoreCase))
                    return Uri.UnescapeDataString(kv[1]);
            }
            var parts = u.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2 && parts[0] is "embed" or "shorts" or "v")
                return parts[1];
        }
        catch { /* ignore */ }
        return null;
    }

    private async Task<string?> Presign(string? objectKey, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(objectKey) || string.IsNullOrEmpty(tenant.TenantSlug))
            return objectKey;
        if (objectKey.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            objectKey.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return objectKey;
        try { return await storage.GetPresignedUrlAsync(tenant.TenantSlug!, objectKey, ct); }
        catch { return objectKey; }
    }

    private async Task Ready(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tenant.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
        await provisioner.EnsureWebsiteModuleAsync(tenant.SchemaName!, ct);
        await uow.Website.EnsureCmsSettingsRowAsync(ct);
    }
}
