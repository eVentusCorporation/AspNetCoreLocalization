using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Localization.SqlLocalizer.DbStringLocalizer;

public class SqlStringLocalizerFactory : IStringExtendedLocalizerFactory
{
    private const string Global = "global";
    private static readonly ConcurrentDictionary<string, IStringLocalizer> ResourceLocalizations = new();
    private readonly LocalizationModelContext _context;
    private readonly DevelopmentSetup _developmentSetup;
    private readonly IOptions<SqlLocalizationOptions> _options;

    public SqlStringLocalizerFactory(
        LocalizationModelContext context,
        DevelopmentSetup developmentSetup,
        IOptions<SqlLocalizationOptions> localizationOptions)
    {
        _options = localizationOptions ?? throw new ArgumentNullException(nameof(localizationOptions));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _developmentSetup = developmentSetup;
    }

    public IStringLocalizer Create(Type resourceSource)
    {
        var returnOnlyKeyIfNotFound = _options.Value.ReturnOnlyKeyIfNotFound;
        var createNewRecordWhenLocalisedStringDoesNotExist =
            _options.Value.CreateNewRecordWhenLocalisedStringDoesNotExist;
        SqlStringLocalizer sqlStringLocalizer;

        if (_options.Value.UseOnlyPropertyNames)
        {
            if (ResourceLocalizations.Keys.Contains(Global)) return ResourceLocalizations[Global];

            sqlStringLocalizer = new SqlStringLocalizer(GetAllFromDatabaseForResource(Global), _developmentSetup,
                Global, returnOnlyKeyIfNotFound, createNewRecordWhenLocalisedStringDoesNotExist);
            return ResourceLocalizations.GetOrAdd(Global, sqlStringLocalizer);
        }

        if (_options.Value.UseTypeFullNames)
        {
            if (resourceSource.FullName != null &&
                ResourceLocalizations.TryGetValue(resourceSource.FullName, out var resourceLocalization))
                return resourceLocalization;

            sqlStringLocalizer = new SqlStringLocalizer(GetAllFromDatabaseForResource(resourceSource.FullName),
                _developmentSetup, resourceSource.FullName, returnOnlyKeyIfNotFound,
                createNewRecordWhenLocalisedStringDoesNotExist);
            return ResourceLocalizations.GetOrAdd(resourceSource.FullName, sqlStringLocalizer);
        }

        if (ResourceLocalizations.TryGetValue(resourceSource.Name, out var localization)) return localization;

        sqlStringLocalizer = new SqlStringLocalizer(GetAllFromDatabaseForResource(resourceSource.Name),
            _developmentSetup, resourceSource.Name, returnOnlyKeyIfNotFound,
            createNewRecordWhenLocalisedStringDoesNotExist);
        return ResourceLocalizations.GetOrAdd(resourceSource.Name, sqlStringLocalizer);
    }

    public IStringLocalizer Create(string baseName, string location)
    {
        if (ResourceLocalizations.ContainsKey(baseName + location)) return ResourceLocalizations[baseName + location];

        var sqlStringLocalizer = new SqlStringLocalizer(GetAllFromDatabaseForResource(baseName + location),
            _developmentSetup, baseName + location, false, false);
        return ResourceLocalizations.GetOrAdd(baseName + location, sqlStringLocalizer);
    }

    public void ResetCache()
    {
        lock (_context)
        {
            _context.DetachAllEntities();
        }

        foreach (var localizerKey in ResourceLocalizations.Keys)
        {
            var localizer = ResourceLocalizations[localizerKey] as SqlStringLocalizer;

            localizer?.ReloadLocalizations(GetAllFromDatabaseForResource(localizerKey));
        }
    }

    public void ResetCache(Type resourceSource)
    {
        lock (_context)
        {
            _context.DetachAllEntities();
        }

        if (resourceSource.FullName == null ||
            !ResourceLocalizations.TryGetValue(resourceSource.FullName, out _)) return;
        var localizer = ResourceLocalizations[resourceSource.FullName] as SqlStringLocalizer;

        localizer?.ReloadLocalizations(GetAllFromDatabaseForResource(resourceSource.FullName));
    }

    public void UpdateCache(string resourceKey, string culture, string key, string text)
    {
        ResourceLocalizations.TryGetValue(resourceKey, out var localizer);
        var sqlLocalizer = localizer as SqlStringLocalizer;

        sqlLocalizer?.UpdateCache(culture, key, text);
    }

    public IList GetImportHistory()
    {
        lock (_context)
        {
            return _context.ImportHistoryDbSet.ToList();
        }
    }

    public IList GetExportHistory()
    {
        lock (_context)
        {
            return _context.ExportHistoryDbSet.ToList();
        }
    }

    public IList GetLocalizationData(string reason = "export")
    {
        lock (_context)
        {
            _context.ExportHistoryDbSet.Add(new ExportHistory { Reason = reason, Exported = DateTime.UtcNow });
            _context.SaveChanges();

            return _context.LocalizationRecords.ToList();
        }
    }

    public IList GetLocalizationData(DateTime from, string culture = null, string reason = "export")
    {
        lock (_context)
        {
            _context.ExportHistoryDbSet.Add(new ExportHistory { Reason = reason, Exported = DateTime.UtcNow });
            _context.SaveChanges();

            if (culture != null)
                return _context.LocalizationRecords.Where(item =>
                        EF.Property<DateTime>(item, "UpdatedTimestamp") > from &&
                        item.LocalizationCulture == culture)
                    .ToList();

            return _context.LocalizationRecords
                .Where(item => EF.Property<DateTime>(item, "UpdatedTimestamp") > from).ToList();
        }
    }


    public void UpdateLocalizationData(IEnumerable<LocalizationRecord> data, string information)
    {
        lock (_context)
        {
            _context.DetachAllEntities();
            _context.UpdateRange(data);
            _context.ImportHistoryDbSet.Add(new ImportHistory
                { Information = information, Imported = DateTime.UtcNow });
            _context.SaveChanges();
        }
    }

    public void AddNewLocalizationData(IEnumerable<LocalizationRecord> data, string information)
    {
        lock (_context)
        {
            _context.DetachAllEntities();
            _context.AddRange(data);
            _context.ImportHistoryDbSet.Add(new ImportHistory
                { Information = information, Imported = DateTime.UtcNow });
            _context.SaveChanges();
        }
    }

    private Dictionary<string, string> GetAllFromDatabaseForResource(string resourceKey)
    {
        lock (_context)
        {
            return _context.LocalizationRecords.Where(data => data.ResourceKey == resourceKey)
                .ToDictionary(kvp => kvp.Key + "." + kvp.LocalizationCulture, kvp => kvp.Text);
        }
    }
}