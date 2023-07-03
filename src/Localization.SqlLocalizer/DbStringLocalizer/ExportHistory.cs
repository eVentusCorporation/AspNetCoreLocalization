using System;

namespace Localization.SqlLocalizer.DbStringLocalizer;

public class ExportHistory
{
    public long Id { get; set; }

    public DateTime Exported { get; set; }

    public string Reason { get; set; }
}