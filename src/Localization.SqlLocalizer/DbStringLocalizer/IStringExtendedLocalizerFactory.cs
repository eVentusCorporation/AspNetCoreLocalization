using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Localization;

namespace Localization.SqlLocalizer.DbStringLocalizer;

public interface IStringExtendedLocalizerFactory : IStringLocalizerFactory
{
    void ResetCache();

    void ResetCache(Type resourceSource);

    IList GetImportHistory();

    IList GetExportHistory();

    IList GetLocalizationData(string reason = "export");

    IList GetLocalizationData(DateTime from, string culture = null, string reason = "export");

    void UpdateLocalizationData(IEnumerable<LocalizationRecord> data, string information);

    void AddNewLocalizationData(IEnumerable<LocalizationRecord> data, string information);

    void UpdateCache(string resourceKey, string culture, string key, string text);
}