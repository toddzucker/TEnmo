using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    /// <summary>
    /// The SQL DAO class for the <see cref="Transfer"/> Model
    /// </summary>
    /// <seealso cref="TenmoServer.DAO.ITransferDAO" />
    public class TransferSqlDAO : ITransferDAO
    {

        private const string SQL_CREATE_TRANSFER =
            "Insert into transfers(transfer_type_id, transfer_status_id, account_from, account_to, amount) Values ((Select transfer_type_id from transfer_types where transfer_type_desc = @transferType),(select transfer_status_id from transfer_statuses where transfer_status_desc = @transferStatus),@accountFromId,@accountToId,@amount)";
        private const string SQL_GET_TRANSFER_BY_ID = "select * from transfers as t join transfer_types as tt on t.transfer_type_id = tt.transfer_type_id join transfer_statuses as ts on t.transfer_status_id = ts.transfer_status_id join (select a.account_id, u.username as sender from accounts as a join users as u on a.user_id = u.user_id) as af on af.account_id = t.account_from join(select a.account_id, u.username as receiver from accounts as a join users as u on a.user_id = u.user_id) as at on at.account_id = t.account_to where transfer_id = @transferId";
        private const string SQL_UPDATE_TRANSFER_STATUS_BY_ID = "update transfers set transfer_status_id = (select transfer_status_id from transfer_statuses where transfer_status_desc = @statusDescription) where transfer_id = @transferId";
        private const string SQL_GET_TRANSFERS_INVOLVING_USER_BY_USERID = "select * from transfers as t join transfer_types as tt on t.transfer_type_id = tt.transfer_type_id join transfer_statuses as ts on t.transfer_status_id = ts.transfer_status_id join (select account_id,username as sender,a.user_id from accounts as a join users as u on a.user_id = u.user_id) as af on t.account_from = af.account_id join (select account_id,username as receiver,a.user_id from accounts as a join users as u on a.user_id = u.user_id) as at on t.account_to = at.account_id where af.user_id = @userId or at.user_id = @userId";
        private readonly string connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransferSqlDAO"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public TransferSqlDAO(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Gets a <see cref="Transfer"/> object from the Sql Database by it's ID
        /// </summary>
        /// <param name="transferId">The transfer identifier.</param>
        public Transfer GetTransferById(int transferId)
        {
            Transfer retrievedTransfer = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(SQL_GET_TRANSFER_BY_ID, conn);
                    cmd.Parameters.AddWithValue("@transferId", transferId);

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        retrievedTransfer = RowToObject(reader);
                    }

                }
                return retrievedTransfer;
            }
            catch (SqlException)
            {
                throw;
            }
        }


        /// <summary>
        /// Inserts a <see cref="Transfer"/> object into the Sql DB
        /// </summary>
        /// <param name="transferToCreate">The transfer to create.</param>
        /// <returns>the <see cref="Transfer"/> object that was actually inserted on the Sql DB</returns>
        public Transfer CreateTransfer(Transfer transferToCreate)
        {
            Transfer createdTransfer = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(SQL_CREATE_TRANSFER, conn);
                    cmd.Parameters.AddWithValue("@transferType", Enum.GetName(typeof(TransferType), transferToCreate.TransferType));
                    cmd.Parameters.AddWithValue("@transferStatus", Enum.GetName(typeof(TransferStatus), transferToCreate.TransferStatus));
                    cmd.Parameters.AddWithValue("@accountFromId", transferToCreate.AccountFromId);
                    cmd.Parameters.AddWithValue("@accountToId", transferToCreate.AccountToId);
                    cmd.Parameters.AddWithValue("@amount", transferToCreate.Amount);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        SqlCommand idCmd = new SqlCommand("select @@identity", conn);
                        int newTransferId = Convert.ToInt32(idCmd.ExecuteScalar());
                        createdTransfer = GetTransferById(newTransferId);
                    }

                }
                return createdTransfer;
            }
            catch (SqlException ex)
            {
                throw;
            }
        }


        /// <summary>
        /// Gets a <see cref="List{T}"/> of <see cref="Transfer"/>s where either the sender or receiver is a given user. 
        /// The user is indicated by ID
        /// </summary>
        /// <param name="userId">The user ID</param>
        public List<Transfer> GetTransfersByUserId(int userId)
        {
            List<Transfer> result = new List<Transfer>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(SQL_GET_TRANSFERS_INVOLVING_USER_BY_USERID, conn);
                    cmd.Parameters.AddWithValue("@userId", userId);

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        result.Add(RowToObject(reader));
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
        /// Updates the <see cref="TransferStatus"/> of an existing transfer object on the Sql DB
        /// </summary>
        /// <param name="transferId">The ID of the transfer to update</param>
        /// <param name="newStatus">The new <see cref="TransferStatus"/></param>
        /// <returns>A <see cref="Transfer"/> Object with the updated properties as it exists on the Sql DB</returns>
        public Transfer UpdateTransferStatus(int transferId, TransferStatus newStatus)
        {
            Transfer updatedTransfer = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(SQL_UPDATE_TRANSFER_STATUS_BY_ID, conn);
                    cmd.Parameters.AddWithValue("@statusDescription", Enum.GetName(typeof(TransferStatus), newStatus));
                    cmd.Parameters.AddWithValue("@transferId", transferId);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        updatedTransfer = GetTransferById(transferId);
                    }
                }
                return updatedTransfer;
            }
            catch (SqlException)
            {
                throw;
            }
        }
        private Transfer RowToObject(SqlDataReader reader)
        {
            Transfer transfer = new Transfer()
            {
                TransferId = (int)reader["transfer_id"],
                TransferType = (TransferType)Enum.Parse(typeof(TransferType), Convert.ToString(reader["transfer_type_desc"])),
                TransferStatus = (TransferStatus)Enum.Parse(typeof(TransferStatus), Convert.ToString(reader["transfer_status_desc"])),
                AccountFromId = (int)reader["account_from"],
                UsernameFrom = (string)reader["sender"],
                AccountToId = (int)reader["account_to"],
                UsernameTo = (string)reader["receiver"],
                Amount = Convert.ToDecimal(reader["amount"])
            };
            return transfer;
        }

    }
}
