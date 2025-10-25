using demo_158.MVVM.Model;
using WebSocketSharpServer.DbContext.DbModel;
using WebSocketSharpServer.DbContext.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Server_API_With_SignalR_For_Messager_01.Services
{
    public class ProfileServices(ApplicationDbModel dbModel)
    {

        public async Task ProfileChangeSubmitAsync(ProfileEditModel profile,User user)
        {
            user.BioCaption = profile.Bio;
            user.Email = profile.Email;
            await dbModel.SaveChangesAsync();
        }
    }
}
