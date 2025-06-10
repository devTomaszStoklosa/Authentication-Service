using AuthService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using BCrypt.Net;

namespace AuthService.Services
{
    public class UserService
    {
        private readonly List<User> _users = new();

        public IEnumerable<User> GetAll() => _users;

        public User GetById(string id) => _users.FirstOrDefault(u => u.Id == id);

        public User Create(User user)
        {
            user.Id = Guid.NewGuid().ToString();
            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.PasswordHash))
            {
                throw new ArgumentException("Email and Password cannot be empty.");
            }
            if (_users.Any(u => u.Email == user.Email))
            {
                throw new InvalidOperationException("User with this email already exists.");
            }
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            _users.Add(user);

            return user;
        }

        public bool Update(string id, User user)
        {
            var existing = GetById(id);
            if (existing == null) return false;
            existing.Email = user.Email;
            existing.PasswordHash = user.PasswordHash;

            return true;
        }

        public bool Delete(string id)
        {
            var user = GetById(id);
            if (user == null) return false;
            _users.Remove(user);
            return true;
        }
    }
}