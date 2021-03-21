using System.Collections.Generic;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public interface IAccountDAO
    {
        Account DeductFromAccount(int accountId, decimal amount);
        Account DepositIntoAccount(int accountId, decimal amount);
        Account GetAccountById(int accountId);
        List<Account> GetAccountsByUserId(int userId);
        int? GetDefaultAccountIdForUserId(int userId);
    }
}