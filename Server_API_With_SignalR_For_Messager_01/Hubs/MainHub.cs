using Microsoft.AspNetCore.SignalR;
using Server_API_With_SignalR_For_Messager_01.Abstracts.HubHelperInterfaces;
using Server_API_With_SignalR_For_Messager_01.Services;
using System.Security.Cryptography.X509Certificates;
using demo_158.MVVM.Model;
using Server_API_With_SignalR_For_Messager_01.Models;
using WebSocketSharpServer.DbContext.Entities;
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

        public MainHub(UsersManager users,MemberShipServices memberShipServices,MessageServices messageServices, ConversationServices conversationServices,ProfileServices profileServices)
        {
            _users = users;
            _memberShipServices = memberShipServices;
            _messageServices = messageServices;
            _conversationServices = conversationServices;
            _profileServices = profileServices;
        }


        public async Task RegisterRequest(UserModelFromUser user)
        {
            try
            {
                if (await _memberShipServices.UsernamePasswordValidationAsync(user.Username, user.Password))
                {
                    var dbuser = await _memberShipServices.GetUserAsync(user.Username);
                    var userToSend = new UserModelFromServer()
                    {
                        Username = user.Username,
                        BioCaption = dbuser.BioCaption,
                        Email = dbuser.Email,
                        UserId = dbuser.Id
                        
                       
                    };
                    var conversations = await _conversationServices.GetConversationsAsync(dbuser.Id);
                    var conversationToSend = conversations.Select(e =>
                    {
                        var contactUser = e.Users.FirstOrDefault(o => o.Id != dbuser.Id);
                        var lastMessage = e.Messages
                            .OrderBy(m => m.SentTime)
                            .LastOrDefault();
                        return new ConversationModel()
                        {
                            Id = e.Id,
                            ContactUsername = contactUser.Username,
                            CreatedTime = e.CreatedTime,
                            IsConversationPrivateChat = e.IsConversationPrivateChat,
                            LastMessage = _messageServices.ConvertMessageToLastMessageModel(lastMessage)
                        };
                    }).ToList();
                    await Clients.Caller.SendAsync("ReceiveUser", userToSend,conversationToSend);
                    _users.ConnectedUsers.Add(user.Username,Context.ConnectionId);
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

        public async Task SendMessageToPrivate(string toUser, MessageModelFromUser message)
        {
            _users.ConnectedUsers.TryGetValue(toUser, out var value);
            var convertMessage = _messageServices.ConvertMessageFromUserToMessageFromServer(message);
            if (convertMessage == null)
                throw new Exception();

            if (!string.IsNullOrEmpty(value))
            {
                await Clients.Client(value).SendAsync("ReceivePrivateMessage", convertMessage);
            }
            
            await _messageServices.SaveMessageToDataBase(convertMessage);
        }

        public async Task ConversationSender(ConversationModel conversationModel)
        {
            var messages = await _messageServices.UploadMessagesAsync(conversationModel, null);
            var user = await _memberShipServices.GetUserAsync(conversationModel.ContactUsername);
            var messageModels = await _messageServices.ConvertMessagesToMessagesModelFromUserAsync(messages);
            var userModelFromServer = _memberShipServices.ConvertUserToUserModelFromServer(user);
            await Clients.Caller.SendAsync("ReceiveConversation", messageModels, userModelFromServer);

        }

        public async Task ChangeProfile(ProfileEditModel profile)
        {
            var user =await _memberShipServices.GetUserAsync(profile.Username);
            await _profileServices.ProfileChangeSubmitAsync(profile, user);
            await Clients.Caller.SendAsync("ChangeProfile", "ProfileUpdatedSuccessfully.");

        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string connectionId = Context.ConnectionId;
            await base.OnDisconnectedAsync(exception);
        }

    }
}
