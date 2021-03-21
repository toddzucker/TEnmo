using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serialization.Json;
using System;
using System.Collections.Generic;
using System.IO;
using TenmoClient.Data;


namespace TenmoClient
{
    /// <summary>
    /// all bad things get printed here
    /// </summary>
    public class AuthService
    {
        private static readonly string API_BASE_URL = "https://localhost:44315/";
        private readonly IRestClient client = new RestClient();

        public AuthService()
        {

        }

        //login endpoints
        public bool Register(LoginUser registerUser)
        {
            bool successfulRegistration = false;
            RestRequest request = new RestRequest(API_BASE_URL + "login/register");
            request.AddJsonBody(registerUser);
            IRestResponse<APIUser> response = client.Post<APIUser>(request);
            (bool, string) status = checkResponse(response);
            if (status.Item1)
            {
                successfulRegistration = true;
            }
            else
            {
                Console.WriteLine(status.Item2);
            }
            return successfulRegistration;
        }

        public APIUser Login(LoginUser loginUser)
        {
            APIUser returnUser = null;
            RestRequest request = new RestRequest(API_BASE_URL + "login");
            request.AddJsonBody(loginUser);
            IRestResponse<APIUser> response = client.Post<APIUser>(request);
            (bool, string) status = checkResponse(response);
            if (status.Item1)
            {
                /*this is where we get the JWT token. It gets added to the RestClient. 
                * Now, all future HTTP requests will include the JWT token.
                * The JWT token allows us to authenticate our HTTP request 
                * AND includes the user ID along with other stuff*/
                client.Authenticator = new JwtAuthenticator(response.Data.Token);
                returnUser = response.Data;
            }
            else
            {
                Console.WriteLine(status.Item2);
            }
            return returnUser;
        }



        /// <summary>
        /// A list of all the users. The User objects only contain the UserIDs and UserNames
        /// </summary>
        /// <returns></returns>
        public List<User> GetUsers()
        {
            List<User> users = null;
            RestRequest request = new RestRequest(API_BASE_URL + "users");
            IRestResponse<List<User>> response = client.Get<List<User>>(request);
            (bool, string) status = checkResponse(response);
            if (status.Item1)
            {
                users = response.Data;
            }
            else
            {
                Console.WriteLine(status.Item2);
            }
            return users;
        }

        public int? GetDefaultAccountIdFromUserId(int userId)
        {
            int? result = null;
            RestRequest request = new RestRequest(API_BASE_URL + $"{userId}/default_account");
            IRestResponse<int?> response = client.Get<int?>(request);
            (bool, string) status = checkResponse(response);
            if (status.Item1)
            {
                result = response.Data;
            }
            else
            {
                Console.WriteLine(status.Item2);
            }
            return result;
        }

        /// <summary>
        /// Gets a list of all accounts that belong to the logged-in user.
        /// Currently, we expect all users to have exactly 1 account, but we 
        /// may want to add functionality for multiple accounts per user (i.e. Checking, savings, etc)
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">
        /// Error occurred - unable to reach server.
        /// or
        /// Error{(int)response.StatusCode}: Something went wrong
        /// </exception>
        public List<Account> GetAccountsForCurrentUser()
        {
            //the thing we will return to the user
            List<Account> returnList = null;

            RestRequest request = new RestRequest(API_BASE_URL + "accounts");
            IRestResponse<List<Account>> response = client.Get<List<Account>>(request);
            (bool, string) status = checkResponse(response);
            if (status.Item1)
            {
                returnList = response.Data;
            }
            else
            {
                Console.WriteLine(status.Item2);
            }
            return returnList;

        }
        public List<Transfer> GetTransfersForCurrentUser()
        {
            List<Transfer> returnList = null;

            RestRequest request = new RestRequest(API_BASE_URL + "transfers");
            IRestResponse<List<Transfer>> response = client.Get<List<Transfer>>(request);
            (bool, string) status = checkResponse(response);
            if (status.Item1)
            {
                returnList = response.Data;
            }
            else
            {
                Console.WriteLine(status.Item2);
            }
            return returnList;
        }
        /// <summary>
        /// Gets the public information for an <see cref="Account"/>.
        /// </summary>
        /// <param name="accountId">The AccountID</param>
        /// <returns>a tuple of type(<see cref="User"/> (the owner of the account) , <see cref="Account"/>(the account itself))</returns>
        public (User, Account) GetAccountPublicInfo(int accountId)
        {
            (User, Account) result = (null, null);
            RestRequest request = new RestRequest(API_BASE_URL + $"accounts/{accountId}/public_info");
            IRestResponse<List<object>> response = client.Get<List<object>>(request);
            (bool, string) status = checkResponse(response);
            if (status.Item1)
            {
                //User user = (User)response.Data[0];
                Dictionary<string, object> userDict = (Dictionary<string, object>)response.Data[0];
                Dictionary<string, object> accountDict = (Dictionary<string, object>)response.Data[1];

                User user = new User()
                {
                    UserId = Convert.ToInt32(userDict["userId"]),
                    Username = Convert.ToString(userDict["username"])
                };
                Account account = new Account()
                {
                    AccountId = Convert.ToInt32(accountDict["accountId"]),
                    UserId = Convert.ToInt32(accountDict["userId"])
                };
                result = (user, account);
            }
            else
            {
                Console.WriteLine(status.Item2);
            }
            return result;
        }


        public Transfer CreateTransfer(Transfer transferToCreate)
        {
            Transfer transfer = null;
            RestRequest request = new RestRequest(API_BASE_URL + "transfers");
            request.AddJsonBody(transferToCreate);
            IRestResponse<Transfer> response = client.Post<Transfer>(request);
            //RestRequest request = new RestRequest(API_BASE_URL + "login");
            //request.AddJsonBody(loginUser);
            //IRestResponse<APIUser> response = client.Post<APIUser>(request);

            (bool, string) status = checkResponse(response);
            if (status.Item1)
            {
                transfer = response.Data;
            }
            else
            {
                Console.WriteLine(status.Item2);
            }
            return transfer;


        }
        public bool RespondToTransfer(Transfer transferToUpdate, bool isAccepted)
        {
            bool success = false;
            transferToUpdate.TransferStatus = isAccepted ? TransferStatus.Approved : TransferStatus.Rejected;
            RestRequest request = new RestRequest(API_BASE_URL + $"transfers/{transferToUpdate.TransferId}");
            request.AddJsonBody(transferToUpdate);
            IRestResponse response = client.Put(request);
            (bool, string) status = checkResponse(response);
            if (status.Item1)
            {
                success = true;
            }
            else
            {
                Console.WriteLine(status.Item2);
            }
            return success;
        }

        public (bool, string) checkResponse(IRestResponse response)
        {
            if (response.ResponseStatus != ResponseStatus.Completed && response.ResponseStatus != ResponseStatus.Error)
            {
                return (false, "An error occurred communicating with the server.");
            }
            if (!response.IsSuccessful)
            {
                string errorMessage = response.Content;
                if (errorMessage == null)
                {
                    errorMessage = "";
                }
                else if (errorMessage.StartsWith("\"") && errorMessage.EndsWith("\""))
                {
                    errorMessage = errorMessage.Substring(1, errorMessage.Length - 2);//stripping ugly quotes from error messages
                }
                //return (false, $"Error {(int)response.StatusCode}: {errorMessage}");
                return (false, $"Error: {errorMessage}");
            }
            else
            {
                return (true, "all okay!");
            }
        }





    }
}
