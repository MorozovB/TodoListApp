using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoListApp.Entities;

namespace TodoListApp.DataAccess.Configuration;
public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("Tasks");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        builder.Property(t => t.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(t => t.Priority)
            .IsRequired()
            .HasDefaultValue(Entities.Enums.TaskPriority.Medium);

        builder.Property(testc => testc.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(false);

        builder.Property(t => t.IsCompleted)
            .IsRequired()
            .HasDefaultValue(false);


        builder.HasOne(t => t.TodoList)
            .WithMany(tl => tl.Tasks)
            .HasForeignKey(t => t.TodoListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.TodoListId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.DueDate);
    }
}
