using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Server_API_With_SignalR_For_Messager_01.Services;
using demo_158.MVVM.Model;
using demo_158.Services.Enums;
using Microsoft.IdentityModel.Tokens;
using WebSocketSharpServer.DbContext.DbModel;
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
                var userCreated = await _memberShipServices.CreateUserAsync(user.Email, user.Username, user.Password);
                var userFromServer = new UserModelFromServer()
                {

                    Username = userCreated.Username,
                    Email = userCreated.Email,
                    BioCaption = userCreated.BioCaption,
                    Id = userCreated.Id,
                    Image = userCreated.Image?.ImageData,
                };
                await Clients.Caller.SendAsync("SignedUserReceived", userFromServer);


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
                    _users.ConnectedUsers.TryAdd(user.Username,Context.ConnectionId);
                    await _stateServices.OnConnectUser(user.Username);
                    await Clients.All.SendAsync("CheckUsersState", State.Online,user.Username);
                    _users.OfflineUsersMessages[user.Username] = new ConcurrentQueue<MessageModelFromServer>();
                    _users.OfflineDeletedMessage[user.Username] = new ConcurrentQueue<int>();
                    _users.OfflineEditedMessage[user.Username] = new ConcurrentQueue<EditMessageModel>();
                }
                else
                {
                    await Clients.Caller.SendAsync("OnErrorLogin", "Incorrect Username or Password");
                }
            }
            catch (Exception e)
            {
                await Clients.Caller.SendAsync("OnErrorLogin", "Incorrect Username or Password");
            }
        }

        public async Task ReconnectRequest(UserModelFromUser user)
        {
                _users.ConnectedUsers[user.Username] = Context.ConnectionId;
                if (_users.OfflineUsersMessages.TryGetValue(user.Username,out var queue))
                {
                    while (!queue.IsNullOrEmpty() && _users.ConnectedUsers.ContainsKey(user.Username))
                    {
                        queue.TryDequeue(out var result);
                        await Clients.Caller.SendAsync("ReceivePrivateMessage",result);
                    }
                }

                if (_users.OfflineDeletedMessage.TryGetValue(user.Username,out var deletedQueue))
                {
                     while (!deletedQueue.IsNullOrEmpty() && _users.ConnectedUsers.ContainsKey(user.Username))
                     {
                         // اینجا ی مشکلی هست که متد دلیت مسیج درواقع ی پارامتر بیشتر میخواد
                         deletedQueue.TryDequeue(out var result);
                         await Clients.Caller.SendAsync("ContactDeletedMessage", result);
                     }
                }
                if (_users.OfflineEditedMessage.TryGetValue(user.Username, out var editedQueue))
                {
                    while (!editedQueue.IsNullOrEmpty() && _users.ConnectedUsers.ContainsKey(user.Username))
                    {
                        editedQueue.TryDequeue(out var result);
                        await Clients.Caller.SendAsync("ContactEditedMessage", result);
                    }
                }
                await Clients.All.SendAsync("CheckUsersState", State.Online,user.Username);
        }


        // conversations methods
        public async Task ReceiveConversations(int userId)
        {
            var conversations = await _conversationServices.GetUserConversationsAsync(userId);
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

        public async Task<ServerAnswer> DeleteConversation(int id,string contactUsername)
        {

          var conversation =  await _conversationServices.GetConversationAsync(id);
          await _conversationServices.DeleteConversationAsync(conversation);
          _users.ConnectedUsers.TryGetValue(contactUsername, out var value);
          if ( string.IsNullOrEmpty(value))
          {
              
          }
          else
          {
              await Clients.Client(value).SendAsync("ContactDeletedConversation", id);
          }
               
          return ServerAnswer.ok;
        }


        //messages methods
        public async Task<int> SendMessageToPrivate(string toUser, MessageModelFromUser message)
        {
            _users.ConnectedUsers.TryGetValue(toUser, out var value);
            var messageToSend = _messageServices.MessageFromServerMapping(message);
            var toUserId = await _memberShipServices.GetUserAsync(toUser);
            var myUser = await _memberShipServices.GetUserAsync(message.Username);
            if (messageToSend == null)
                throw new Exception();

            if (!await _conversationServices.IsConversationExistAsync(message.UserId,toUserId.Id))
            {
                var conversationId =    await _conversationServices.AddUsersToNewConversationAsync(myUser, toUserId);
                messageToSend.ConversationId = conversationId;
                await Clients.Caller.SendAsync("GetConversationId", conversationId, toUser);

            }
            messageToSend.Id =  await _messageServices.SaveMessageToDataBase(messageToSend);
            if (!string.IsNullOrEmpty(value))
            {
                await Clients.Client(value).SendAsync("ReceivePrivateMessage", messageToSend);
            }
            else
            {
                var queue = _users.OfflineUsersMessages.GetOrAdd(toUser, _ => new ConcurrentQueue<MessageModelFromServer>());
                queue.Enqueue(messageToSend);
            }

            return messageToSend.Id;
        }

        public async Task ReceiveMessages(int conversationId)
        {
            var messages = await _messageServices.UploadMessagesAsync(conversationId, null);
            var messageModels = await _messageServices.MessagesFromServerMapping(messages);
            await Clients.Caller.SendAsync("ReceiveMessages", messageModels);

        }

        public async Task<ServerAnswer> DeleteMessage(int messageId,string senderUsername,string receiverUsername)
        {
            
            if (await _messageServices.DeleteMessage(messageId))
            {
               await Clients.Caller.SendAsync("MessageDeleted", ServerAnswer.ok, messageId);
               _users.ConnectedUsers.TryGetValue(receiverUsername,out var value);
               if (value != null)
               {
                   await Clients.Client(value).SendAsync("ContactDeletedMessage", messageId, senderUsername);
               }
               else
               {
                   var queue = _users.OfflineDeletedMessage.GetOrAdd(receiverUsername, _ => new ConcurrentQueue<int>());
                   queue.Enqueue(messageId);
               }
               return ServerAnswer.ok;
            }
            else
            {
                return ServerAnswer.bad;
            }
        }

        public async Task EditMessage(EditMessageModel newMessage)
        {

            if (await _messageServices.EditMessage(newMessage.NewText,newMessage.MessageId))
            {
              await  Clients.Caller.SendAsync("MessageEdited", ServerAnswer.ok, newMessage.NewText, newMessage.MessageId);
              _users.ConnectedUsers.TryGetValue(newMessage.ContactUsername, out var value);
              if (value != null)
              {
                  await Clients.Client(value).SendAsync("ContactEditedMessage",newMessage);
              }
              else
              {
                  var queue = _users.OfflineEditedMessage.GetOrAdd(newMessage.ContactUsername, _ => new ConcurrentQueue<EditMessageModel>());
                  queue.Enqueue( newMessage);
              }
            }
        }

        //profile methods
        public async Task ChangeProfile(ProfileEditModel profile)
        {
            var user =await _memberShipServices.GetUserAsync(profile.Username);
            await _profileServices.ProfileChangeSubmitAsync(profile, user);
            await Clients.Caller.SendAsync("ChangeProfile","ProfileUpdatedSuccessfully.");
            await Clients.All.SendAsync("UserChangedProfile", profile);

        }

        public async Task<ServerAnswer> UploadProfileImage(byte[] imageBytes,int userId)
        {

            var result = await _memberShipServices.UploadProfileImage(imageBytes, userId);

            if (result == ServerAnswer.ok)
            {
                // این بخش رو اضافه نمیکنم فعلا
              await  Clients.All.SendAsync("ContactChangedProfilePicture", userId, imageBytes);
            }

            return result;

        }
        // ask users Methods 

        public async Task AskUsers(int userId)
        {
          var users =  await _memberShipServices.GetTopUsersAsync(userId);
          await Clients.Caller.SendAsync("GetUsersToTalk", users);
        }

        public async Task<ContactUserModel> AskNewMessageUser(string username)
        { 
            
            var user = await _memberShipServices.GetUserAsync(username);
            var contactUsername = new ContactUserModel()
            {

                Bio = user.BioCaption,
                ContactImage = user.Image?.ImageData,
                ContactUsername = user.Username,
                Email = user.Email,
                Id = user.Id,
                LastActiveTime = user.LastActiveTime,
                State = user.State,
            };
            return contactUsername;
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
                  _users.ConnectedUsers.Remove(user.Key,out var value);
                    await _stateServices.OnDisconnectUser(user.Key);
                  await Clients.All.SendAsync("CheckUsersState", State.Offline,user.Key);
              }
          
            }
            await base.OnDisconnectedAsync(exception);
        }
    
    }
}
