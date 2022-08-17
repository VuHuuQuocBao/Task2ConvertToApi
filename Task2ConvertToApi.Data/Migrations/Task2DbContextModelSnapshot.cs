﻿// <auto-generated />
using System;
using Data.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Task2ConvertToApi.Data.Migrations
{
    [DbContext(typeof(Task2DbContext))]
    partial class Task2DbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Data.Entities.UserDailyTimesheetModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime?>("CheckInTime")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("CheckOutTime")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("Day")
                        .HasColumnType("int");

                    b.Property<bool?>("IsLate")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("Month")
                        .HasColumnType("int");

                    b.Property<string>("Status")
                        .HasColumnType("longtext");

                    b.Property<int?>("TotalActualWorkingTimeInSeconds")
                        .HasColumnType("int");

                    b.Property<int?>("TotalLateInSeconds")
                        .HasColumnType("int");

                    b.Property<Guid>("UserId")
                        .HasColumnType("char(36)");

                    b.Property<int>("Year")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("UserDailyTimesheetModels");
                });
#pragma warning restore 612, 618
        }
    }
}
