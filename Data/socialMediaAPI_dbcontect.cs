using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SocialMediaAPI.Models.Domain.User;


namespace SocialMediaAPI.Data;
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Share> Shares { get; set; }
    public DbSet<Follow> Follows { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql();
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);


                // Add PostgreSQL-specific configurations
        builder.HasDefaultSchema("public");

        // Configure case-insensitive string comparison
        builder.UseCollation("und-u-ks-level2");

        // Add timestamp columns
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (entityType.ClrType.GetProperty("CreatedAt") != null)
            {
                builder.Entity(entityType.ClrType)
                    .Property("CreatedAt")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            }
            if (entityType.ClrType.GetProperty("UpdatedAt") != null)
            {
                builder.Entity(entityType.ClrType)
                    .Property("UpdatedAt")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            }
        }
        

        //Relationship configuration for Post
        builder.Entity<Post>()
            .HasOne(u => u.User)
            .WithMany(p => p.Posts)
            .HasForeignKey(u => u.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        //Relationship configuration for Comment
        builder.Entity<Comment>()
            .HasOne(u => u.Post)
            .WithMany(c => c.Comments)
            .HasForeignKey(u => u.PostId)
            .OnDelete(DeleteBehavior.Restrict);

        //Relationship configuration for Like on Post
        builder.Entity<Like>()
            .HasOne(P => P.Post)
            .WithMany(l => l.Likes)
            .HasForeignKey(l => l.PostId)
            .OnDelete(DeleteBehavior.NoAction);

        //Relationship configuration for Like on Comment
        builder.Entity<Like>()
            .HasOne(c => c.Comment)
            .WithMany(l => l.Likes)
            .HasForeignKey(l => l.CommentId)
            .OnDelete(DeleteBehavior.NoAction);

        //Relationship configuration for Like on User
        builder.Entity<Like>()
            .HasOne(u => u.User)
            .WithMany(l => l.Likes)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        //Relationship configuration for Follower
        builder.Entity<Follow>()
            .HasOne(f => f.Follower)
            .WithMany(f => f.Followers)
            .HasForeignKey(f => f.FollowerUserId)
            .OnDelete(DeleteBehavior.NoAction);

        //Relationship configuration for Following
        builder.Entity<Follow>()
            .HasOne(f => f.Following)
            .WithMany(f => f.Following)
            .HasForeignKey(f => f.FollowingUserId)
            .OnDelete(DeleteBehavior.NoAction);

        //Relationship configuration for Share on Post
        builder.Entity<Share>()
            .HasOne(p => p.Post)
            .WithMany(s => s.Shares)
            .HasForeignKey(s => s.PostId)
            .OnDelete(DeleteBehavior.Restrict);

        //Relationship configuration for Share on User
        builder.Entity<Share>()
            .HasOne(u => u.User)
            .WithMany(s => s.Shares)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Like>()
            .HasIndex(l => new { l.UserId, l.PostId })
            .IsUnique();
        
        builder.Entity<Like>()
            .HasIndex(l => new { l.UserId, l.CommentId })
            .IsUnique();

        builder.Entity<Follow>()
            .HasIndex(f => new { f.FollowerUserId, f.FollowingUserId })
            .IsUnique();
        
        builder.Entity<Share>()
            .HasIndex(s => new { s.UserId, s.PostId })
            .IsUnique(false);

        builder.Entity<Comment>()
            .HasIndex(c => new { c.UserId, c.PostId })
            .IsUnique(false);

        builder.Entity<Post>()
            .HasIndex(p => new { p.UserId, p.Content })
            .IsUnique(false);

        builder.Entity<Follow>()
            .HasKey(f => new { f.FollowerUserId, f.FollowingUserId });

        builder.Entity<Comment>()
            .HasIndex(c => c.CreatedAt);

        builder.Entity<Post>()
            .HasIndex(c => c.CreatedAt);

        builder.Entity<Share>()
            .HasIndex(c => c.CreatedAt);
    }
}