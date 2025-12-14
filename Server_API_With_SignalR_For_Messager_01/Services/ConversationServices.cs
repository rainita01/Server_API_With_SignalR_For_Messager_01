using System.Diagnostics.CodeAnalysis;
using demo_158.MVVM.Model;
using Microsoft.EntityFrameworkCore;
using WebSocketSharpServer.DbContext.DbModel;
using WebSocketSharpServer.DbContext.Entities;
using WebSocketSharpServer.Models;
using WebSocketSharpServer.Services;

namespace Server_API_With_SignalR_For_Messager_01.Services
{
    public class ConversationServices(ApplicationDbModel dbContext,MessageServices messageServices)
    {
        public async Task<List<Conversation>> GetUserConversationsAsync(int userId)
        {

            var conversations = await dbContext.Conversations
                .AsNoTracking()
                .Include(o=>o.Users)
                .Where(e => e.Users.Any(i => i.Id == userId))
                .ToListAsync();
            return conversations;
        }

        public async Task<Conversation> GetConversationAsync(int id)
        {
            
            var result =  await dbContext.Conversations
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
            if (result == null)
            {
                throw new NullReferenceException("Conversation not found");
            }
            return result;
        }
        public async Task<bool> IsConversationExistAsync(int myUserId, int contactUserId)
        {
            return await 
                dbContext.Conversations.AnyAsync(c => c.Users.Any(u => u.Id == myUserId) && c.Users.Any(u => u.Id == contactUserId));
        }

        public async Task<int> AddUsersToNewConversationAsync(User u1, User u2)
        {
            var users = new List<User>();
            users.Add(u1);
            users.Add(u2);
            var conversation = new Conversation()
            {
                Users = users,
                CreatedBy = u1.Username,
                CreatedTime = DateTime.Now,
                IsConversationPrivateChat = true,
                Messages = new List<Message>()
            };

            await dbContext.Conversations.AddAsync(conversation);
            await  dbContext.SaveChangesAsync();
            return conversation.Id;
        }

        public async Task DeleteConversationAsync(Conversation conversation)
        {
            dbContext.Conversations.Remove(conversation);
            var messages = await dbContext.Messages.Where(e => e.ConversationId == conversation.Id).ToListAsync();
            foreach (var message in messages)
            {
                dbContext.Messages.Remove(message);
            }
            await dbContext.SaveChangesAsync();
        }
    }
}
