using Assignment5.Application.DTOs;
using Assignment5.Application.Interfaces.IRepositories;
using Assignment5.Application.Interfaces.IService;
using Assignment5.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment5.Application.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;

        public BookService(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<Book> AddBook(Book book)
        {
            if (book == null)
            {
                throw new ArgumentNullException("Book data cannot be null.");
            }

            var existingBooks = await _bookRepository.GetAllBooks();
            if (existingBooks.Any(b => b.ISBN.Equals(book.ISBN, StringComparison.OrdinalIgnoreCase) ||
                b.title.Equals(book.title, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("A book with the same ISBN or Title already exists.");
            }

            return await _bookRepository.AddBook(book);
        }

        public async Task<IEnumerable<ShowBookDto>> GetAllBooks(paginationDto pagination)
        {
            if (pagination.pageNumber <= 0 || pagination.pageSize <= 0)
            {
                throw new ArgumentException("Page number and page size must be greater than zero.");
            }

            var books = await _bookRepository.GetAllBooks();
            if (books == null || !books.Any())
            {
                throw new InvalidOperationException("No books found.");
            }

            return books.Select(b => new ShowBookDto
            {
                bookId = b.bookId,
                category = b.category,
                title = b.title,
                ISBN = b.ISBN,
                author = b.author,
                publisher = b.publisher,
                description = b.description,
                location = b.location,
                totalBook = b.totalBook,
                language = b.language,
            })
            .Skip((pagination.pageNumber - 1) * pagination.pageSize)
            .Take(pagination.pageSize)
            .OrderBy(b => b.title)
            .ToList();
        }

        public async Task<IEnumerable<Book>> GetAllBooksNoPages()
        {
            var books = await _bookRepository.GetAllBooks();
            if (books == null || !books.Any())
            {
                return Enumerable.Empty<Book>();
            }

            return books;
        }

        public async Task<ShowBookDto> GetBookById(int bookId)
        {
            if (bookId <= 0)
            {
                throw new ArgumentException("Book ID must be greater than zero.");
            }

            var book = await _bookRepository.GetBookById(bookId);
            if (book == null)
            {
                throw new KeyNotFoundException($"Book with ID {bookId} was not found.");
            }

            return new ShowBookDto
            {
                bookId = book.bookId,
                category = book.category,
                title = book.title,
                ISBN = book.ISBN,
                author = book.author,
                publisher = book.publisher,
                description = book.description,
                location = book.location,
                price = book.price,
                totalBook = book.totalBook,
                language = book.language,
            };
        }

        public async Task<bool> UpdateBook(int bookId, Book book)
        {
            if (book == null)
            {
                throw new ArgumentNullException("Book data cannot be null.");
            }

            if (bookId <= 0)
            {
                throw new ArgumentException("Book ID must be greater than zero.");
            }

            var existingBook = await _bookRepository.GetBookById(bookId);
            if (existingBook == null)
            {
                throw new KeyNotFoundException($"Book with ID {bookId} was not found.");
            }

            bool isTitleChanged = !existingBook.title.Equals(book.title, StringComparison.OrdinalIgnoreCase);
            bool isISBNChanged = !existingBook.ISBN.Equals(book.ISBN, StringComparison.OrdinalIgnoreCase);

            var allBooks = await _bookRepository.GetAllBooks();

            if (isTitleChanged || isISBNChanged)
            {
                if (allBooks.Any(b =>
                    (isISBNChanged && b.ISBN.Equals(book.ISBN, StringComparison.OrdinalIgnoreCase)) ||
                    (isTitleChanged && b.title.Equals(book.title, StringComparison.OrdinalIgnoreCase)) &&
                    b.bookId != bookId))
                {
                    throw new InvalidOperationException("Duplicate ISBN or Title found.");
                }
            }

            existingBook.category = book.category;
            existingBook.title = book.title;
            existingBook.ISBN = book.ISBN;
            existingBook.publisher = book.publisher;
            existingBook.author = book.author;
            existingBook.description = book.description;
            existingBook.location = book.location;
            existingBook.price = book.price;
            existingBook.totalBook = book.totalBook;
            existingBook.language = book.language;

            return await _bookRepository.UpdateBook(existingBook);
        }


        public async Task<bool> DeleteBook(int bookId, string reason)
        {
            if (bookId <= 0)
            {
                throw new ArgumentException("Book ID must be greater than zero.");
            }

            if (string.IsNullOrEmpty(reason))
            {
                throw new ArgumentException("The reason should not be empty.", nameof(reason));
            }

            var existingBook = await _bookRepository.GetBookById(bookId);
            if (existingBook == null)
            {
                throw new KeyNotFoundException($"Book with ID {bookId} not found.");
            }

            existingBook.status = "Deleted";
            existingBook.reason = reason;

            return await _bookRepository.UpdateBook(existingBook);
        }

        public async Task<IEnumerable<ShowBookDto>> Search(SearchDto query, paginationDto pagination)
        {
            if (query == null)
            {
                throw new ArgumentException("Search criteria cannot be null.", nameof(query));
            }

            if (pagination.pageSize <= 0 || pagination.pageNumber <= 0)
            {
                throw new ArgumentException("Pagination parameters should be greater than zero.");
            }

            var allBooks = await _bookRepository.GetAllBooks();
            if (allBooks == null || !allBooks.Any())
            {
                return Enumerable.Empty<ShowBookDto>();
            }

            IEnumerable<Book> filteredBooks = allBooks;

            if (!string.IsNullOrEmpty(query.title))
            {
                filteredBooks = filteredBooks.Where(b => b.title != null && b.title.ToLower().Contains(query.title.ToLower()));
            }

            if (!string.IsNullOrEmpty(query.author))
            {
                filteredBooks = filteredBooks.Where(b => b.author != null && b.author.ToLower().Contains(query.author.ToLower()));
            }

            if (!string.IsNullOrEmpty(query.ISBN))
            {
                filteredBooks = filteredBooks.Where(b => b.ISBN != null && b.ISBN.ToLower().Contains(query.ISBN.ToLower()));
            }

            if (!string.IsNullOrEmpty(query.category))
            {
                filteredBooks = filteredBooks.Where(b => b.category != null && b.category.ToLower().Contains(query.category.ToLower()));
            }

            if (!string.IsNullOrEmpty(query.language))
            {
                filteredBooks = filteredBooks.Where(b => b.language != null && b.language.ToLower().Contains(query.language.ToLower()));
            }

            if (!string.IsNullOrEmpty(query.logicOperator) && query.logicOperator.Equals("OR", StringComparison.OrdinalIgnoreCase))
            {
                var orFilteredBooks = allBooks.AsEnumerable();

                if (!string.IsNullOrEmpty(query.title))
                {
                    orFilteredBooks = orFilteredBooks.Where(b => b.title != null && b.title.ToLower().Contains(query.title.ToLower()));
                }

                if (!string.IsNullOrEmpty(query.author))
                {
                    orFilteredBooks = orFilteredBooks.Where(b => b.author != null && b.author.ToLower().Contains(query.author.ToLower()));
                }

                if (!string.IsNullOrEmpty(query.ISBN))
                {
                    orFilteredBooks = orFilteredBooks.Where(b => b.ISBN != null && b.ISBN.ToLower().Contains(query.ISBN.ToLower()));
                }

                if (!string.IsNullOrEmpty(query.category))
                {
                    orFilteredBooks = orFilteredBooks.Where(b => b.category != null && b.category.ToLower().Contains(query.category.ToLower()));
                }

                if (!string.IsNullOrEmpty(query.language))
                {
                    orFilteredBooks = orFilteredBooks.Where(b => b.language != null && b.language.ToLower().Contains(query.language.ToLower()));
                }

                filteredBooks = orFilteredBooks;
            }

            var skipNumber = (pagination.pageNumber - 1) * pagination.pageSize;
            return filteredBooks
                .Skip(skipNumber)
                .Take(pagination.pageSize)
                .Select(b => new ShowBookDto
                {
                    bookId = b.bookId,
                    category = b.category,
                    title = b.title,
                    ISBN = b.ISBN,
                    author = b.author,
                    publisher = b.publisher,
                    description = b.description,
                    location = b.location,
                    totalBook = b.totalBook,
                    language = b.language
                })
                .OrderBy(b => b.title)
                .ToList();
        }


        public async Task<object> SearchBooksAsync(QueryObject query)
        {
            var allBooks = await _bookRepository.GetAllBooks();
            var temp = allBooks.AsQueryable();

            if (!string.IsNullOrEmpty(query.Keyword))
            {
                string keywordLower = query.Keyword.ToLower();
                temp = temp.Where(b => b.title.ToLower().Contains(keywordLower) ||
                                       b.author.ToLower().Contains(keywordLower) ||
                                       b.ISBN.ToLower().Contains(keywordLower) ||
                                       b.bookId.ToString().Contains(query.Keyword));
            }

            if (query.BookId.HasValue)
                temp = temp.Where(b => b.bookId == query.BookId.Value);

            if (!string.IsNullOrEmpty(query.Title))
                temp = temp.Where(b => b.title.ToLower().Contains(query.Title.ToLower()));

            if (!string.IsNullOrEmpty(query.Author))
                temp = temp.Where(b => b.author.ToLower().Contains(query.Author.ToLower()));

            if (!string.IsNullOrEmpty(query.ISBN))
                temp = temp.Where(b => b.ISBN.ToLower().Contains(query.ISBN.ToLower()));

            var total = temp.Count();

            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                switch (query.SortBy.ToLower())
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

            var skipNumber = (query.PageNumber - 1) * query.PageSize;

            var list = temp.Skip(skipNumber).Take(query.PageSize).ToList();

            return new { total = total, data = list };
        }

        public async Task<bool> RemoveBook(int bookId)
        {
            if(bookId <= 0)
            {
                throw new ArgumentException("Book ID must be grether than zero");
            }

            var existingBook = await _bookRepository.GetBookById(bookId);
            if(existingBook == null)
            {
                throw new ArgumentException("Book ID not found.");
            }

            return await _bookRepository.DeleteBook(bookId);
        }

    }
}
