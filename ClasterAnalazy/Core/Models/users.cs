namespace ClusterVisualizer.Core.Models {
    public class User
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public UserRole Role { get; set; }

        public enum UserRole
        {
            Admin,
            Analyst
        }
    }
}