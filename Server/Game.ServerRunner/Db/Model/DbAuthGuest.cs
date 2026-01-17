namespace Game.ServerRunner.Db.Model;

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

[Index(nameof(DeviceId), IsUnique = true)]
public class DbAuthGuest {
    [Key, Required, MaxLength(100)]
    public string DeviceId { get; set; }

    [Required]
    public DbUser User { get; set; }

    public static void OnModelCreating(EntityTypeBuilder<DbAuthGuest> builder) {
    }
}