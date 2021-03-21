using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TenmoServer.DAO;
using TenmoServer.Models;

namespace TenmoServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class AccountsController : ControllerBase
    {

        private IAccountDAO accountDAO;
        private IUserDAO userDAO;
        public AccountsController(IAccountDAO AccountDAO,IUserDAO userDAO)
        {
            this.accountDAO = AccountDAO;
            this.userDAO = userDAO;
        }
        [HttpGet]
        public ActionResult<List<Account>> GetAccountsForCurrentUser()
        {
            //we need to get the UserID of currently logged in user
            int id = Convert.ToInt32(User.FindFirst("sub")?.Value);
            List<Account> result = accountDAO.GetAccountsByUserId(id);
            if (result.Count == 0)
            {
                return BadRequest("No accounts were found for the user currently logged in");
            }
            else
            {
                return Ok(result);
            }
        }
        [HttpGet("{accountId}/public_info")]
        public ActionResult<List<object>> GetPublicInfoForAccount(int accountId)
        {
            Account account = accountDAO.GetAccountById(accountId);
            User user = null;
            List<object> result = new List<object>();
            if (account != null)
            {
                account.Balance = decimal.MinValue;//protects sensitive data
                user = userDAO.GetUser(account.UserId);
                if(user == null)
                {
                    return StatusCode(500, "The account exists, but it's owner does not. this should not be possible");
                }
                result.Add(user);
                result.Add(account);
                return Ok(result);

            }
            else
            {
                return BadRequest("That account doesn't exist.");
            }


        }
    }
}
