using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using demo_158.MVVM.Model;
using WebSocketSharpServer.DbContext.Entities;

namespace WebSocketSharpServer.Models
{
    public class MessageModelFromUser
    {
        public int Id { get; set; }
        public string? Text { get; set; }
        public MessageTypes MessageType { get; set; }
        public byte[]? Object { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public int ConversationId { get; set; }
    }
    public class MessageModelFromServer   
    {
        public int Id { get; set; }
        public MessageTypes MessageType { get; set; }
        public string? Text { get; set; }
        public byte[]? Object { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public DateTime SendDate { get; set; }
        public int ConversationId { get; set; }
    }
}
