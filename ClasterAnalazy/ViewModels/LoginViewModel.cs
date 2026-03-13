
using ClusterVisualizer.Services;
namespace ClusterVisualizer.ViewModels
{
    public class LoginViewModel
    {
        private readonly AuthenticationService authenticationService;

        public string Username { get; set; }
        public string Password { get; set; }

        public LoginViewModel()
        {
            authenticationService = new AuthenticationService();
        }

        public bool Login()
        {
            var user = authenticationService.Login(Username, Password);
            if (user == null)
            {
                return false;
            }
            SessionManager.CurrentUser = user;

            return true;
        }
    }
}