using MenuFramework;
using System;
using System.Collections.Generic;
using System.Text;
using TenmoClient.Data;

namespace TenmoClient.Views
{
    public class CreateTransfersMenu : ConsoleMenu
    {
        public TransferType transferType;

        private readonly AuthService authService;
        public CreateTransfersMenu(AuthService authService, TransferType transferType)
        {
            this.authService = authService;
            this.transferType = transferType;

            List<User> users = authService.GetUsers();
            foreach (User user in users)
            {
                if (UserService.GetUserId() != user.UserId)
                {
                    AddOption($"{user.Username}", CreateTransfer, user);
                }
            }
            AddOption("Exit", Exit);
        }

        protected override void OnBeforeShow()
        {
            Console.WriteLine($"TE Transfer Menu for User: {UserService.GetUserName()}");
            switch (this.transferType)
            {
                case TransferType.Send:
                    Console.WriteLine("Select a user you wish to send money to."); 
                    break;
                case TransferType.Request:
                    Console.WriteLine("Select a user you wish to request money from.");
                    break;
            }
        }

        private MenuOptionResult CreateTransfer(User otherUser)
        {
            Transfer transfer = new Transfer();
            string prompt = "";
            string sendingMessage = "";
            switch (this.transferType)
            {
                case TransferType.Send:
                    prompt = $"How much money do you wish to send to {otherUser.Username}?";
                    transfer.TransferType = TransferType.Send;
                    transfer.AccountFromId = (int)authService.GetDefaultAccountIdFromUserId(UserService.GetUserId());
                    transfer.AccountToId = (int)authService.GetDefaultAccountIdFromUserId(otherUser.UserId);
                    sendingMessage = "Sending transfer to server...";
                    break;
                case TransferType.Request:
                    prompt = $"How much money do you wish to request from {otherUser.Username}?";
                    transfer.TransferType = TransferType.Request;
                    transfer.AccountToId = (int)authService.GetDefaultAccountIdFromUserId(UserService.GetUserId());
                    transfer.AccountFromId = (int)authService.GetDefaultAccountIdFromUserId(otherUser.UserId);
                    sendingMessage = "Sending request to server...";
                    break;
            }
            //tell the user that they are currently making a transfer of type transfgerType
            decimal amount = GetDecimal(prompt,defaultValue:0);
            if (amount <= 0)//error checking, we cant even send a negative request to the server
            {
                Console.WriteLine("Transaction Canceled");
                return MenuOptionResult.WaitAfterMenuSelection;
            } else if(amount<=(decimal)0.0000000001) 
            {
                Console.WriteLine("Amount too small");
                return MenuOptionResult.WaitAfterMenuSelection;
            }
            transfer.Amount= amount;
            Console.WriteLine(sendingMessage);
            Transfer createdTransfer = authService.CreateTransfer(transfer);
            if (createdTransfer != null)
            {
                Console.WriteLine("Success!");
            }

            return MenuOptionResult.WaitAfterMenuSelection;
        }

    }
}
