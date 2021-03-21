using MenuFramework;
using System;
using System.Collections.Generic;
using System.Text;
using TenmoClient.Data;

namespace TenmoClient.Views
{
    public class PendingTransfersMenu : ConsoleMenu
    {
        private readonly AuthService authService;
        private Dictionary<int, (Transfer, string)> transferDetails = new Dictionary<int, (Transfer, string)>();

        public PendingTransfersMenu(AuthService authService)
        {

            this.authService = authService;
            
        }

        protected override void RebuildMenuOptions()
        {
            base.ClearOptions();
            List<Transfer> myTransfers = authService.GetTransfersForCurrentUser();
            foreach (Transfer transfer in myTransfers)
            {
                User ownerOfSourceAccount = authService.GetAccountPublicInfo(transfer.AccountFromId).Item1;
                if (transfer.TransferType == TransferType.Request && transfer.TransferStatus == TransferStatus.Pending && ownerOfSourceAccount.UserId == UserService.GetUserId())
                {
                    transferDetails[transfer.TransferId] = (transfer, transfer.ToString());
                    AddOption(transfer.TransferId.ToString(), SelectTransfer, transfer.TransferId);

                }
            }
            AddOption("Exit", Close);
        }
        protected override void OnBeforeShow()
        {
            Console.WriteLine($"Pending Requests:");
        }

        private MenuOptionResult SelectTransfer(int transferId)
        {
            Console.WriteLine(transferDetails[transferId].Item2);
            bool accepted = GetBool("Would you like to accept this transfer?");
            bool successful = authService.RespondToTransfer(transferDetails[transferId].Item1, accepted);
            if (successful)
            {
                Console.Write($"{transferDetails[transferId].Item1.UsernameTo}'s ");
                Console.WriteLine(accepted ? "Request accepted!":"Request declined.") ;
            }

            return MenuOptionResult.WaitAfterMenuSelection;
        }
    }

}
