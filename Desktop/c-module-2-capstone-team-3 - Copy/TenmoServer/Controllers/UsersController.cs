using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenmoServer.DAO;
using TenmoServer.Models;

namespace TenmoServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        IUserDAO userDao;
        IAccountDAO accountDao;

        public UsersController(IUserDAO userDao, IAccountDAO accountDao)
        {
            this.userDao = userDao;
            this.accountDao = accountDao;
        }

        [HttpGet]
        public ActionResult<List<User>> GetUsers()
        {
            List<User> result = userDao.GetUsers();
            if (result.Count == 0)
            {
                return StatusCode(500, "Database error.");
            }
            else
            {
                return Ok(result);
            }
        }

        [HttpGet("/{userId}/default_account")]
        public ActionResult<int?> GetDefaultAccountIdForUserId(int userId)
        {
            int? result = accountDao.GetDefaultAccountIdForUserId(userId);
            if (result == null)
            {
                return StatusCode(500, "No default account found.");
            }
            else
            {
                return Ok(result);
            }
        }

    }
}
