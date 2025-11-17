using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoListApp.Entities;

namespace TodoListApp.DataAccess.Configuration;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(t => t.Name)
            .IsUnique();


        builder.HasMany(tag => tag.Tasks)
            .WithMany(task => task.Tags)
            .UsingEntity<Dictionary<string, object>>(
                "TaskTag",
                j => j
                    .HasOne<TaskItem>()
                    .WithMany()
                    .HasForeignKey("TaskId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne<Tag>()
                    .WithMany()
                    .HasForeignKey("TagId")
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.ToTable("TaskTags");
                    j.HasKey("TagId", "TaskId");
                    j.HasIndex("TaskId");
                    j.HasIndex("TagId");
                }
            );
    }
}
