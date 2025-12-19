using System.Collections.Concurrent;
using WebSocketSharpServer.Models;

namespace Server_API_With_SignalR_For_Messager_01.Hubs
{
    public  class  UsersManager
    {
        public  ConcurrentDictionary<string,string> ConnectedUsers = new ();    
        public ConcurrentDictionary<string,ConcurrentQueue<MessageModelFromServer>> OfflineUsersMessages = new ();
        public ConcurrentDictionary<string,ConcurrentQueue<int>> OfflineDeletedMessage = new ();
        public ConcurrentDictionary<string, ConcurrentQueue<EditMessageModel>> OfflineEditedMessage = new();
        public ConcurrentDictionary<string, ConcurrentQueue<int>> OfflineConversationDeleted = new();
    }
}
