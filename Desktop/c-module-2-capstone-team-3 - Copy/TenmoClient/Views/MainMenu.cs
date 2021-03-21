using MenuFramework;
using System;
using System.Collections.Generic;
using System.Text;
using TenmoClient.Data;

namespace TenmoClient.Views
{
    public class MainMenu : ConsoleMenu
    {
        private readonly AuthService authService;
        public MainMenu(AuthService authService)
        {
            this.authService = authService;
            AddOption("View your current balance", ViewBalance)
                .AddOption("View your past transfers", ViewTransfers)
                .AddOption("View your pending requests", ViewRequests)
                .AddOption("Send TE bucks", SendTEBucks)
                .AddOption("Request TE bucks", RequestTEBucks)
                .AddOption("Log in as different user", Logout)
                .AddOption("Exit", Exit);
        }

        protected override void OnBeforeShow()
        {
            Console.WriteLine($"TE Account Menu for User: {UserService.GetUserName()}");
        }

        private MenuOptionResult ViewBalance()
        {
            //get a list of all the accounts that the current logged in user owns
            List<Account> myAccounts = authService.GetAccountsForCurrentUser();

            //if the list was length 0, that means no accounts were found.
            if (myAccounts.Count == 0)
            {
                Console.WriteLine("Error. No accounts were found in your name");
            }
            else
            {
                //for now, we expect that every user has exactly 1 account
                Account account = myAccounts[0];

                //get the balance from it
                Console.WriteLine($"Your current account balance is: {account.Balance:c2}");
            }
            return MenuOptionResult.WaitAfterMenuSelection;
        }

        private MenuOptionResult ViewTransfers()//number 5 and onwards
        {
            TransferHistoryMenu transferHistoryMenu = new TransferHistoryMenu(authService);
            transferHistoryMenu.Show();
            return MenuOptionResult.DoNotWaitAfterMenuSelection;
        }

        private MenuOptionResult ViewRequests()//opt
        {
            PendingTransfersMenu pendingTransfersMenu = new PendingTransfersMenu(authService);
            pendingTransfersMenu.Show();
            return MenuOptionResult.DoNotWaitAfterMenuSelection;
        }

        private MenuOptionResult SendTEBucks()
        {
            //open a list of all users.
            CreateTransfersMenu createTransfersMenu = new CreateTransfersMenu(authService, TransferType.Send);
            createTransfersMenu.Show();
            return MenuOptionResult.DoNotWaitAfterMenuSelection;
        }

        private MenuOptionResult RequestTEBucks()//opt
        {
            CreateTransfersMenu createTransfersMenu = new CreateTransfersMenu(authService, TransferType.Request);
            createTransfersMenu.Show();
            return MenuOptionResult.DoNotWaitAfterMenuSelection;
        }

        private MenuOptionResult Logout()
        {
            UserService.SetLogin(new APIUser()); //wipe out previous login info
            return MenuOptionResult.CloseMenuAfterSelection;
        }

    }
}
