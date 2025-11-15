using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using demo_158.MVVM.Model;
using WebSocketSharpServer.DbContext.Entities;

namespace WebSocketSharpServer.Models
{
    public class ConversationModelFromServer
    {
        public int Id { get; set; }
        public bool IsConversationPrivateChat { get; set; }
        public DateTime CreatedTime { get; set; }
        public ContactUserModel ContactUserModel { get; set; }

    }
}
