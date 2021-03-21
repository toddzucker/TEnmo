using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using TenmoServer.DAO;
using TenmoServer.Models;

namespace TenmoServerTests.DAO
{
    /// <summary>
    /// Testing class for the <see cref="TransferSqlDAO"/> class's methods
    /// </summary>
    /// <seealso cref="TenmoServerTests.DAO.DAOTests" />
    [TestClass]
    public class TransferSqlDAOTests : DAOTests
    {
        /// <summary>
        /// The <see cref="TransferSqlDAO"/> object to be used in all the tests
        /// </summary>
        TransferSqlDAO dao;

        [TestInitialize]
        public override void Setup()
        {
            base.Setup();
            dao = new TransferSqlDAO(connectionString);
        }
        //updateTransfer

        /// <summary>
        /// Tests the TransferSqlDAO.getTransferById() method
        /// </summary>
        /// <param name="transferId">the ID of the transfer</param>
        /// <param name="expectedAmount">The expected amount of money of the transfer</param>
        [DataTestMethod]
        [DataRow(1,150)]
        [DataRow(2, 100)]
        [DataRow(3, 25)]
        public void GetTransferByID_test(int transferId,double expectedAmount)
        {
            //arrange

            //act
            Transfer actualTransfer = dao.GetTransferById(transferId);

            //assert
            Assert.IsNotNull(actualTransfer);
            Assert.AreEqual((decimal)expectedAmount, actualTransfer.Amount,"The expected amount in the transfer is incorrect. This probably means the wrong transfer was returned");
        }

        /// <summary>
        /// Tests the TransferSqlDAO.CreateTransfer() method
        /// </summary>
        /// <param name="senderUsername">The sender username.</param>
        /// <param name="receiverUsername">The receiver username.</param>
        /// <param name="transferType">Type of the transfer.</param>
        /// <param name="amount">The amount.</param>
        [DataTestMethod]
        [DataRow("Val","Ben", TransferType.Request,50)]
        [DataRow("Todd", "Paul", TransferType.Send, 500)]
        public void CreateTransfer_test(string senderUsername, string receiverUsername, int transferType, double amount)
        {
            //todo this should be in the TransferController test. If we knew how to test controllers that is...
            //arrange
            AccountSqlDAO accountDao = new AccountSqlDAO(connectionString);
            UserSqlDAO userDao = new UserSqlDAO(connectionString);
            int senderAccountId = (int)accountDao.GetDefaultAccountIdForUserId(userDao.GetApiUser(senderUsername).UserId);
            int receiverAccountId = (int)accountDao.GetDefaultAccountIdForUserId(userDao.GetApiUser(receiverUsername).UserId);
            TransferStatus transferStatus = transferType == (int)TransferType.Send ? TransferStatus.Approved : TransferStatus.Pending;
            Transfer transferToCreate = new Transfer()
            {
                AccountFromId = senderAccountId,
                AccountToId = receiverAccountId,
                TransferType = (TransferType)transferType,
                TransferStatus = transferStatus,
                Amount = (decimal)amount
            };


            //act
            Transfer actualTransfer = dao.CreateTransfer(transferToCreate);

            //assert
            Assert.IsNotNull(actualTransfer);
            Assert.AreEqual(transferToCreate.AccountFromId, actualTransfer.AccountFromId);
            Assert.AreEqual(senderUsername, actualTransfer.UsernameFrom);
            Assert.AreEqual(transferToCreate.AccountToId, actualTransfer.AccountToId);
            Assert.AreEqual(receiverUsername, actualTransfer.UsernameTo);
            Assert.AreEqual(transferToCreate.TransferType, actualTransfer.TransferType);
            Assert.AreEqual(transferToCreate.TransferStatus, actualTransfer.TransferStatus);
            Assert.AreEqual(transferToCreate.Amount, actualTransfer.Amount);

        }

        /// <summary>
        /// Tests the <see cref="TransferSqlDAO.GetAllTransfersInvolvingUserId(int)"/> Method.
        /// </summary>
        /// <param name="userId">The UserID</param>
        /// <param name="expectedNumOfTransfers">The expected number of transfers.</param>
        [DataTestMethod]
        [DataRow(4,3)]//ben has 3 transfers
        [DataRow(5, 3)]//val has 3 transfers
        [DataRow(2, 0)]//paul has 0 transfers
        public void GetAllTransfersInvolvingUserId_test(int userId, int expectedNumOfTransfers)
        {
            //arrange

            //act
            List<Transfer> actualTransfers = dao.GetTransfersByUserId(userId);

            //assert
            Assert.IsNotNull(actualTransfers);
            Assert.AreEqual(expectedNumOfTransfers, actualTransfers.Count);
        }
    }


}