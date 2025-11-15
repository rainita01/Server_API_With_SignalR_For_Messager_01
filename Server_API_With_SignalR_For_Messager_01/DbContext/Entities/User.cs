using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using demo_158.Services.Enums;

namespace WebSocketSharpServer.DbContext.Entities
{
   
        public class User
        {
            public int Id { get; set; }
            public string Email { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string PasswordSalt { get; set; }
            public DateTime RegisterDate { get; set; }
            public string? BioCaption { get; set; }
            public UserImage? Image { get; set; }
            public int? ImageId { get; set; }
            public DateTime LastActiveTime { get; set; } 
            public State State { get; set; }
            public List<Conversation> Conversations { get; set; }

        }
        public class UserImage
        {
            public int Id { get; set; }
            public byte[] ImageData { get; set; }
            public User User { get; set; }
            public int UserId { get; set; }
        }

    public class Conversation
        {

            public int Id { get; set; }
            public bool IsConversationPrivateChat { get; set; }
            public DateTime CreatedTime { get; set; }
            public string CreatedBy { get; set; }
            public List<Message> Messages { get; set; } = new();
            public List<User> Users { get; set; }

         }
        
          public class Message
         {
            public int Id { get; set; }
            public DateTime SentTime { get; set; }
            public int ConversationId { get; set; }
            public Conversation Conversation { get; set; }
            public int UserId { get; set; }
            public User User { get; set; }
            public bool IsSeen { get; set; } = false;
         }
        
        public class TextMessage : Message
        {
            public string Text { get; set; }
        }

        public class ImageMessage : Message
        {
            public byte[] ImageData { get; set; }
            public string? Title { get; set; }
        }

        public class VideoMessage : Message
        {
            public byte[] VideoData { get; set; }
            public string? Title { get; set; }

        }
        public class AudioMessage : Message
        {
            public byte[] AudioData { get; set; }
            public string? Title { get; set; }
        }
        public class FileMessage : Message
        {
            public byte[] FileData { get; set; }
            public string? Title { get; set; }
    }
}

