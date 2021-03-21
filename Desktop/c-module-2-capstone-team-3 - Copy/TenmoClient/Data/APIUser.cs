namespace TenmoClient.Data
{


    /// <summary>
    /// Return value from login endpoint
    /// </summary>
    public class APIUser : User
    {
        public string Token { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// Model to provide login parameters
    /// </summary>
    public class LoginUser
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
