using System;
using System.Collections.Generic;
using System.Text;

namespace TenmoClient.Data
{
    /// <summary>
    /// a publicly viewable user that is separated from protected info
    /// </summary>
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }

    }
}