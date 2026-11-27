
using Docuvault.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Docuvault.Api.Data;

public class DocuVaultDbContext : DbContext
{
    public DocuVaultDbContext(DbContextOptions<DocuVaultDbContext> options)
        : base(options)
    {
    }

    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Folder> Folders => Set<Folder>();
    public DbSet<DocumentActivity>DocumentActivities => Set<DocumentActivity>();
    public DbSet<User>Users {get; set;}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("documents", "public");
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Id).HasColumnName("id");
            entity.Property(d => d.UserId).HasColumnName("user_id");
            entity.Property(d => d.Name).HasColumnName("name");
            entity.Property(d => d.Description).HasColumnName("description");
            entity.Property(d => d.Category).HasColumnName("category");
            entity.Property(d => d.ContentType).HasColumnName("content_type");
            entity.Property(d => d.SizeBytes).HasColumnName("size_bytes");
            entity.Property(d => d.StoragePath).HasColumnName("storage_path");
            entity.Property(d => d.IsFavorite).HasColumnName("is_favorite");
            entity.Property(d => d.DeletedAt).HasColumnName("deleted_at");
            entity.Property(d => d.CreatedAt).HasColumnName("created_at");
            entity.Property(d => d.UpdatedAt).HasColumnName("updated_at");
            entity.Property(d => d.FolderId).HasColumnName("folder_id");
        });

        modelBuilder.Entity<Folder>(entity =>
        {
            entity.ToTable("folders");
            entity.HasKey(f => f.Id);
            entity.Property(f => f.Id).HasColumnName("Id");
            entity.Property(f => f.UserId).HasColumnName("user_id");
            entity.Property(f => f.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(f => f.CreatedAt).HasColumnName("created_at");
            entity.Property(f => f.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(f => new {f.UserId, f.Name}).IsUnique();
            entity.HasMany(f => f.Documents).WithOne(d => d.Folder).HasForeignKey(d => d.FolderId).OnDelete(DeleteBehavior.SetNull);

        });

        modelBuilder.Entity<DocumentActivity>(entity =>
        {
            entity.ToTable("document_activities");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id).HasColumnName("id");
            entity.Property(a => a.UserId).HasColumnName("user_id");
            entity.Property(a => a.DocumentId).HasColumnName("document_id");
            entity.Property(a => a.DocumentName).HasColumnName("document_name").IsRequired();
            entity.Property(a => a.Action).HasColumnName("action").HasMaxLength(50).IsRequired();
            entity.Property(a => a.CreatedAt).HasColumnName("created_at");
            entity.HasOne(a => a.Document).WithMany().HasForeignKey(a => a.DocumentId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(a => a.UserId);
            entity.HasIndex(a => a.DocumentId);
            entity.HasIndex(a => a.CreatedAt);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users", "public");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).HasColumnName("id");
            entity.Property(u => u.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.Property(u => u.FullName).HasColumnName("full_name").HasMaxLength(150);
            entity.Property(u => u.Role).HasColumnName("role").HasMaxLength(20).IsRequired();
            entity.Property(u => u.IsActive).HasColumnName("is_active").IsRequired();
            entity.Property(u => u.CreatedAt).HasColumnName("created_at");
            entity.Property(u => u.UpdatedAt).HasColumnName("updated_at");

        });
    }
}