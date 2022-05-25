using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace ConsoleApp1
{
	class Program
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		static async Task Main(string[] args)
		{
			using (AppContext db = new AppContext())
			{
				Position manager = new Position { Name = "Manager" };
				Position developer = new Position { Name = "Developer" };
				db.Positions.AddRange(manager, developer);

				City washington = new City { Name = "Washington" };
				db.Cities.Add(washington);

				Country usa = new Country { Name = "USA", Capital = washington };
				db.Countries.Add(usa);

				Company microsoft = new Company { Name = "Microsoft", Country = usa };
				Company google = new Company { Name = "Google", Country = usa };
				db.Companies.AddRange(microsoft, google);

				User tom = new User { Name = "Tom", Company = microsoft, Position = manager };
				User bob = new User { Name = "Bob", Company = google, Position = developer };
				User alice = new User { Name = "Alice", Company = microsoft, Position = developer };
				User kate = new User { Name = "Kate", Company = google, Position = manager };
				db.Users.AddRange(tom, bob, alice, kate);

				db.SaveChanges();
			}
			using (AppContext db = new AppContext())
			{
				// получаем пользователей
				var users = db.Users
					.Include(u => u.Company)  // добавляем данные по компаниям
					.ThenInclude(comp => comp!.Country)      // к компании добавляем страну 
					.ThenInclude(count => count!.Capital)    // к стране добавляем столицу
					.Include(u => u.Position) // добавляем данные по должностям
					.ToList();
				foreach (var user in users)
				{
					Console.WriteLine($"{user.Name} - {user.Position?.Name}");
					Console.WriteLine($"{user.Company?.Name} - {user.Company?.Country?.Name} - {user.Company?.Country?.Capital?.Name}");
					Console.WriteLine("----------------------");     // для красоты
				}
			}

			//explicit loading
			using (var db = new AppContext())
			{
				var company = await db.Companies.FirstOrDefaultAsync();

				if (company != null)
				{
					await db.Users.Where(x => x.CompanyId == company.Id).LoadAsync(); //full analog db.Entry(company).Collection(c => c.Users).Load(); that is used for collections, ot if we have object: db.Entry(user).Reference(u => u.Company).Load();

					Console.WriteLine($"Company: {company.Name}");
					foreach (var u in company.Users)
						Console.WriteLine($"User: {u.Name}");
				}
			}


			using (var db = new AppContext())
			{

				// добавляем начальные данные
				MenuItem file = new MenuItem { Title = "File" };
				MenuItem edit = new MenuItem { Title = "Edit" };
				MenuItem open = new MenuItem { Title = "Open", Parent = file };
				MenuItem save = new MenuItem { Title = "Save", Parent = file };

				MenuItem copy = new MenuItem { Title = "Copy", Parent = edit };
				MenuItem paste = new MenuItem { Title = "Paste", Parent = edit };

				db.MenuItems.AddRange(file, edit, open, save, copy, paste);
				db.SaveChanges();
			}


			using (var db = new AppContext())
			{
				// получаем все пункты меню из бд
				var menuItems = db.MenuItems.ToList();
				Console.WriteLine("All Menu:");
				foreach (MenuItem m in menuItems)
				{
					Console.WriteLine(m.Title);
				}

				Console.WriteLine();
				// получаем определенный пункт меню с подменю
				var fileMenu = db.MenuItems.FirstOrDefault(m => m.Title == "File");
				if (fileMenu != null)
				{
					Console.WriteLine(fileMenu.Title);
					foreach (var m in fileMenu.Children)
					{
						Console.WriteLine($"---{m.Title}");
					}
				}
			}

			// inheritance (TPH - table per hierarchy)
			using (var db = new AppContext())
			{
				var user1 = new Person { Name = "Tom" };
				var user2 = new Person { Name = "Bob" };
				db.Persons.Add(user1);
				db.Persons.Add(user2);

				Employee employee = new Employee { Name = "Sam", Salary = 500 };
				db.Employees.Add(employee);

				Manager manager = new Manager { Name = "Robert", Departament = "IT" };
				db.Managers.Add(manager);

				db.SaveChanges();

				var users = db.Persons.ToList();
				Console.WriteLine("Все пользователи");
				foreach (var user in users)
				{
					Console.WriteLine(user.Name);
				}

				Console.WriteLine("\n Все работники");
				foreach (var emp in db.Employees.ToList())
				{
					Console.WriteLine(emp.Name);
				}

				Console.WriteLine("\nВсе менеджеры");
				foreach (var man in db.Managers.ToList())
				{
					Console.WriteLine(man.Name);
				}

				var manager2 = new Position() { Name = "manager" };
				var hr = new Position() { Name = "HR-BP" };

				await db.Positions.AddRangeAsync(manager2, hr);

			}

			using (var db = new AppContext())
			{
				var users = await (from user in db.Users.Include(x => x.Company)
								   where user.CompanyId == 1 && user.Company!.Name != "ATLANT" // && EF.Functions.Like(user.PositionId, "[2-5]")
								   select user).ToListAsync();

				var oneUser = await (from user2 in db.Users.Include(x => x.Position)
									 where EF.Functions.Like(user2.Name, "_li%")
									 select user2).FirstAsync();

				var sortedUsers = await (from user3 in db.Users.Include(x => x.Company)
										 orderby user3.Name
										 select new { Name = user3.Name, Company = user3.Company!.Name })
					.ToListAsync();
			}

			/*
			 analog:
			var users = db.Users.Join(db.Companies, // второй набор
        u => u.CompanyId, // свойство-селектор объекта из первого набора
        c => c.Id, // свойство-селектор объекта из второго набора
        (u, c) => new // результат
        {
            Name=u.Name, 
            Company = c.Name, 
            Age=u.Age
        });
			 */

			//joins
			using (var db = new AppContext())
			{
				var userCust = await (from user in db.Users
									  join company in db.Companies on user.CompanyId equals company.Id
									  select new
									  {
										  UserId = user.Id,
										  UserName = user.Name,
										  CompanyId = company.Id,
										  CompanyName = company.Name
									  }).ToListAsync();

				var countOfEmesInCOmpanies = await (from user in db.Users
													join company in db.Companies on user.CompanyId equals company.Id
													group company by company.Id
					into companyGroup
													select new
													{
														Company = companyGroup.Key,
														CountOfEmployee = companyGroup.Count()
													}).ToListAsync();

				var countOfTrackableEntitiesAtTheMoment = db.ChangeTracker.Entries().Count();
			}

			using (var db = new AppContext())
			{
				var user1 = db.Users.FirstOrDefault();
				var user2 = db.Users.FirstOrDefault();
				if (user1 != null && user2 != null)
				{
					Console.WriteLine($"Before User1: {user1.Name}   User2: {user2.Name}");

					user1.Name = "Bob";

					Console.WriteLine($"After User1: {user1.Name}   User2: {user2.Name}"); //in second will be Bob as well, cause during initialization there is no call of a DB, EF returns user2 from its cache, so the both are linked to the same reference
				}
			}

			using (var db = new AppContext())
			{
				var companies = await db.Companies.FromSqlRaw("SELECT * FROM Companies").OrderBy(x => x.Name).ToListAsync();

				var param = new SqliteParameter("@name", "%Tom%");
				var users = await db.Users.FromSqlRaw("SELECT * FROM Users WHERE Name LIKE @name", param).ToListAsync();

				// обновление
				string appleInc = "Apple Inc.";
				string apple = "Apple";
				int numberOfRowUpdated = db.Database.ExecuteSqlRaw("UPDATE Companies SET Name={0} WHERE Name={1}", appleInc, apple);

				string jetbrains = "JetBrains";
				//db.Database.ExecuteSqlInterpolated($"INSERT INTO Companies (Name) VALUES ({jetbrains})");
			}

			//pre-compiled queries
			Func<AppContext, int, User?> findUserById = EF.CompileQuery(
				(AppContext context, int id) => context.Users.Include(u => u.Company).FirstOrDefault(x => x.Id == id));

			Func<AppContext, string, int, IEnumerable<User>> findUsersByNameAndPosition = EF.CompileQuery(((AppContext context, string name, int position) =>
					context.Users.Include(x => x.Company).Where(x => x.PositionId == position && EF.Functions.Like(x.Name, name))));

			using (var db = new AppContext())
			{
				var users = findUsersByNameAndPosition(db, "%Tom%", 3);
			}

			using (var db = new AppContext())
			{
				using (var transaction = db.Database.BeginTransaction())
				{
					try
					{
						// отключаем автоматогенерацию идентификатор по добавлению
						db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[Users] ON");
						db.Users.Add(new User());
						await db.SaveChangesAsync();
						// включаем автогенерацию идентификаторов обратно
						db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[Users] OFF");

						await db.Database.CommitTransactionAsync();
						Console.WriteLine("Пользователь восстановлен");
					}
					catch (Exception e)
					{
						await transaction.RollbackAsync();
					}
				}
			}

			Console.ReadLine();
		}
	}
}

