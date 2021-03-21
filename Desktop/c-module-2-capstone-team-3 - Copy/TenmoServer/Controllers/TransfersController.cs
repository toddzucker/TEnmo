using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using TenmoServer.DAO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using TenmoServer.Models;
using System.Transactions;

namespace TenmoServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class TransfersController : ControllerBase
    {

        private ITransferDAO transferDAO;
        private IAccountDAO accountDAO;
        public TransfersController(ITransferDAO transferDAO, IAccountDAO accountDAO)
        {
            this.transferDAO = transferDAO;
            this.accountDAO = accountDAO;
        }

        /// <summary>
        /// Gets a <see cref="List{T}"/> of <see cref="Transfer"/>s where either the sender or receiver is a given user. 
        /// The user is indicated by ID
        /// </summary>
        /// <param name="userId">The user ID</param>
        [HttpGet]
        public ActionResult<List<Transfer>> GetTransfersForCurrentUser()
        {
            int id = Convert.ToInt32(User.FindFirst("sub")?.Value);
            List<Transfer> result = transferDAO.GetTransfersByUserId(id);
            return Ok(result);
        }

        /// <summary>
        /// adds a new transfer onto the database
        /// </summary>
        /// <param name="transferToCreate">The transfer to create.</param>
        /// <returns>An <see cref="ActionResult"/> with generic type <see cref="Transfer"/></returns>
        [HttpPost]
        public ActionResult<Transfer> CreateTransfer(Transfer transferToCreate)
        {
            Transfer createdTransfer = null;
            //step 1) create the transfer
            //are either of the accounts null?
            if (accountDAO.GetAccountById(transferToCreate.AccountFromId) == null || accountDAO.GetAccountById(transferToCreate.AccountToId) == null)
            {
                return UnprocessableEntity("One or both Account Id's are invalid.");//422
            }
            //is someone trying to transfer zero or negative money?
            if ((float)transferToCreate.Amount <= 0)
            {
                return UnprocessableEntity("The amount to transfer must be greater than zero!");//422
            }
            //does the from account have enough money to complete the transfer? only applicable to SEND transfers
            if ((transferToCreate.TransferType == TransferType.Send) && accountDAO.GetAccountById(transferToCreate.AccountFromId).Balance < transferToCreate.Amount)
            {
                return UnprocessableEntity("Insufficient funds to complete the transfer.");//422
            }

            //set the transfer status based on the transfer type
            /* So this might sound weird, but hear me out. If the transfer is type send, we will default the status to rejected.
             * Why? we're going to add a transfer to the database fully. if no errors occurred, then we will execute it
             * This is done in two steps.
             * 
             * If the creation of the transfer was successful, but the execution was not successful, we want to handle this.
             */
            switch (transferToCreate.TransferType)
            {
                case TransferType.Send:
                    transferToCreate.TransferStatus = TransferStatus.Rejected;
                    break;

                case TransferType.Request:
                    transferToCreate.TransferStatus = TransferStatus.Pending;
                    break;
            }
            createdTransfer = transferDAO.CreateTransfer(transferToCreate);
            //okay the transfer is in the DB. Now we need to try to execute it
            if (createdTransfer == null)
            {
                return StatusCode(500, "The server was unable to create the transfer");
            }
            if (createdTransfer.TransferType == TransferType.Send)
            {
                createdTransfer = ExecuteTransfer(createdTransfer);
                if (createdTransfer.TransferStatus == TransferStatus.Rejected)
                {
                    //this should not happen ever
                    return StatusCode(500, "The Server rejected the transfer.");
                }
            }
            return Created($"{createdTransfer.TransferId}", createdTransfer);
        }
        /// <summary>
        /// Updates a transfer. This method is only to be called upon <see cref="Transfer"/>s where the <see cref="Transfer.TransferType">TransferType</see> is <see cref="TransferType.Request">Request</see> <br></br>
        /// 
        /// </summary>
        /// <param name="transferId">The transfer identifier.</param>
        /// <param name="transferToUpdate">The transfer to update.</param>
        /// <returns></returns>
        [HttpPut("/transfers/{transferId}")]
        public ActionResult UpdateTransferRequest(int transferId, Transfer transferToUpdate)
        {
            try
            {
                Transfer transferActuallyOnTheDB = transferDAO.GetTransferById(transferId);
                if (!Transfer.AreEquivalent(transferToUpdate, transferActuallyOnTheDB))
                {
                    return BadRequest("Fraud detected.");
                }
                else if (transferActuallyOnTheDB.TransferStatus != TransferStatus.Pending)
                {
                    return BadRequest("Transfer must be pending to accept or reject.");
                }
                else if (transferActuallyOnTheDB.TransferType != TransferType.Request)
                {
                    return BadRequest("Transfer must be a request to accept or reject.");
                } else if(accountDAO.GetAccountById(transferActuallyOnTheDB.AccountFromId).Balance < transferActuallyOnTheDB.Amount)
                {
                    return BadRequest("insufficient funds");
                }
                else//happy
                {
                    if (transferToUpdate.TransferStatus == TransferStatus.Approved)
                    {
                        try
                        {
                            ExecuteTransfer(transferToUpdate);
                        }
                        catch
                        {
                            return StatusCode(500, "Internal Database Error, could not execute transfer");
                        }
                    }
                    TransferStatus newStatus = transferToUpdate.TransferStatus;
                    transferDAO.UpdateTransferStatus(transferId, newStatus);
                    return Ok();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        /// <summary>
        /// Executes a given transfer
        /// </summary>
        /// <param name="transfer">The transfer to execture</param>
        /// <returns>a <see cref="Transfer"/> Object that has been executed. 
        /// It's <see cref="Transfer.TransferStatus">TransferStatus</see> should be set to <see cref="TransferStatus.Approved">Approved</see></returns>
        private Transfer ExecuteTransfer(Transfer transfer)
        {
            Transfer executedTransfer = null;
            try
            {
                //todo: ask ben how to make transactions work good
                //using (TransactionScope transaction = new TransactionScope())//ben told me about this, but ben lied
                {
                    //todon't: make sure that the ID of the currently logged in user owns the toAccount on send transfers, or owns the fromAccount on request transfers. but maybe do this in another method
                    //deduct
                    decimal balanceBeforeDeduct = accountDAO.GetAccountById(transfer.AccountFromId).Balance;
                    accountDAO.DeductFromAccount(transfer.AccountFromId, transfer.Amount);
                    decimal balanceAfterDeduct = accountDAO.GetAccountById(transfer.AccountFromId).Balance;
                    decimal amountActuallyDeducted = balanceBeforeDeduct - balanceAfterDeduct;

                    //deposit
                    accountDAO.DepositIntoAccount(transfer.AccountToId, amountActuallyDeducted);

                    //update
                    executedTransfer = transferDAO.UpdateTransferStatus(transfer.TransferId, TransferStatus.Approved);

                    //transaction.Complete();
                }
                return executedTransfer;
            }
            catch (TransactionAbortedException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw e;
            }

        }
    }
}
