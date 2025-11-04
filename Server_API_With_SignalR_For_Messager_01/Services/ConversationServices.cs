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
        public async Task<List<Conversation>> GetConversationsAsync(int userId)
        {

            var conversations = await dbContext.Conversations
                .AsNoTracking()
                .Include(o=>o.Users)
                .Where(e => e.Users.Any(i => i.Id == userId))
                .ToListAsync();
            return conversations;
        }
        
    }
}
