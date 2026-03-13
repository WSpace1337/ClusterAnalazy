using System.Linq;
using System;
using System.Collections.Generic;
using ClusterVisualizer.Core.Models;
using System.Windows.Documents;
using ClusterAnalazy.Services;
using ClusterVisualizer.ViewModels;

namespace ClusterVisualizer.Services { 
public class AuthenticationService
    {
        private List<User> users;

        public AuthenticationService()
        {
            var userService = new UserService();
            users = userService.LoadUser();
        }

        public User Login(string username, string password)
        {
            return users.FirstOrDefault(u =>
            u.Username == username &&
            u.Password == password);
        }
    }
}