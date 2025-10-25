using Microsoft.AspNetCore.SignalR;

namespace Server_API_With_SignalR_For_Messager_01.Abstracts.HubHelperInterfaces
{
    public interface IMessageHandler
    {

        public Task MessageSenderToAll(string user, string message);
        public Task SendToPrivate(string fromUser, string toUser, string message);
        


    }
}
