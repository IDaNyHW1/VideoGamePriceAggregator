using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace VideoGamePriceAggregator.Models;

public partial class VideoGamePriceAggregatorContext : DbContext
{
    public VideoGamePriceAggregatorContext()
    {
    }

    public VideoGamePriceAggregatorContext(DbContextOptions<VideoGamePriceAggregatorContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Developer> Developers { get; set; }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<Game> Games { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<Offer> Offers { get; set; }

    public virtual DbSet<Platform> Platforms { get; set; }

    public virtual DbSet<PriceAlert> PriceAlerts { get; set; }

    public virtual DbSet<PriceHistory> PriceHistories { get; set; }

    public virtual DbSet<Publisher> Publishers { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Store> Stores { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=VideoGamePriceAggregator;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Developer>(entity =>
        {
            entity.HasKey(e => e.DeveloperId).HasName("PK__Develope__DE084CD1FDD0F50A");

            entity.HasIndex(e => e.Name, "UQ__Develope__737584F6D6954F04").IsUnique();

            entity.Property(e => e.DeveloperId).HasColumnName("DeveloperID");
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Website).HasMaxLength(255);
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.GameId }).HasName("PK__Favorite__D52345D19D951BB3");

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.GameId).HasColumnName("GameID");
            entity.Property(e => e.AddedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Game).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.GameId)
                .HasConstraintName("FK_Favorites_Games");

            entity.HasOne(d => d.User).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Favorites_Users");
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.GameId).HasName("PK__Games__2AB897DDBD974D1D");

            entity.HasIndex(e => e.Title, "IX_Games_Title");

            entity.Property(e => e.GameId).HasColumnName("GameID");
            entity.Property(e => e.AgeRating).HasMaxLength(20);
            entity.Property(e => e.DeveloperId).HasColumnName("DeveloperID");
            entity.Property(e => e.PublisherId).HasColumnName("PublisherID");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Developer).WithMany(p => p.Games)
                .HasForeignKey(d => d.DeveloperId)
                .HasConstraintName("FK_Games_Developers");

            entity.HasOne(d => d.Publisher).WithMany(p => p.Games)
                .HasForeignKey(d => d.PublisherId)
                .HasConstraintName("FK_Games_Publishers");

            entity.HasMany(d => d.Genres).WithMany(p => p.Games)
                .UsingEntity<Dictionary<string, object>>(
                    "GameGenre",
                    r => r.HasOne<Genre>().WithMany()
                        .HasForeignKey("GenreId")
                        .HasConstraintName("FK_GameGenres_Genres"),
                    l => l.HasOne<Game>().WithMany()
                        .HasForeignKey("GameId")
                        .HasConstraintName("FK_GameGenres_Games"),
                    j =>
                    {
                        j.HasKey("GameId", "GenreId").HasName("PK__GameGenr__DA80C788C1B6111B");
                        j.ToTable("GameGenres");
                        j.IndexerProperty<int>("GameId").HasColumnName("GameID");
                        j.IndexerProperty<int>("GenreId").HasColumnName("GenreID");
                    });

            entity.HasMany(d => d.Platforms).WithMany(p => p.Games)
                .UsingEntity<Dictionary<string, object>>(
                    "GamePlatform",
                    r => r.HasOne<Platform>().WithMany()
                        .HasForeignKey("PlatformId")
                        .HasConstraintName("FK_GamePlatforms_Platforms"),
                    l => l.HasOne<Game>().WithMany()
                        .HasForeignKey("GameId")
                        .HasConstraintName("FK_GamePlatforms_Games"),
                    j =>
                    {
                        j.HasKey("GameId", "PlatformId").HasName("PK__GamePlat__95ED08B03DDD7D7E");
                        j.ToTable("GamePlatforms");
                        j.IndexerProperty<int>("GameId").HasColumnName("GameID");
                        j.IndexerProperty<int>("PlatformId").HasColumnName("PlatformID");
                    });
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.GenreId).HasName("PK__Genres__0385055E5504892B");

            entity.HasIndex(e => e.Name, "UQ__Genres__737584F699277AD9").IsUnique();

            entity.Property(e => e.GenreId).HasColumnName("GenreID");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Offer>(entity =>
        {
            entity.HasKey(e => e.OfferId).HasName("PK__Offers__8EBCF0B14A069D92");

            entity.HasIndex(e => e.GameId, "IX_Offers_GameID");

            entity.HasIndex(e => e.PlatformId, "IX_Offers_PlatformID");

            entity.HasIndex(e => e.Price, "IX_Offers_Price");

            entity.HasIndex(e => e.StoreId, "IX_Offers_StoreID");

            entity.HasIndex(e => new { e.GameId, e.PlatformId, e.StoreId }, "UQ_Offers").IsUnique();

            entity.Property(e => e.OfferId).HasColumnName("OfferID");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasDefaultValue("EUR")
                .IsFixedLength();
            entity.Property(e => e.DiscountPercent).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.GameId).HasColumnName("GameID");
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            entity.Property(e => e.LastUpdated).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.PlatformId).HasColumnName("PlatformID");
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductUrl)
                .HasMaxLength(1000)
                .HasColumnName("ProductURL");
            entity.Property(e => e.StoreId).HasColumnName("StoreID");

            entity.HasOne(d => d.Game).WithMany(p => p.Offers)
                .HasForeignKey(d => d.GameId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Offers_Games");

            entity.HasOne(d => d.Platform).WithMany(p => p.Offers)
                .HasForeignKey(d => d.PlatformId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Offers_Platforms");

            entity.HasOne(d => d.Store).WithMany(p => p.Offers)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Offers_Stores");
        });

        modelBuilder.Entity<Platform>(entity =>
        {
            entity.HasKey(e => e.PlatformId).HasName("PK__Platform__F559F6DA34801BE0");

            entity.HasIndex(e => e.Name, "UQ__Platform__737584F681D3988F").IsUnique();

            entity.Property(e => e.PlatformId).HasColumnName("PlatformID");
            entity.Property(e => e.Manufacturer).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<PriceAlert>(entity =>
        {
            entity.HasKey(e => e.AlertId).HasName("PK__PriceAle__EBB16AEDD13D4FE9");

            entity.Property(e => e.AlertId).HasColumnName("AlertID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasDefaultValue("EUR")
                .IsFixedLength();
            entity.Property(e => e.GameId).HasColumnName("GameID");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.TargetPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Game).WithMany(p => p.PriceAlerts)
                .HasForeignKey(d => d.GameId)
                .HasConstraintName("FK_PriceAlerts_Games");

            entity.HasOne(d => d.User).WithMany(p => p.PriceAlerts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_PriceAlerts_Users");
        });

        modelBuilder.Entity<PriceHistory>(entity =>
        {
            entity.HasKey(e => e.PriceHistoryId).HasName("PK__PriceHis__A927CB2B41DEF39A");

            entity.ToTable("PriceHistory");

            entity.HasIndex(e => new { e.OfferId, e.RecordedAt }, "IX_PriceHistory_OfferID_Date");

            entity.Property(e => e.PriceHistoryId).HasColumnName("PriceHistoryID");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.DiscountPercent).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.OfferId).HasColumnName("OfferID");
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.RecordedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Offer).WithMany(p => p.PriceHistories)
                .HasForeignKey(d => d.OfferId)
                .HasConstraintName("FK_PriceHistory_Offers");
        });

        modelBuilder.Entity<Publisher>(entity =>
        {
            entity.HasKey(e => e.PublisherId).HasName("PK__Publishe__4C657E4B869042FD");

            entity.HasIndex(e => e.Name, "UQ__Publishe__737584F6F42B3AC0").IsUnique();

            entity.Property(e => e.PublisherId).HasColumnName("PublisherID");
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Website).HasMaxLength(255);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__Reviews__74BC79AE759194F0");

            entity.HasIndex(e => e.GameId, "IX_Reviews_GameID");

            entity.HasIndex(e => new { e.UserId, e.GameId }, "UQ_Reviews_User_Game").IsUnique();

            entity.Property(e => e.ReviewId).HasColumnName("ReviewID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.GameId).HasColumnName("GameID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Game).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.GameId)
                .HasConstraintName("FK_Reviews_Games");

            entity.HasOne(d => d.User).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Reviews_Users");
        });

        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasKey(e => e.StoreId).HasName("PK__Stores__3B82F0E1494AED80");

            entity.HasIndex(e => e.Name, "UQ__Stores__737584F6AFB3C4AA").IsUnique();

            entity.Property(e => e.StoreId).HasColumnName("StoreID");
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Website).HasMaxLength(255);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC0EBDC38A");

            entity.HasIndex(e => e.Login, "UQ__Users__5E55825B6777145D").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534963530C6").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Login).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.RegistrationDate).HasDefaultValueSql("(sysdatetime())");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
