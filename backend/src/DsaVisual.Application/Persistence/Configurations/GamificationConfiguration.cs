using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DsaVisual.Application.Persistence.Configurations;

/// <summary>DailyQuests + UserQuests + ShopItems + UserInventory + GemTransactions + PremiumSubscriptions + Achievements + UserAchievements — SDD §7.3.26/7.3.27/7.3.28/7.3.19/7.3.20.</summary>
public sealed class DailyQuestConfiguration : IEntityTypeConfiguration<DailyQuest>
{
    public void Configure(EntityTypeBuilder<DailyQuest> builder)
    {
        builder.ToTable("DailyQuests");

        builder.Property(q => q.QuestKey).HasMaxLength(100).IsRequired();
        builder.Property(q => q.Title).HasMaxLength(200).IsRequired();
        builder.Property(q => q.ConditionJson).IsRequired();
        builder.Property(q => q.RewardJson).IsRequired();
        builder.Property(q => q.PoolEnabled).HasDefaultValue(true);

        builder.HasIndex(q => q.QuestKey).IsUnique();
    }
}

public sealed class UserQuestConfiguration : IEntityTypeConfiguration<UserQuest>
{
    public void Configure(EntityTypeBuilder<UserQuest> builder)
    {
        builder.ToTable("UserQuests");

        builder.Property(q => q.QuestDate).HasColumnType("datetime2");
        builder.Property(q => q.Progress).HasDefaultValue(0);
        builder.Property(q => q.Claimed).HasDefaultValue(false);
        builder.Property(q => q.RowVersion).IsRowVersion();   // concurrency token (finding #3)

        builder.HasIndex(q => new { q.UserId, q.QuestDate, q.QuestId }).IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(q => q.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<DailyQuest>()
            .WithMany()
            .HasForeignKey(q => q.QuestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class ShopItemConfiguration : IEntityTypeConfiguration<ShopItem>
{
    public void Configure(EntityTypeBuilder<ShopItem> builder)
    {
        builder.ToTable("ShopItems");

        builder.Property(i => i.ItemKey).HasMaxLength(100).IsRequired();
        builder.Property(i => i.Name).HasMaxLength(200).IsRequired();
        builder.Property(i => i.PriceGems).HasDefaultValue(0);
        builder.Property(i => i.MaxStack).HasDefaultValue(1);

        builder.HasIndex(i => i.ItemKey).IsUnique();
    }
}

public sealed class UserInventoryConfiguration : IEntityTypeConfiguration<UserInventory>
{
    public void Configure(EntityTypeBuilder<UserInventory> builder)
    {
        builder.ToTable("UserInventory");

        builder.Property(i => i.Quantity).HasDefaultValue(1);
        builder.Property(i => i.IsEquipped).HasDefaultValue(false);   // v2.9 (SDD §7.3.27)
        builder.Property(i => i.PurchasedAt).HasColumnType("datetime2");
        builder.Property(i => i.ExpiresAt).HasColumnType("datetime2");
        builder.Property(i => i.RowVersion).IsRowVersion();           // concurrency token (finding #3)

        builder.HasIndex(i => new { i.UserId, i.ItemId }).IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ShopItem>()
            .WithMany()
            .HasForeignKey(i => i.ItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class GemTransactionConfiguration : IEntityTypeConfiguration<GemTransaction>
{
    public void Configure(EntityTypeBuilder<GemTransaction> builder)
    {
        builder.ToTable("GemTransactions");   // append-only — không xóa mềm (SDD §7.3.27)

        builder.Property(t => t.RefType).HasMaxLength(50);
        builder.Property(t => t.CreatedAt).HasColumnType("datetime2");

        builder.HasIndex(t => new { t.UserId, t.CreatedAt });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class PremiumSubscriptionConfiguration : IEntityTypeConfiguration<PremiumSubscription>
{
    public void Configure(EntityTypeBuilder<PremiumSubscription> builder)
    {
        builder.ToTable("PremiumSubscriptions");

        builder.Property(s => s.PlanId).HasMaxLength(50);
        builder.Property(s => s.OrderRef).HasMaxLength(100);
        builder.Property(s => s.StartedAt).HasColumnType("datetime2");
        builder.Property(s => s.ExpiresAt).HasColumnType("datetime2");
        builder.Property(s => s.Status).HasDefaultValue(0);
        builder.Property(s => s.CreatedAt).HasColumnType("datetime2");
        builder.Property(s => s.RowVersion).IsRowVersion();   // concurrency token (finding #3)

        builder.HasIndex(s => new { s.UserId, s.Status });
        builder.HasIndex(s => new { s.Status, s.ExpiresAt });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class AchievementConfiguration : IEntityTypeConfiguration<Achievement>
{
    public void Configure(EntityTypeBuilder<Achievement> builder)
    {
        builder.ToTable("Achievements");

        builder.Property(a => a.Code).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Name).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Description).HasMaxLength(500);
        builder.Property(a => a.IconUrl).HasMaxLength(500);
        builder.Property(a => a.ConditionJson).IsRequired();

        builder.HasIndex(a => a.Code).IsUnique();
    }
}

public sealed class UserAchievementConfiguration : IEntityTypeConfiguration<UserAchievement>
{
    public void Configure(EntityTypeBuilder<UserAchievement> builder)
    {
        builder.ToTable("UserAchievements");

        builder.Property(a => a.EarnedAt).HasColumnType("datetime2");

        builder.HasIndex(a => new { a.UserId, a.AchievementId }).IsUnique();
        builder.HasIndex(a => a.UserId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Achievement>()
            .WithMany()
            .HasForeignKey(a => a.AchievementId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
