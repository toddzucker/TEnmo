using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using TenmoServer.DAO;
using TenmoServer.Models;

namespace TenmoServerTests.DAO
{
    /// <summary>
    /// Test class for testing the <see cref="UserSqlDAO"/> class's methods
    /// </summary>
    /// <seealso cref="TenmoServerTests.DAO.DAOTests" />
    [TestClass]
    public class UserSqlDAOTests : DAOTests
    {
        /// <summary>
        /// The <see cref="UserSqlDAO"/> object to use in all the tests
        /// </summary>
        UserSqlDAO dao;


        [TestInitialize]
        public override void Setup()
        {
            base.Setup();
            dao = new UserSqlDAO(connectionString);
        }

        /// <summary>
        /// tests the <see cref="UserSqlDAO.GetUsers"/> method
        /// </summary>
        [TestMethod]
        public void GetUsers_test()
        {
            //arange

            //act
            List<User> actualUsers = dao.GetUsers();

            //assert
            Assert.IsNotNull(actualUsers);
            Assert.AreEqual(5, actualUsers.Count, "The number of users we got back was not the expected number!");

        }

        /// <summary>
        /// Tests the <see cref="UserSqlDAO.AddApiUser(string, string)"/> method
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        [DataTestMethod]
        [DataRow("Spongebob", "gary1")]
        [DataRow("Mr.Krabs", "money")]
        public void AddApiUser_Test(string username, string password)
        {
            //arrange

            //act
            ApiUser actualUser = dao.AddApiUser(username, password);

            //assert
            Assert.IsNotNull(actualUser);
            Assert.AreEqual(username, actualUser.Username);
        }

        /// <summary>
        /// Tests the <see cref="UserSqlDAO.GetApiUsers"/> method
        /// </summary>
        [TestMethod]
        public void GetApiUsers_test()
        {
            //arrange

            //act
            List<ApiUser> actualApiUsers = dao.GetApiUsers();

            //assert
            Assert.IsNotNull(actualApiUsers);
            Assert.AreEqual(5, actualApiUsers.Count, "The number of users we got back was not the expected number!");

        }
        /// <summary>
        /// Tests the <see cref="UserSqlDAO.GetApiUser(string)"/> method
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="expectExists">Whether or not we expect the user to exist in the database</param>

        [DataTestMethod]
        [DataRow("Paul", true)]
        [DataRow("Todd", true)]
        [DataRow("Mike", true)]
        [DataRow("Sir Not-Appearing-In-This-Film", false)]
        public void GetApiUser_test(string username, bool expectExists)
        {
            //arrange

            //act
            ApiUser actualUser = dao.GetApiUser(username);

            //assert
            if (expectExists)
            {
                Assert.IsNotNull(actualUser);
                Assert.AreEqual(username, actualUser.Username);
            }
            else
            {
                Assert.IsNull(actualUser);
            }
        }

        /// <summary>
        /// tests the <see cref="UserSqlDAO.GetUser(int)" method/>
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="expectExists">Whether or not we expect the user to exist in the database</param>
        [DataTestMethod]
        [DataRow(1, true)]
        [DataRow(2, true)]
        [DataRow(3, true)]
        [DataRow(9, false)]
        public void GetUser_test(int userId, bool expectExists)
        {
            //arrange

            //act
            User actualUser = dao.GetUser(userId);

            //assert
            if (expectExists)
            {
                Assert.IsNotNull(actualUser);
                Assert.AreEqual(userId, actualUser.UserId);
            }
            else
            {
                Assert.IsNull(actualUser);
            }
        }



    }
}
