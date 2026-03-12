using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Linq;
using System;

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

            // КРИТИЧЕСКОЕ ИСПРАВЛЕНИЕ: Добавляем опции десериализации
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Позволяет читать "username" как "Username"
            };

            return JsonSerializer.Deserialize<List<User>>(json, options) ?? new List<User>();
        }
        catch (JsonException)
        {
            // Если JSON файл поврежден или имеет неверный формат
            return new List<User>();
        }
    }

    public User GetByUsername(string username)
    {
        var users = GetUsers();

        // Добавляем .Trim() и StringComparison, чтобы поиск не зависел от пробелов и регистра
        return users.FirstOrDefault(u =>
            u.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}