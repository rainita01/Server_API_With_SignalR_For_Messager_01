using Microsoft.AspNetCore.SignalR;
using Server_API_With_SignalR_For_Messager_01.Services;
using demo_158.MVVM.Model;
using demo_158.Services.Enums;
using WebSocketSharpServer.DbContext.DbModel;
using WebSocketSharpServer.Models;
using WebSocketSharpServer.Services;

namespace Server_API_With_SignalR_For_Messager_01.Hubs
{
    public class MainHub : Hub
    {
        private readonly UsersManager _users;
        private readonly MemberShipServices _memberShipServices;
        private readonly MessageServices _messageServices;
        private readonly ConversationServices _conversationServices;
        private readonly ProfileServices _profileServices;
        private readonly StateServices _stateServices;
       

        public MainHub(UsersManager users,
            MemberShipServices memberShipServices,
            MessageServices messageServices,
            ConversationServices conversationServices,
            ProfileServices profileServices,
            StateServices stateServices
            )
        {

      
            _users = users;
            _memberShipServices = memberShipServices;
            _messageServices = messageServices;
            _conversationServices = conversationServices;
            _profileServices = profileServices;
            _stateServices = stateServices;
        }
        // sign up methods
        public async Task SignUp(UserModelFromUser user)
        {
            if (!string.IsNullOrEmpty(user.Password) && !await (_memberShipServices.IsUserExistAsync(user.Username)))
            {
                await _memberShipServices.CreateUserAsync(user.Email, user.Username, user.Password);
                await Clients.Caller.SendAsync("SuccessSignUp", $"Welcome to AMassager {user.Username}");

            }
            else
            {
                await Clients.Caller.SendAsync("InvalidSignUp", $"Invalid Username or password");
            }
        }
        // Connections Methods 
        public async Task RegisterRequest(UserModelFromUser user)
        {
            try
            {
                if (await _memberShipServices.UsernamePasswordValidationAsync(user.Username, user.Password))
                {
                    var dbuser = await _memberShipServices.GetUserAsync(user.Username);
                    var userToSend = new UserModelFromServer()
                    {
                        Username = dbuser.Username,
                        BioCaption = dbuser.BioCaption,
                        Email = dbuser.Email,
                        Id = dbuser.Id,
                        Image = dbuser.Image?.ImageData
                    };
                    await Clients.Caller.SendAsync("ReceiveUser", userToSend);
                    _users.ConnectedUsers.Add(user.Username,Context.ConnectionId);
                    await _stateServices.OnConnectUser(user.Username);
                    await Clients.All.SendAsync("CheckUsersState", State.Online,user.Username);
                }
                else
                {
                    await Clients.Caller.SendAsync("ExceptionMessage", "Incorrect Username or Password");
                }
            }
            catch (Exception e)
            {
                await Clients.Caller.SendAsync("ExceptionMessage", "There is an exception for sending message");
            }
        }

        public async Task ReconnectRequest(UserModelFromUser user)
        {
            if (user.Username != null)
            {
                _users.ConnectedUsers.Add(user.Username, Context.ConnectionId);
                await Clients.All.SendAsync("CheckUsersState", State.Online,user.Username);
            }
        }


        // conversations methods
        public async Task ReceiveConversations(int userId)
        {
            var conversations = await _conversationServices.GetConversationsAsync(userId);
            var conversationToSend = new List<ConversationModelFromServer>();
                
            foreach (var e in conversations)
            {
                var contactUser = e.Users.FirstOrDefault(s => s.Id != userId);
                await _memberShipServices.GetUserImageAsync(contactUser);

                var contactInfo = new ContactUserModel()
                {
                    ContactUsername = contactUser.Username,
                    Id = contactUser.Id,
                    UsersId = e.Users.Select(s => s.Id).ToList(),
                    ContactImage = contactUser.Image?.ImageData,
                    Bio = contactUser.BioCaption,
                    Email = contactUser.Email,
                    LastActiveTime = contactUser.LastActiveTime,
                    State = contactUser.State
                };

                conversationToSend.Add(new ConversationModelFromServer()
                {
                    Id = e.Id,
                    IsConversationPrivateChat = e.IsConversationPrivateChat,
                    CreatedTime = e.CreatedTime,
                    ContactUserModel = contactInfo
                });
            }
            await Clients.Caller.SendAsync("ReceiveConversations", conversationToSend);
        }
     
        //messages methods
        public async Task SendMessageToPrivate(string toUser, MessageModelFromUser message)
        {
            _users.ConnectedUsers.TryGetValue(toUser, out var value);
            var messageToSend = _messageServices.ConvertMessageFromUserToMessageFromServer(message);
            if (messageToSend == null)
                throw new Exception();

            if (!string.IsNullOrEmpty(value))
            {       
                await Clients.Client(value).SendAsync("ReceivePrivateMessage", messageToSend);
            }
            
            await _messageServices.SaveMessageToDataBase(messageToSend);
        }

        public async Task ReceiveMessages(int conversationId)
        {
            var messages = await _messageServices.UploadMessagesAsync(conversationId, null);
            var messageModels = await _messageServices.ConvertMessagesToMessagesModelFromUserAsync(messages);
            await Clients.Caller.SendAsync("ReceiveMessages", messageModels);

        }

        //profile methods
        public async Task ChangeProfile(ProfileEditModel profile)
        {
            var user =await _memberShipServices.GetUserAsync(profile.Username);
            await _profileServices.ProfileChangeSubmitAsync(profile, user);
            await Clients.Caller.SendAsync("ChangeProfile", "ProfileUpdatedSuccessfully.");

        }

        public async Task<ServerAnswer> UploadProfileImage(byte[] imageBytes,int userId)
        {
            return await _memberShipServices.UploadProfileImage(imageBytes, userId);

        }
        // disconnect handler
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string connectionId = Context.ConnectionId;
            
            if (!string.IsNullOrEmpty(connectionId))
            {
              var user =  _users.ConnectedUsers.FirstOrDefault(e => e.Value == connectionId);
              if (user.Key != null)
              {
                  _users.ConnectedUsers.Remove(user.Key);
                    await _stateServices.OnDisconnectUser(user.Key);
                  await Clients.All.SendAsync("CheckUsersState", State.Offline,user.Key);
              }
          
            }
            await base.OnDisconnectedAsync(exception);
        }
    
    }
}
