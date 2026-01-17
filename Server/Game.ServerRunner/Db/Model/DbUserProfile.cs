namespace Game.ServerRunner.Db.Model;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

[Index(nameof(Id), IsUnique = true)]
[Index(nameof(NickName), IsUnique = true)]
public class DbUserProfile {
    [Key, Required]
    public Guid Id { get; set; }
    
    [Required, Column(TypeName = "citext")]  
    public string NickName { get; set; }

    [Required]
    public byte[] Data { get; set; }

    public static void OnModelCreating(EntityTypeBuilder<DbUserProfile> builder) {
        builder.Property(profile => profile.NickName)
                        .HasColumnType("citext");

        builder.HasOne<DbUser>()
            .WithOne()
            .HasForeignKey<DbUserProfile>(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}