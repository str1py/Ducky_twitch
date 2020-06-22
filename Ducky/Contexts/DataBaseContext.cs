using Ducky.Models;
using Microsoft.EntityFrameworkCore;


namespace Ducky.Contexts
{
    public class DataBaseContext : DbContext
    {
        public DbSet<TwitchUserModel> Users { get; set; }
        public DbSet<LevelsModel> Levels { get; set; }
        public DbSet<ActivityCountModel> ActivityCount { get; set; }
        public DbSet<ActivityModel> Activity { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL("server=remotemysql.com;Port=3306;database=HyvDyV4OuD;user=HyvDyV4OuD;password=Eg2OKjtQxe");
            // optionsBuilder.UseMySQL("server=127.0.0.1;database=local_db;user=root;password=root");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<TwitchUserModel>()
            //    .ToTable("Users", "local_db");

            //modelBuilder.Entity<LevelsModel>()
            //    .ToTable("Levels", "local_db");

            //modelBuilder.Entity<ActivityCountModel>()
            //    .ToTable("ActivityCount", "local_db");



            modelBuilder.Entity<TwitchUserModel>()
                .ToTable("Users", "HyvDyV4OuD");

            modelBuilder.Entity<LevelsModel>()
                .ToTable("Levels", "HyvDyV4OuD");

            modelBuilder.Entity<ActivityCountModel>()
                .ToTable("ActivityCount", "HyvDyV4OuD");

            modelBuilder.Entity<ActivityModel>()
             .ToTable("Activity", "HyvDyV4OuD");

        }
    }
}
