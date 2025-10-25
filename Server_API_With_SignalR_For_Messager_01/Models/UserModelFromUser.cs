﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharpServer.DbContext.Entities;

namespace WebSocketSharpServer.Models
{
    public class UserModelFromUser
    {

        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }
    public class UserModelFromServer
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string? BioCaption { get; set; }
        public byte[] Image { get; set; }
        public string Email { get; set; }

    }
}
