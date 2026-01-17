namespace Game.ServerRunner.Db.Model;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.DTO;
public enum EFriendStatus {
    None     = 0,
    Pending  = 1,
    Accepted = 2,
}

public class DbFriendship {
    [Key] public long Id { get; set; }

    [Required] public Guid RequesterId { get; set; }
    [Required] public Guid AddresseeId { get; set; }

    [Required] public EFriendStatus Status     { get; set; }
    [Required] public DateTime      Created    { get; set; }
    public            DateTime?     AcceptedAt { get; set; }
    public            DateTime?     Updated    { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public Guid UserA { get; private set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public Guid UserB { get; private set; }

    public static void OnModelCreating(EntityTypeBuilder<DbFriendship> b) {
        // самого себя нельзя подружить
        b.ToTable("Friendships", (TableBuilder<DbFriendship> tb) => {
            tb.HasCheckConstraint("CK_Friendship_NotSelf", "\"RequesterId\" <> \"AddresseeId\"");
        });

        b.Property((DbFriendship x) => x.Id).ValueGeneratedOnAdd();

        /// упорядоченная пара для гарантии что при встречном френд инвайте не создатся дубликат в БД (userA, userB)
        {

            b.Property(propertyExpression: x => x.UserA).HasComputedColumnSql(
                            "CASE WHEN \"RequesterId\" < \"AddresseeId\" THEN \"RequesterId\" ELSE \"AddresseeId\" END",
                            stored: true);

            b.Property(propertyExpression: x => x.UserB).HasComputedColumnSql(
                            "CASE WHEN \"RequesterId\" > \"AddresseeId\" THEN \"RequesterId\" ELSE \"AddresseeId\" END",
                            stored: true);
        }

        // для быстрого поиска в бд статуса дружбы
        b.HasIndex(indexExpression: x => new { x.UserA, x.UserB }).IsUnique();

        // на всякий случай для быстрого поиска
        b.HasIndex(indexExpression: x => new { x.RequesterId, x.Status });
        b.HasIndex(indexExpression: x => new { x.AddresseeId, x.Status });

        // при удалении пользователя чистим его из всех списков друзей, на всякий случай, если надо будет удалить акк
        {
            b.HasOne<DbUser>().WithMany()
                            .HasForeignKey(x => x.RequesterId)
                            .OnDelete(DeleteBehavior.Cascade);

            b.HasOne<DbUser>().WithMany()
                            .HasForeignKey(x => x.AddresseeId)
                            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}