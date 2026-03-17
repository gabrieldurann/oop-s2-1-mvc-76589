using Bogus;
using Library.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Library.MVC.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        var adminEmail = "admin@library.local";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
        else if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        if (!context.Books.Any())
        {
            var categories = new[] { "Fiction", "History", "Science", "Biography", "Technology" };

            var books = new Faker<Book>()
                .RuleFor(b => b.Title, f => f.Lorem.Sentence(3))
                .RuleFor(b => b.Author, f => f.Name.FullName())
                .RuleFor(b => b.Isbn, f => f.Random.ReplaceNumbers("978##########"))
                .RuleFor(b => b.Category, f => f.PickRandom(categories))
                .RuleFor(b => b.IsAvailable, true)
                .Generate(20);

            context.Books.AddRange(books);
            await context.SaveChangesAsync();
        }

        if (!context.Members.Any())
        {
            var members = new Faker<Member>()
                .RuleFor(m => m.FullName, f => f.Name.FullName())
                .RuleFor(m => m.Email, f => f.Internet.Email())
                .RuleFor(m => m.Phone, f => f.Phone.PhoneNumber())
                .Generate(10);

            context.Members.AddRange(members);
            await context.SaveChangesAsync();
        }

        if (!context.Loans.Any())
        {
            var books = await context.Books.ToListAsync();
            var members = await context.Members.ToListAsync();
            var random = new Random();

            var chosenBooks = books.Take(15).ToList();
            var loans = new List<Loan>();

            for (int i = 0; i < 15; i++)
            {
                var book = chosenBooks[i];
                var member = members[random.Next(members.Count)];
                var loanDate = DateTime.Today.AddDays(-random.Next(5, 30));
                var dueDate = loanDate.AddDays(14);

                DateTime? returnedDate = null;

                if (i < 5)
                {
                    returnedDate = dueDate.AddDays(-2);
                    book.IsAvailable = true;
                }
                else
                {
                    book.IsAvailable = false;
                }

                loans.Add(new Loan
                {
                    BookId = book.Id,
                    MemberId = member.Id,
                    LoanDate = loanDate,
                    DueDate = dueDate,
                    ReturnedDate = returnedDate
                });
            }

            context.Loans.AddRange(loans);
            await context.SaveChangesAsync();
        }
    }
}