using demo_158.Services.Enums;
using WebSocketSharpServer.DbContext.DbModel;
using WebSocketSharpServer.DbContext.Entities;
using WebSocketSharpServer.Services;

namespace Server_API_With_SignalR_For_Messager_01.Services;

public class StateServices(ApplicationDbModel dbModel,MemberShipServices memberShipServices)
{
    public async Task OnConnectUser(string username)
    {
        var user = await memberShipServices.GetUserAsync(username);
        user.State = State.Online;
        await dbModel.SaveChangesAsync();

    }
    public async Task OnDisconnectUser(string username)
    {
        var user = await memberShipServices.GetUserAsync(username);
        user.State = State.Offline;
        user.LastActiveTime =DateTime.Now;
        await dbModel.SaveChangesAsync();

    }   
}