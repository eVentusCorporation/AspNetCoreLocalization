using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Localization;

namespace Localization.SqlLocalizer.DbStringLocalizer;

public class SqlStringLocalizer : IStringLocalizer
{
    private readonly bool _createNewRecordWhenLocalisedStringDoesNotExist;

    private readonly DevelopmentSetup _developmentSetup;
    private ConcurrentDictionary<string, string> _localizations;
    private readonly string _resourceKey;
    private readonly bool _returnKeyOnlyIfNotFound;

    public SqlStringLocalizer(Dictionary<string, string> localizations, DevelopmentSetup developmentSetup,
        string resourceKey, bool returnKeyOnlyIfNotFound, bool createNewRecordWhenLocalisedStringDoesNotExist)
    {
        _localizations = new ConcurrentDictionary<string, string>(localizations);
        _developmentSetup = developmentSetup;
        _resourceKey = resourceKey;
        _returnKeyOnlyIfNotFound = returnKeyOnlyIfNotFound;
        _createNewRecordWhenLocalisedStringDoesNotExist = createNewRecordWhenLocalisedStringDoesNotExist;
    }

    public LocalizedString this[string name]
    {
        get
        {
            var text = GetText(name, out var notSucceed);

            return new LocalizedString(name, text, notSucceed);
        }
    }

    public LocalizedString this[string name, params object[] arguments] => this[name];

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        throw new NotImplementedException();
    }

    public IStringLocalizer WithCulture(CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private string GetText(string key, out bool notSucceed)
    {
#if NET451
            var culture = System.Threading.Thread.CurrentThread.CurrentCulture.ToString();
#elif NET46
            var culture = System.Threading.Thread.CurrentThread.CurrentCulture.ToString();
#else
        var culture = CultureInfo.CurrentCulture.ToString();
#endif
        var computedKey = $"{key}.{culture}";

        if (_localizations.TryGetValue(computedKey, out var result))
        {
            notSucceed = false;
            return result;
        }

        notSucceed = true;
        if (_createNewRecordWhenLocalisedStringDoesNotExist)
        {
            if (_developmentSetup.TryAddNewLocalizedItem(key, culture, _resourceKey, out var text))
                _localizations.TryAdd(computedKey, text);

            return text;
        }

        if (_returnKeyOnlyIfNotFound) return key;

        return _resourceKey + "." + computedKey;
    }

    public void UpdateCache(string culture, string key, string text)
    {
        var computedKey = $"{key}.{culture}";

        if (!_localizations.ContainsKey(computedKey)) return;

        _localizations[computedKey] = text;
    }
    public void ReloadLocalizations(Dictionary<string, string> localizations)
    {
        if (_localizations != null)
            _localizations.Clear();

        _localizations = new ConcurrentDictionary<string, string>(localizations);
    }
}