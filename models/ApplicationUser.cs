using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace homeCookAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Prevent auto-generation of GUIDs
        public override string Id { get; set; } = GenerateUserId(); // Custom User ID
        
        public string FullName { get; set; }
        public string? ProfileImage { get; set; } //optional
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;

         public ICollection<RecipeRating> Ratings { get; set; } = new List<RecipeRating>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<SavedRecipe> SavedRecipes { get; set; } = new List<SavedRecipe>();

        //without ? list must be initialized (new List<x>()), 
        //also this allows the property to be null if no related records exist

        // Generates a 6-character alphanumeric User ID
        public static string GenerateUserId()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}