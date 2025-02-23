using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace homeCookAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string? ProfileImage { get; set; } // Optional
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;

        public ICollection<RecipeRating> Ratings { get; set; } = new List<RecipeRating>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<SavedRecipe> SavedRecipes { get; set; } = new List<SavedRecipe>();
    }
}
