using Microsoft.EntityFrameworkCore;
using SuperheroApi.DbModels;


namespace SuperheroApi
{
    //Uses entity framework to generate the in-memory database for the api
    public class ApiContext : DbContext 
    {
        public ApiContext(DbContextOptions<ApiContext> options)
            : base(options)
        {
          
        }

        //Holds all superheros and their personal data
        public DbSet<Superhero> Users { get; set; }

        //Holds all the squations (a list of superheros)
        public DbSet<Squadron> Squad { get; set; }

    }
}
