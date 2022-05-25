using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleApp1.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ConsoleApp1
{
	public class AppContext : DbContext
	{
		public DbSet<User> Users { get; set; } = null!;
		public DbSet<Company> Companies { get; set; } = null!;
		public DbSet<City> Cities { get; set; } = null!;
		public DbSet<Country> Countries { get; set; } = null!;
		public DbSet<Position> Positions { get; set; } = null!;

		public DbSet<Course> Courses { get; set; } = null!;
		public DbSet<Student> Students { get; set; } = null!;

		public DbSet<MenuItem> MenuItems { get; set; } = null!;



		public DbSet<Person> Persons { get; set; } = null!;
		public DbSet<Employee> Employees { get; set; } = null!;
		public DbSet<Manager> Managers { get; set; } = null!;

		public IQueryable<User> GetUserByAge(int age) => FromExpression(() => GetUserByAge(age));

		public AppContext()
		{
			//Database.EnsureDeleted();
			Database.EnsureCreated();
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite("Data Source=helloapp3.db");
			optionsBuilder.UseLoggerFactory(EFFileLoggerProvider);
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			modelBuilder.Entity<Course>()
				.HasMany(x => x.Students)
				.WithMany(x => x.Courses);

			modelBuilder.HasDbFunction(() => GetUserByAge(default)); // use it
		}

		public static readonly ILoggerFactory EFFileLoggerProvider = LoggerFactory.Create(builder =>
		{
			builder.AddProvider(new EFFileLoggerProvider());

			//a way to use more opened to settings approach:
			//builder.AddFilter((category, level) => category == DbLoggerCategory.Database.Command.Name
			//                                       && level == LogLevel.Information)
			//	.AddProvider(new MyLoggerProvider());
		});
	}
}
