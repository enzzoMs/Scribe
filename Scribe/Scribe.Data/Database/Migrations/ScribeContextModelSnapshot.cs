﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Scribe.Data.Database;

#nullable disable

namespace Scribe.Data.Database.Migrations
{
    [DbContext(typeof(ScribeContext))]
    partial class ScribeContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.4");

            modelBuilder.Entity("Scribe.Data.Model.Document", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedTimestamp")
                        .HasColumnType("TEXT");

                    b.Property<int?>("FolderId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsArchived")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsFavorite")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastModifiedTimestamp")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("FolderId");

                    b.ToTable("Documents");
                });

            modelBuilder.Entity("Scribe.Data.Model.Folder", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("NavigationPosition")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Folders");
                });

            modelBuilder.Entity("Scribe.Data.Model.Tag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("DocumentId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("DocumentId");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("Scribe.Data.Model.Document", b =>
                {
                    b.HasOne("Scribe.Data.Model.Folder", null)
                        .WithMany("Documents")
                        .HasForeignKey("FolderId");
                });

            modelBuilder.Entity("Scribe.Data.Model.Tag", b =>
                {
                    b.HasOne("Scribe.Data.Model.Document", null)
                        .WithMany("Tags")
                        .HasForeignKey("DocumentId");
                });

            modelBuilder.Entity("Scribe.Data.Model.Document", b =>
                {
                    b.Navigation("Tags");
                });

            modelBuilder.Entity("Scribe.Data.Model.Folder", b =>
                {
                    b.Navigation("Documents");
                });
#pragma warning restore 612, 618
        }
    }
}
