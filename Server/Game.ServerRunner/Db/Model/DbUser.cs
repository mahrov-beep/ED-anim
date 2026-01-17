namespace Game.ServerRunner.Db.Model;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

[Index(nameof(Id), IsUnique = true)]
public class DbUser {
    [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    public DateTime Created { get; set; }

    public static void OnModelCreating(EntityTypeBuilder<DbUser> builder) {
    }
}