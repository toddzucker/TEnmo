using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public class AccountSqlDAO : IAccountDAO
    {
        private const string SQL_SELECT_ACCOUNT_BY_ID = "select * from accounts where account_id = @accountId";
        private const string SQL_SELECT_ACCOUNTS_BY_USERID = "select * from accounts where user_id = @userId";
        private const string SQL_DEDUCT_FROM_ACCOUNT = "update accounts set balance = balance - @amount where account_id = @accountId";
        private const string SQL_DEPOSIT_INTO_ACCOUNT = "update accounts set balance = balance + @amount where account_id = @accountId";
        private const string SQL_GET_DEFAULT_ACCOUNT_BY_USERID = "select account_id from accounts where user_id = @userId AND is_default_account = 1";

        private readonly string connectionString;

        public AccountSqlDAO(string connectionString)
        {
            this.connectionString = connectionString;
        }



        /// <summary>
        /// Gets a list of all accounts belonging to a given user.
        /// Currently, we expect all users to have exactly 1 account, so this should normally return a list of length 1.
        /// However, the database design *suggests* that there could be multiple accounts per user in the future,
        /// so we will build in that functionality from the start
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve</param>
        public List<Account> GetAccountsByUserId(int userId)
        {
            List<Account> result = new List<Account>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(SQL_SELECT_ACCOUNTS_BY_USERID, conn);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        Account a = RowToObject(reader);
                        result.Add(a);
                    }
                }
                return result;
            }
            catch (SqlException ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Given a userID, returns the ID of that user's default account.
        /// Returns null if no default account was found for the user, or if no user exists
        /// </summary>
        /// <param name="userId">the id of the user</param>
        /// <returns>the ID of the default account.</returns>
        public int? GetDefaultAccountIdForUserId(int userId)
        {

            int? result = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(SQL_GET_DEFAULT_ACCOUNT_BY_USERID, conn);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        result = Convert.ToInt32(reader["account_id"]);
                    }
                }
                return result;
            }
            catch (SqlException ex)
            {
                throw;
            }
        }


        /// <summary>
        /// Gets an Account in the DB for a given ID. If no such account exists, null is returned.
        /// </summary>
        /// <param name="accountId">The ID of the account to retrieve</param>
        /// <returns></returns>
        public Account GetAccountById(int accountId)
        {
            try
            {
                Account result = null;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(SQL_SELECT_ACCOUNT_BY_ID, conn);
                    cmd.Parameters.AddWithValue("@accountId", accountId);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        result = RowToObject(reader);
                    }
                }
                return result;
            }
            catch (SqlException ex)
            {
                throw;
            }

        }



        /// <summary>
        /// Deducts a given amount of money from an account. If no such account exists, return null
        /// </summary>
        /// <param name="accountId">the ID of the account to remove money from</param>
        /// <param name="amount">The amount to deduct</param>
        /// <returns>the account with the updated balance</returns>
        public Account DeductFromAccount(int accountId, decimal amount)
        {
            /* This method is basically identical to DepositIntoAccount.
             * We decided to make two separate (slightly smelly) methods for deduct and deposit to make sure nobody ever gets them mixed up.
             * Better safe than sorry when dealing with customer's money!
             */
            try
            {
                Account result = null;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(SQL_DEDUCT_FROM_ACCOUNT, conn);
                    cmd.Parameters.AddWithValue("@accountId", accountId);
                    cmd.Parameters.AddWithValue("@amount", amount);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        result = GetAccountById(accountId);
                    }

                }
                return result;
            }
            catch (SqlException)
            {
                throw;
            }
        }

        /// <summary>
        /// deposits a given amount of money into an account. If no such account exists, return null
        /// </summary>
        /// <param name="accountId">the ID of the account to add money to</param>
        /// <param name="amount">The amount to deposit</param>
        /// <returns>the account with the updated balance</returns>
        public Account DepositIntoAccount(int accountId, decimal amount)
        {
            try
            {
                Account result = null;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(SQL_DEPOSIT_INTO_ACCOUNT, conn);
                    cmd.Parameters.AddWithValue("@accountId", accountId);
                    cmd.Parameters.AddWithValue("@amount", amount);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        result = GetAccountById(accountId);
                    }

                }
                return result;
            }
            catch (SqlException)
            {
                throw;
            }
        }
        private Account RowToObject(SqlDataReader reader)
        {
            Account account = new Account()
            {
                AccountId = Convert.ToInt32(reader["account_id"]),
                UserId = Convert.ToInt32(reader["user_id"]),
                Balance = Convert.ToDecimal(reader["balance"])
            };
            return account;
        }
    }
}
