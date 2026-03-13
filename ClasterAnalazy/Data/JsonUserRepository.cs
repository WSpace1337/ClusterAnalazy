using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Linq;
using System;

using ClusterVisualizer.Core.Models;

public class JsonUserRepository
{
    // Используем Path.Combine для более надежного пути
    private readonly string path = "Data/users.json";

    public List<User> GetUsers()
    {
        if (!File.Exists(path))
        {
            return new List<User>();
        }

        try
        {
            string json = File.ReadAllText(path);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<User>();
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Позволяет читать "username" как "Username"
            };

            return JsonSerializer.Deserialize<List<User>>(json, options) ?? new List<User>();
        }
        catch (JsonException)
        {
            return new List<User>();
        }
    }

    public User GetByUsername(string username)
    {
        var users = GetUsers();

        return users.FirstOrDefault(u =>
            u.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}