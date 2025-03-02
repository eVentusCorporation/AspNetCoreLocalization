﻿using System;
using Localization.SqlLocalizer.DbStringLocalizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace AspNetCoreLocalization.Migrations;

[DbContext(typeof(LocalizationModelContext))]
internal class LocalizationModelContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "1.1.1");

        modelBuilder.Entity("Localization.SqlLocalizer.DbStringLocalizer.ExportHistory", b =>
        {
            b.Property<long>("Id")
                .ValueGeneratedOnAdd();

            b.Property<DateTime>("Exported");

            b.Property<string>("Reason");

            b.HasKey("Id");

            b.ToTable("ExportHistoryDbSet");
        });

        modelBuilder.Entity("Localization.SqlLocalizer.DbStringLocalizer.ImportHistory", b =>
        {
            b.Property<long>("Id")
                .ValueGeneratedOnAdd();

            b.Property<DateTime>("Imported");

            b.Property<string>("Information");

            b.HasKey("Id");

            b.ToTable("ImportHistoryDbSet");
        });

        modelBuilder.Entity("Localization.SqlLocalizer.DbStringLocalizer.LocalizationRecord", b =>
        {
            b.Property<long>("Id")
                .ValueGeneratedOnAdd();

            b.Property<string>("Key")
                .IsRequired();

            b.Property<string>("LocalizationCulture")
                .IsRequired();

            b.Property<string>("ResourceKey")
                .IsRequired();

            b.Property<string>("Text");

            b.Property<DateTime>("UpdatedTimestamp");

            b.HasKey("Id");

            b.HasAlternateKey("Key", "LocalizationCulture", "ResourceKey");

            b.ToTable("LocalizationRecords");
        });
    }
}