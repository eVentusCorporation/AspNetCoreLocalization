using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace Localization.SqlLocalizer.DbStringLocalizer;

// >dotnet ef migrations add LocalizationMigration
public class DevelopmentSetup
{
    private readonly LocalizationModelContext _context;
    private readonly IOptions<SqlLocalizationOptions> _options;
    private readonly IOptions<RequestLocalizationOptions> _requestLocalizationOptions;

    public DevelopmentSetup(
        LocalizationModelContext context,
        IOptions<SqlLocalizationOptions> localizationOptions,
        IOptions<RequestLocalizationOptions> requestLocalizationOptions)
    {
        _options = localizationOptions;
        _context = context;
        _requestLocalizationOptions = requestLocalizationOptions;
    }

    public bool TryAddNewLocalizedItem(string key, string culture, string resourceKey, out string text)
    {
        text = _options.Value.AppendCultureToNewRecordText ? $"{key}.{culture}" : key;

        if (_requestLocalizationOptions.Value.SupportedCultures == null ||
            !_requestLocalizationOptions.Value.SupportedCultures.Contains(new CultureInfo(culture))) return false;
        var localizationRecord = new LocalizationRecord
        {
            LocalizationCulture = culture,
            Key = key,
            Text = text,
            ResourceKey = resourceKey
        };

        lock (_context)
        {
            if (_context.LocalizationRecords
                    .SingleOrDefault(r => r.Key == localizationRecord.Key
                                          && r.LocalizationCulture == localizationRecord.LocalizationCulture
                                          && r.ResourceKey == localizationRecord.ResourceKey) != null) return false;
            _context.LocalizationRecords.Add(localizationRecord);
            _context.SaveChanges();
            return true;
        }
    }
}