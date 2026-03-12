using System.Linq;
using System;

public class AuthenticationService
{
        private readonly JsonUserRepository repository;

        public AuthenticationService()
        {
            repository = new JsonUserRepository();
        }

    public bool Login(string username, string password)
    {
        var users = repository.GetUsers();

        var user = users.FirstOrDefault(u => u.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase));

        if (user == null) return false;

        return user.Password == password;
    }
}