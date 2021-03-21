using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using TenmoServer.DAO;
using TenmoServer.Models;

namespace TenmoServerTests.DAO
{
    /// <summary>
    /// Test class for the <see cref="AccountSqlDAO"/> class
    /// </summary>
    /// <seealso cref="TenmoServerTests.DAO.DAOTests" />
    [TestClass]
    public class AccountSqlDAOTests : DAOTests
    {
        /// <summary>
        /// The <see cref="AccountSqlDAO"/> object to be used in all the tests
        /// </summary>
        AccountSqlDAO dao;

        /// <summary>
        /// Sets up this instance of <see cref="DAOTests" />.
        /// This should have the <see cref="TestInitializeAttribute">TestInitialize</see> attribute on all subclass implementations <br></br>
        /// also, the <see cref="ResetDB" /> method should be called each time this method is called.
        /// </summary>
        [TestInitialize]
        public override void Setup()
        {
            base.Setup();
            dao = new AccountSqlDAO(connectionString);
        }

        /// <summary>
        /// Tests the <see cref="AccountSqlDAO.GetAccountsByUserId(int)"/> Method. 
        /// </summary>
        /// <param name="userId">the UserID </param>
        /// <param name="numberOfAccounts">The number of accounts we expect to be owned by that user</param>
        [DataTestMethod]
        [DataRow(1, 1)]//todd
        [DataRow(2, 2)]//paul
        [DataRow(3, 1)]//mike
        //deduct, deposit

        public void GetAccountsByUserId_Test(int userId, int numberOfAccounts)
        {
            //arrange

            //act
            List<Account> actualAccounts = dao.GetAccountsByUserId(userId);


            //assert
            Assert.AreEqual(numberOfAccounts, actualAccounts.Count);
        }
        /// <summary>
        /// Tests the <see cref="AccountSqlDAO.GetDefaultAccountIdForUserId(int)"/> 
        /// </summary>
        /// <param name="userId">The UserId</param>
        /// <param name="expectedDefaultAccountId">the expected AccountID of that user's default account</param>
        [DataTestMethod]
        [DataRow(1, 1)]
        [DataRow(2, 2)]
        [DataRow(3, 4)]

        public void GetDefaultAccountByUserId_Test(int userId, int expectedDefaultAccountId)
        {
            int? actualDefaultAccountId = dao.GetDefaultAccountIdForUserId(userId);
            Assert.AreEqual(expectedDefaultAccountId, actualDefaultAccountId);
        }
        /// <summary>
        /// Tests the <see cref="AccountSqlDAO.GetAccountById(int)"/> Method
        /// </summary>
        /// <param name="accountId">the AccountID</param>
        /// <param name="expectedBalance">The expected balance of that account</param>
        [DataTestMethod]
        [DataRow(1, 1000)]
        [DataRow(2, 1000)]
        [DataRow(3, 2000)]
        [DataRow(4, 1000)]
        [DataRow(5, 775)]
        [DataRow(6, 1225)]
        public void GetAccountById_Test(int accountId, double expectedBalance)
        {
            Account actualAccount = dao.GetAccountById(accountId);
            Assert.AreEqual((decimal)expectedBalance, actualAccount.Balance);
            Assert.AreEqual(accountId, actualAccount.AccountId);
        }
        /// <summary>
        /// Tests the <see cref="AccountSqlDAO.DeductFromAccount(int, decimal)"/> Method.
        /// Does two deductions back-to-back and tests the account for an expected final balance
        /// </summary>
        /// <param name="accountId">the accountID</param>
        /// <param name="deduction1">first deduction</param>
        /// <param name="deduction2">second deduction</param>
        /// <param name="expectedFinalBalance">The expected final balance.</param>
        [DataTestMethod]
        [DataRow(1, 5, 200, 795)]
        [DataRow(1, 20, 80, 900)]
        [DataRow(1, 300, 380, 320)]

        public void DeductFromAccount_Test(int accountId, double deduction1, double deduction2, double expectedFinalBalance)
        {
            dao.DeductFromAccount(accountId, (decimal)deduction1);
            dao.DeductFromAccount(accountId, (decimal)deduction2);
            Account account = dao.GetAccountById(accountId);
            Assert.AreEqual((decimal)expectedFinalBalance, account.Balance);
        }

        /// <summary>
        /// Tests the <see cref="AccountSqlDAO.DepositIntoAccount(int, decimal)"/> Method.
        /// Does two deposits back-to-back and tests the account for an expected final balance
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="deposit1">First Deposit deposit1.</param>
        /// <param name="deposit2">Second Deposit deposit2.</param>
        /// <param name="expectedFinalBalance">The expected final balance.</param>
        [DataTestMethod]
        [DataRow(1, 5, 200, 1205)]
        [DataRow(1, 20, 80, 1100)]
        [DataRow(1, 300, 380, 1680)]

        public void DepositIntoAccount_Test(int accountId, double deposit1, double deposit2, double expectedFinalBalance)
        {
            dao.DepositIntoAccount(accountId, (decimal)deposit1);
            dao.DepositIntoAccount(accountId, (decimal)deposit2);
            Account account = dao.GetAccountById(accountId);
            Assert.AreEqual((decimal)expectedFinalBalance, account.Balance);
        }
    }
}
