using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

using ClusterVisualizer.Core.Models;
using System.Text.Json.Serialization;


namespace ClusterAnalazy.Services
{
    public class UserService
    {
        public List<User> LoadUser()
        {
            string path = "Data/users.json";

            if(!File.Exists(path))
            {
                return new List<User>();
            }

            string json = File.ReadAllText(path);

            var options = new JsonSerializerOptions{
                Converters = { new JsonStringEnumConverter() },
                PropertyNameCaseInsensitive = true
            }; 
                return JsonSerializer.Deserialize<List<User>>(json, options);
        }
    }
}
