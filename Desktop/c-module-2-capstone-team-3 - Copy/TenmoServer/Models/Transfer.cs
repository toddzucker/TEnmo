using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TenmoServer.Models
{
    public class Transfer
    {
        public int TransferId { get; set; }
        public TransferType TransferType { get; set; }
        public TransferStatus TransferStatus { get; set; }
        public int AccountFromId { get; set; }//change these to userfrom and usertoid ???
        public string UsernameFrom { get; set; }
        public int AccountToId { get; set; }
        public string UsernameTo { get; set; }
        public decimal Amount { get; set; }
        public static bool AreEquivalent(Transfer transferA, Transfer transferB)
        {
            if (transferA.TransferId != transferB.TransferId)
            {
                return false;
            }
            else if (transferA.TransferType != transferB.TransferType)
            {
                return false;
            }
            else if (transferA.AccountFromId != transferB.AccountFromId)
            {
                return false;
            }
            else if (transferA.AccountToId != transferB.AccountToId)
            {
                return false;
            }
            else if (transferA.Amount != transferB.Amount)
            {
                return false;
            }
            else
            {
                return true; // :)
            }
        }
    }
    public enum TransferType
    {
        Request = 1, Send = 2
    }
    public enum TransferStatus
    {
        Pending = 1, Approved = 2, Rejected = 3
    }
    
}
