using Asp.Versioning;
using Assignment5.Application.DTOs;
using Assignment5.Application.Interfaces.IService;
using Assignment5.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Assignment5.WebAPI.Controllers
{
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[Controller]")]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        /// <summary>
        /// Adds a new book to the system.
        /// </summary>
        /// <remarks>
        /// Ensure that the book data is not null and that the book details are valid.
        /// Validate that no book with the same ISBN or Title already exists.
        /// </remarks>
        /// <param name="book">The book to be added.</param>
        /// <returns>Result of the operation.</returns>
        [HttpPost]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> AddBook([FromBody] Book book)
        {
            if (book == null)
            {
                return BadRequest("Invalid input data. Please check the book details.");
            }

            try
            {
                var addedBook = await _bookService.AddBook(book);
                return Ok(new { Message = "Book added successfully.", Book = addedBook });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves a list of all books in the system with pagination.
        /// </summary>
        /// <param name="pagination">Pagination details.</param>
        /// <returns>A list of books.</returns>
        [HttpGet]
        [MapToApiVersion("1.0")]
        public async Task<ActionResult<IEnumerable<ShowBookDto>>> GetAllBooks([FromQuery] paginationDto pagination)
        {
            try
            {
                var books = await _bookService.GetAllBooks(pagination);
                return Ok(books);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves a list of all books in the system without pagination.
        /// </summary>
        /// <returns>A list of books.</returns>
        [HttpGet("noPages")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> GetAllBooksNoPages()
        {
            try
            {
                var books = await _bookService.GetAllBooksNoPages();
                return Ok(books);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves a book by its ID.
        /// </summary>
        /// <param name="bookId">The ID of the book to be retrieved.</param>
        /// <returns>Book details if found, otherwise an error message.</returns>
        [HttpGet("{bookId:int}")]
        [MapToApiVersion("1.0")]
        public async Task<ActionResult<ShowBookDto>> GetBookById(int bookId)
        {
            if (bookId <= 0)
            {
                return BadRequest("Invalid ID. The ID must be greater than zero.");
            }

            try
            {
                var book = await _bookService.GetBookById(bookId);
                if (book == null)
                {
                    return NotFound($"Book with ID {bookId} was not found.");
                }
                return Ok(book);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing book by its ID.
        /// </summary>
        /// <param name="bookId">The ID of the book to be updated.</param>
        /// <param name="book">The updated book details.</param>
        /// <returns>Result of the update operation.</returns>
        [HttpPut("{bookId:int}")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> UpdateBook(int bookId, [FromBody] Book book)
        {
            if (book == null)
            {
                return BadRequest("Invalid input data. Please check the book details.");
            }

            try
            {
                var success = await _bookService.UpdateBook(bookId, book);
                if (!success)
                {
                    return BadRequest("Unable to update book. Title or ISBN might already exist or the book may not be found.");
                }
                return Ok("Book updated successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a book by its ID.
        /// </summary>
        /// <param name="bookId">The ID of the book to be deleted.</param>
        /// <param name="reason">The reason for deletion.</param>
        /// <returns>Result of the delete operation.</returns>
        [HttpDelete("{bookId:int}")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> DeleteBook(int bookId, [FromBody] string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                return BadRequest("The reason should not be empty.");
            }

            try
            {
                var success = await _bookService.DeleteBook(bookId, reason);
                if (!success)
                {
                    return NotFound("Book not found.");
                }
                return Ok("Book deleted successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Searches for books based on the provided search criteria and pagination.
        /// </summary>
        /// <param name="query">Search criteria including title, author, ISBN, and category. These parameters are optional.</param>
        /// <param name="pagination">Pagination details including page number and page size.</param>
        /// <returns>A list of books that match the search criteria.</returns>
        [HttpGet("search")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> Search([FromQuery] SearchDto query, [FromQuery] paginationDto pagination)
        {
            try
            {
                var searchResults = await _bookService.Search(query, pagination);
                return Ok(searchResults);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }

}
