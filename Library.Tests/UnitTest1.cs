using System.Reflection;
using Library.Domain.Models;
using Library.MVC.Controllers;
using Library.MVC.Data;
using Library.MVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Library.Tests;

public class UnitTest1
{
    private ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Cannot_Create_Duplicate_Active_Loan_For_Same_Book()
    {
        using var context = CreateContext();

        var book = new Book
        {
            Id = 1,
            Title = "Clean Code",
            Author = "Robert Martin",
            Isbn = "9781111111111",
            Category = "Technology",
            IsAvailable = false
        };

        var member1 = new Member
        {
            Id = 1,
            FullName = "John Doe",
            Email = "john@test.com",
            Phone = "111111"
        };

        var member2 = new Member
        {
            Id = 2,
            FullName = "Jane Doe",
            Email = "jane@test.com",
            Phone = "222222"
        };

        context.Books.Add(book);
        context.Members.AddRange(member1, member2);
        context.Loans.Add(new Loan
        {
            Id = 1,
            BookId = 1,
            MemberId = 1,
            LoanDate = DateTime.Today.AddDays(-2),
            DueDate = DateTime.Today.AddDays(7),
            ReturnedDate = null
        });

        await context.SaveChangesAsync();

        var controller = new LoanController(context);

        var vm = new LoanCreateViewModel
        {
            BookId = 1,
            MemberId = 2,
            LoanDate = DateTime.Today,
            DueDate = DateTime.Today.AddDays(14)
        };

        var result = await controller.Create(vm);

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Contains(
            controller.ModelState.Values.SelectMany(v => v.Errors),
            e => e.ErrorMessage.Contains("already on an active loan"));

        Assert.Equal(1, await context.Loans.CountAsync());
    }

    [Fact]
    public async Task MarkReturned_Makes_Book_Available_Again()
    {
        using var context = CreateContext();

        var book = new Book
        {
            Id = 1,
            Title = "1984",
            Author = "George Orwell",
            Isbn = "9782222222222",
            Category = "Fiction",
            IsAvailable = false
        };

        var member = new Member
        {
            Id = 1,
            FullName = "Alice Smith",
            Email = "alice@test.com",
            Phone = "333333"
        };

        var loan = new Loan
        {
            Id = 1,
            BookId = 1,
            MemberId = 1,
            LoanDate = DateTime.Today.AddDays(-5),
            DueDate = DateTime.Today.AddDays(5),
            ReturnedDate = null,
            Book = book,
            Member = member
        };

        context.Books.Add(book);
        context.Members.Add(member);
        context.Loans.Add(loan);
        await context.SaveChangesAsync();

        var controller = new LoanController(context);

        var result = await controller.MarkReturned(1);

        Assert.IsType<RedirectToActionResult>(result);

        var savedLoan = await context.Loans.Include(l => l.Book).FirstAsync();
        Assert.NotNull(savedLoan.ReturnedDate);
        Assert.True(savedLoan.Book != null && savedLoan.Book.IsAvailable);
    }

    [Fact]
    public async Task Book_Index_Search_Returns_Expected_Results()
    {
        using var context = CreateContext();

        context.Books.AddRange(
            new Book
            {
                Id = 1,
                Title = "Clean Code",
                Author = "Robert Martin",
                Isbn = "9783333333333",
                Category = "Technology",
                IsAvailable = true
            },
            new Book
            {
                Id = 2,
                Title = "The Hobbit",
                Author = "J.R.R. Tolkien",
                Isbn = "9784444444444",
                Category = "Fantasy",
                IsAvailable = true
            },
            new Book
            {
                Id = 3,
                Title = "Agile Principles",
                Author = "Robert Martin",
                Isbn = "9785555555555",
                Category = "Technology",
                IsAvailable = false
            });

        await context.SaveChangesAsync();

        var controller = new BookController(context);

        var result = await controller.Index("Robert", "", "");

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Book>>(viewResult.Model);
        var books = model.ToList();

        Assert.Equal(2, books.Count);
        Assert.Contains(books, b => b.Title == "Clean Code");
        Assert.Contains(books, b => b.Title == "Agile Principles");
    }

    [Fact]
    public void Loan_IsOverdue_Returns_True_When_DueDate_Has_Passed_And_Not_Returned()
    {
        var loan = new Loan
        {
            LoanDate = DateTime.Today.AddDays(-10),
            DueDate = DateTime.Today.AddDays(-1),
            ReturnedDate = null
        };

        Assert.True(loan.IsActive);
        Assert.True(loan.IsOverdue);
    }

    [Fact]
    public void AdminController_Has_Authorize_Attribute_For_Admin_Role()
    {
        var attribute = typeof(AdminController).GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attribute);
        Assert.Equal("Admin", attribute.Roles);
    }
}