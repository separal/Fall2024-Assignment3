using Fall2024_Assignment3_separal.Data;
using Fall2024_Assignment3_separal.Models.Actors;
using Fall2024_Assignment3_separal.Models.Movies;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        context.Database.EnsureCreated();

        
        

       
    }
}
