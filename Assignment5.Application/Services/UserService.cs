using Assignment5.Application.DTOs;
using Assignment5.Application.Interfaces.IRepositories;
using Assignment5.Application.Interfaces.IService;
using Assignment5.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Assignment5.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> AddUser(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("User data cannot be null");
            }
            var existingUser = await _userRepository.GetAllUsers();
            if (existingUser.Any(u => u.libraryCardNumber.Equals(user.libraryCardNumber, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"User with library card number {user.libraryCardNumber} already exists.");
            }
            return await _userRepository.AddUser(user);
        }

        public async Task<object> GetAllUsers(paginationDto pagination)
        {
            if (pagination.pageNumber <= 0 || pagination.pageSize <= 0)
            {
                throw new ArgumentException("Page number and page size must be greater than zero.");
            }

            var users = await _userRepository.GetAllUsers();
            var temp = users.AsQueryable();
            var total = temp.Count();

            var skipNumber = (pagination.pageNumber - 1) * pagination.pageSize;
            var usersList =  temp.Skip(skipNumber).Take(pagination.pageSize).ToList();

            return new { total = total, data = usersList };
        }

        public async Task<IEnumerable<User>> GetAllUsersNoPages()
        {
            var users = await _userRepository.GetAllUsers();
            return users;
        }

        public async Task<User> GetUserById(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("User ID must be greater than zero.");
            }

            var existingUser = await _userRepository.GetUserById(userId);
            if (existingUser == null)
            {
                throw new ArgumentException("User ID not found.");
            }
            return await _userRepository.GetUserById(userId);
        }

        public async Task<bool> UpdateUser(int userId, User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("User cannot be null.");
            }

            if (userId <= 0)
            {
                throw new ArgumentException("User ID must be greater than zero.");
            }

            var existingUser = await _userRepository.GetUserById(userId);
            if (existingUser == null)
            {
                throw new ArgumentException("User ID not found.");
            }

            existingUser.firstName = user.firstName;
            existingUser.lastName = user.lastName;
            existingUser.position = user.position;
            existingUser.libraryCardNumber = user.libraryCardNumber;
            existingUser.privilage = user.privilage;
            existingUser.notes = user.notes;

            return await _userRepository.UpdateUser(existingUser);
        }

        public async Task<bool> DeleteUser(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("User ID must be greater than zero.");
            }

            var existingUser = await _userRepository.GetUserById(userId);
            if (existingUser == null)
            {
                throw new ArgumentException("User ID not found.");
            }

            return await _userRepository.DeleteUser(userId);
        }
    }

}
