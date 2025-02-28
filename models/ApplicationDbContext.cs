using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace homeCookAPI.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<RecipeRating> RecipeRatings { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<SavedRecipe> SavedRecipes { get; set; }

        // Ensure SQLite enforces foreign key constraints
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=homeCook.db;");
            }

            // Enable foreign keys globally before opening a connection
            using (var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=homeCook.db"))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "PRAGMA foreign_keys = ON;";
                    command.ExecuteNonQuery();
                }
            }
        }


        // Ensure foreign key enforcement on every save
        public override int SaveChanges()
        {
            Database.ExecuteSqlRaw("PRAGMA foreign_keys=ON;");
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=ON;", cancellationToken);
            return await base.SaveChangesAsync(cancellationToken);
        }

        // Define Relationships and Cascade Delete Rules
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); //Required for Identity

            // Comments - Delete user -> Delete comments
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Recipes - Delete user -> Delete recipes
            modelBuilder.Entity<Recipe>()
                .HasOne(r => r.User)
                .WithMany(u => u.Recipes)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Saved Recipes - Delete user -> Delete saved recipes
            modelBuilder.Entity<SavedRecipe>()
                .HasOne(sr => sr.User)
                .WithMany(u => u.SavedRecipes)
                .HasForeignKey(sr => sr.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Likes - Delete user -> Delete likes
            modelBuilder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Recipe Ratings - Delete user -> Delete ratings
            modelBuilder.Entity<RecipeRating>()
                .HasOne(rr => rr.User)
                .WithMany(u => u.RecipeRatings)
                .HasForeignKey(rr => rr.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}