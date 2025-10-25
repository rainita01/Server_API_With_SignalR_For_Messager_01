﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server_API_With_SignalR_For_Messager_01.Models;
using WebSocketSharpServer.DbContext.Entities;

namespace WebSocketSharpServer.Models
{
    public class ConversationModel
    {
        public int Id { get; set; }
        public bool IsConversationPrivateChat { get; set; }
        public DateTime CreatedTime { get; set; }
        public string ContactUsername { get; set; }
        public List<int> UsersId { get; set; }
        public byte[] ContactImage { get; set; }
        public LastMessageModel? LastMessage { get; set; }
    }
}
