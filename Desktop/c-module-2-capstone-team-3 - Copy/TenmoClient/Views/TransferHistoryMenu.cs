using MenuFramework;
using System;
using System.Collections.Generic;
using System.Text;
using TenmoClient.Data;

namespace TenmoClient.Views
{
    public class TransferHistoryMenu : ConsoleMenu
    {


        private readonly AuthService authService;
        private Dictionary<int, string> transferDetails = new Dictionary<int, string>();

        public TransferHistoryMenu(AuthService authService)
        {

            this.authService = authService;
            List<Transfer> myTransfers = authService.GetTransfersForCurrentUser();
            foreach (Transfer transfer in myTransfers)
            {
                transferDetails[transfer.TransferId] = transfer.ToString();
                AddOption(transfer.TransferId.ToString(), DisplayTransferDetails, transfer.TransferId);
            }
            AddOption("Exit", Close);
        }
        protected override void OnBeforeShow()
        {
            Console.WriteLine($"TE Transfer History for User: {UserService.GetUserName()}");
        }

        private MenuOptionResult DisplayTransferDetails(int transferId)
        {
            Console.WriteLine(transferDetails[transferId]);
            return MenuOptionResult.WaitAfterMenuSelection;
        }

        
    }

}
