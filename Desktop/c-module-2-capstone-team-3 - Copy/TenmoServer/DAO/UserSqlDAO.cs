using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using TenmoServer.Models;
using TenmoServer.Security;
using TenmoServer.Security.Models;

namespace TenmoServer.DAO
{
    /// <summary>
    /// the DAO for the <see cref="User"/> class, and it's subclasses: <see cref="ApiUser"/>, <see cref="ReturnUser"/>, and <see cref="LoginUser"/>
    /// </summary>
    /// <seealso cref="TenmoServer.DAO.IUserDAO" />
    public class UserSqlDAO : IUserDAO
    {

        private readonly string connectionString;
        /// <summary>
        /// The starting balance for new user's default account
        /// </summary>
        const decimal startingBalance = 1000;

        private const string SQL_GET_ALL_APIUSERS = "SELECT user_id, username, password_hash, salt FROM users";

        private const string SQL_GET_ALL_USERNAMES_AND_IDS = "SELECT user_id, username FROM users";

        /// <summary>
        /// Initializes a new instance of the <see cref="UserSqlDAO"/> class.
        /// </summary>
        /// <param name="dbConnectionString">The database connection string.</param>

        public UserSqlDAO(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }

        /// <summary>
        /// Gets an <see cref="ApiUser"/> object for a given username, or returns Null if no such user exists
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns>An <see cref="ApiUser"/> Object, or Null</returns>
        public ApiUser GetApiUser(string username)
        {
            ApiUser returnUser = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT user_id, username, password_hash, salt FROM users WHERE username = @username", conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows && reader.Read())
                    {
                        returnUser = GetApiUserFromReader(reader);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return returnUser;
        }
        //as much as possible we will use the distinction: api user is sensitive info, user is just username n id


        /// <summary>
        /// Gets a <see cref="List{T}"/> of all <see cref="ApiUser"/> from the server <br></br>
        /// <b><see cref="ApiUser"/>s CONTAIN SENSITIVE INFORMATION. USE THIS METHOD WITH CAUTION</b>
        /// </summary>
        public List<ApiUser> GetApiUsers()
        {
            List<ApiUser> returnUsers = new List<ApiUser>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(SQL_GET_ALL_APIUSERS, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            ApiUser u = GetApiUserFromReader(reader);
                            returnUsers.Add(u);
                        }

                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return returnUsers;
        }

        /// <summary>
        /// Gets a <see cref="List{T}"/> of <see cref="User"/>s on the server <br></br>
        /// <see cref="User"/> objects only have a <see cref="User.Username">Username</see> and a <see cref="User.UserId">UserId</see><br></br>
        /// Do not consume alcohol when invoking this method. Do not operate heavy machinery for 10 days after invoking this method. 
        /// If you are nursing, pregnant, or may become pregnant, as your doctor if this method is right for you as it can cause serious birth defects 
        /// including data loss, pissed off customers, and death
        /// </summary>
        public List<User> GetUsers() //for transfers yo
        {
            List<User> returnUsers = new List<User>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(SQL_GET_ALL_USERNAMES_AND_IDS, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            User u = new User()
                            {
                                UserId = Convert.ToInt32(reader["user_id"]),
                                Username = Convert.ToString(reader["username"]),
                            };

                            returnUsers.Add(u);
                        }

                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return returnUsers;
        }



        /// <summary>
        /// Gets a <see cref="User"/> object for a given UserId. Returns null if no such user exists
        /// </summary>
        /// <param name="userId">The userID</param>
        public User GetUser(int userId)
        {
            User returnUser = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT user_id, username FROM users where user_id = @userId", conn);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    SqlDataReader reader = cmd.ExecuteReader();


                    if (reader.Read())
                    {
                        returnUser = new User()
                        {
                            UserId = Convert.ToInt32(reader["user_id"]),
                            Username = Convert.ToString(reader["username"]),
                        };

                    }

                }
            }
            catch (SqlException)
            {
                throw;
            }

            return returnUser;
        }

        /// <summary>
        /// Adds a new <see cref="ApiUser"/> to the server
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>a <see cref="ApiUser"/> that was actually added to the db</returns>
        public ApiUser AddApiUser(string username, string password)
        {
            IPasswordHasher passwordHasher = new PasswordHasher();
            PasswordHash hash = passwordHasher.ComputeHash(password);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("INSERT INTO users (username, password_hash, salt) VALUES (@username, @password_hash, @salt)", conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password_hash", hash.Password);
                    cmd.Parameters.AddWithValue("@salt", hash.Salt);
                    cmd.ExecuteNonQuery();

                    cmd = new SqlCommand("SELECT @@IDENTITY", conn);
                    int userId = Convert.ToInt32(cmd.ExecuteScalar());

                    cmd = new SqlCommand("INSERT INTO accounts (user_id, balance,is_default_account) VALUES (@userid, @startBalance,1)", conn);
                    cmd.Parameters.AddWithValue("@userid", userId);
                    cmd.Parameters.AddWithValue("@startBalance", startingBalance);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return GetApiUser(username);
        }

        private ApiUser GetApiUserFromReader(SqlDataReader reader)
        {
            ApiUser u = new ApiUser()
            {
                UserId = Convert.ToInt32(reader["user_id"]),
                Username = Convert.ToString(reader["username"]),
                PasswordHash = Convert.ToString(reader["password_hash"]),
                Salt = Convert.ToString(reader["salt"]),
            };

            return u;
        }
    }
}
