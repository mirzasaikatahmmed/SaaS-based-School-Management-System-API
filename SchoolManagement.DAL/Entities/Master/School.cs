using SchoolManagement.DAL.Entities.Master;

namespace SchoolManagement.DAL.Entities.Master;

/// <summary>
/// School maps to public.tenants. Prefer this type in the School module;
/// <see cref="Tenant"/> remains the EF entity type for MasterDbContext.
/// </summary>
public static class SchoolEntity
{
    public static School AsSchool(this Tenant tenant)
    {
        // Same row — cast via copy for DTO pipelines that expect School
        return new School
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Slug = tenant.Slug,
            Domain = tenant.Domain,
            SchemaName = tenant.SchemaName,
            IsActive = tenant.IsActive,
            SubscriptionPlan = tenant.SubscriptionPlan,
            MaxUsers = tenant.MaxUsers,
            Settings = tenant.Settings,
            CreatedAt = tenant.CreatedAt,
            UpdatedAt = tenant.UpdatedAt,
            Phone = tenant.Phone,
            Email = tenant.Email,
            Website = tenant.Website,
            Address = tenant.Address,
            City = tenant.City,
            State = tenant.State,
            Country = tenant.Country,
            Currency = tenant.Currency,
            CurrencySymbol = tenant.CurrencySymbol,
            Timezone = tenant.Timezone,
            Locale = tenant.Locale,
            LogoUrl = tenant.LogoUrl,
            EstablishedYear = tenant.EstablishedYear,
            SchoolType = tenant.SchoolType,
            SubscriptionExpiresAt = tenant.SubscriptionExpiresAt
        };
    }
}
