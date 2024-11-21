using Assignment5.Application.DTOs;
using Assignment5.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment5.Application.Interfaces.IService
{
    public interface IBookService
    {
        Task<Book> AddBook(Book book);
        Task<IEnumerable<ShowBookDto>> GetAllBooks(paginationDto pagination);
        Task<IEnumerable<Book>> GetAllBooksNoPages();
        Task<ShowBookDto> GetBookById(int bookId);
        Task<bool> UpdateBook(int bookId, Book book);
        Task<bool> DeleteBook(int bookId, string reason);
        Task<IEnumerable<ShowBookDto>> Search(SearchDto query, paginationDto pagination);
        Task<object> SearchBooksAsync(QueryObject query);
        Task<bool> RemoveBook(int bookId);
    }
}
