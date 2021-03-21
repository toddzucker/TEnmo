using TenmoClient.Data;

namespace TenmoClient
{
    public static class UserService
    {
        private static APIUser user = new APIUser();

        public static void SetLogin(APIUser u)
        {
            user = u;
        }

        public static string GetUserName()
        {
            return user.Username;
        }

        public static int GetUserId()
        {
            return user.UserId;
        }

        public static bool IsLoggedIn()
        {
            return !string.IsNullOrWhiteSpace(user.Token);
        }

        public static string GetToken()
        {
            return user?.Token ?? string.Empty;
        }
    }
}
