using Microsoft.EntityFrameworkCore;
using WebSocketSharpServer.DbContext.DbModel;
using WebSocketSharpServer.DbContext.Entities;
using WebSocketSharpServer.Models;
using WebSocketSharpServer.Services;

namespace Server_API_With_SignalR_For_Messager_01.Services
{
    public class ConversationServices(ApplicationDbModel dbContext,MessageServices messageServices)
    {
        public async Task<List<Conversation>> GetConversationsAsync(int userId)
        {

            var conversations = await dbContext.Conversations
                .AsNoTracking()
                .Include(o=>o.Users)
                .Include(o=>o.Messages) 
                .Where(e => e.Users.Any(i => i.Id == userId))
                .ToListAsync();
            return conversations;
        }

        public async Task<object> GetLastMessageFromConversation(int conversationId)
        {
            var message = await dbContext.Messages
                .AsNoTracking()
                .Where(e => e.ConversationId == conversationId)
                .OrderByDescending(e => e.SentTime)
                .FirstOrDefaultAsync();
            if (message == null)
                throw new Exception();
            return message;
        }

    }
}
