using System.Collections.Generic;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public interface ITransferDAO
    {
        Transfer CreateTransfer(Transfer transferToCreate);
        Transfer GetTransferById(int transferId);
        List<Transfer> GetTransfersByUserId(int userId);
        Transfer UpdateTransferStatus(int transferId, TransferStatus newStatus);
    }
}