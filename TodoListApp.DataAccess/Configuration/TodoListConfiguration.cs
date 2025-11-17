using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoListApp.Entities;

namespace TodoListApp.DataAccess.Configuration;
public class TodoListConfiguration : IEntityTypeConfiguration<TodoListEntity>
{
    public void Configure(EntityTypeBuilder<TodoListEntity> builder)
    {
        builder.ToTable("TodoLists");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        builder.Property(t => t.OwnerId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(t => t.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasMany(t => t.Tasks)
            .WithOne(task => task.TodoList)
            .HasForeignKey(task => task.TodoListId)
            .OnDelete(DeleteBehavior.Cascade);

        // Индексы
        builder.HasIndex(t => t.OwnerId);
        builder.HasIndex(t => t.CreatedDate);
    }
}
