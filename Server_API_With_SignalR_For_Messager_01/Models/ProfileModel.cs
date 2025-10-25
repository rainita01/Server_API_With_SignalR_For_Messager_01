﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketSharpServer.Models
{
    public class ProfileModel
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string? Bio { get; set; }
    }
}
