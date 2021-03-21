using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TenmoClient.Data
{
    public class Transfer
    {
        public int TransferId { get; set; }
        public TransferType TransferType { get; set; }
        public TransferStatus TransferStatus { get; set; }
        public int AccountFromId { get; set; }
        public string UsernameFrom { get; set; }
        public int AccountToId { get; set; }

        public string UsernameTo { get; set; }
        public decimal Amount { get; set; }

        public string ToStringFull()
        {
            return $"Id: {this.TransferId}\t " +
                $"Type: {this.TransferType}\t " +
                $"Amount: {this.Amount:c2}\t " +
                $"Sent from: {this.UsernameFrom} " +
                $"to: {this.UsernameTo} " +
                $"status: {this.TransferStatus}";

        }

        public override string ToString()
        {
            if (this.TransferType == TransferType.Send)
            {
                return $"Sent to: {UsernameTo}, from: {UsernameFrom}\t Amount: {Amount:c2}\t Status: {this.TransferStatus}";
            }
            else if (this.TransferType == TransferType.Request)
            {
                return $"Request from: {UsernameFrom}, to: {UsernameTo}\t Amount: {Amount:c2}\t Status: {this.TransferStatus}";
            }
            else
            {
                return base.ToString();
            }

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

