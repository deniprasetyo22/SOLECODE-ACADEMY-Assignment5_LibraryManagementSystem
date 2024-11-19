using Assignment5.Application.DTOs;
using Assignment5.Application.Interfaces.IRepositories;
using Assignment5.Domain.Models;
using Assignment5.Persistence.Context;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment5.Persistence.Repositories
{
    public class BookRepository:IBookRepository
    {
        private readonly LibraryContext _context;

        public BookRepository(LibraryContext context)
        {
            _context = context;
        }

        public async Task<Book> AddBook(Book book)
        {
            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();
            return book;
        }

        public async Task<IEnumerable<Book>> GetAllBooks()
        {
            return await _context.Books.Where(b => !b.status.Contains("Deleted")).ToListAsync();
        }

        public async Task<Book> GetBookById(int bookId)
        {
            return await _context.Books.FirstOrDefaultAsync(b => b.bookId == bookId && !b.status.Contains("Deleted"));
        }

        public async Task<bool> UpdateBook(Book book)
        {
            _context.Books.Update(book);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteBook(Book book)
        {
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<object> SearchBooksAsync(QueryObject query)
        {
            // Start with the base query
            var temp = _context.Books.AsQueryable();

            // Filter by keyword if provided
            if (!string.IsNullOrEmpty(query.Keyword))
            {
                string keywordLower = query.Keyword.ToLower();
                temp = temp.Where(b => b.title.ToLower().Contains(keywordLower) ||
                                       b.author.ToLower().Contains(keywordLower) ||
                                       b.ISBN.ToLower().Contains(keywordLower));
            }

            // Further filtering based on individual properties
            if (!string.IsNullOrEmpty(query.Title))
                temp = temp.Where(b => b.title.ToLower().Contains(query.Title.ToLower()));

            if (!string.IsNullOrEmpty(query.Author))
                temp = temp.Where(b => b.author.ToLower().Contains(query.Author.ToLower()));

            if (!string.IsNullOrEmpty(query.ISBN))
                temp = temp.Where(b => b.ISBN.ToLower().Contains(query.ISBN.ToLower()));

            // Count total results asynchronously
            var total = await temp.CountAsync();

            // Sorting logic
            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                switch (query.SortBy.ToLower()) // handle case insensitivity
                {
                    case "title":
                        temp = query.SortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase)
                            ? temp.OrderBy(s => s.title)
                            : temp.OrderByDescending(s => s.title);
                        break;
                    case "author":
                        temp = query.SortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase)
                            ? temp.OrderBy(s => s.author)
                            : temp.OrderByDescending(s => s.author);
                        break;
                    case "isbn":
                        temp = query.SortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase)
                            ? temp.OrderBy(s => s.ISBN)
                            : temp.OrderByDescending(s => s.ISBN);
                        break;
                    default:
                        temp = query.SortOrder.Equals("asc")
                            ? temp.OrderBy(s => s.bookId)
                            : temp.OrderByDescending(s => s.bookId);
                        break;
                }
            }

            // Pagination logic
            var skipNumber = (query.PageNumber - 1) * query.PageSize;

            // Fetch the data asynchronously
            var list = await temp.Skip(skipNumber).Take(query.PageSize).ToListAsync();

            // Return the results
            return new { total = total, data = list };
        }



    }
}
