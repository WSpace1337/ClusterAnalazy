
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
            return authenticationService.Login(Username, Password); 
        }
    }
}