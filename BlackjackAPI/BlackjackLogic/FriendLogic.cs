using BlackjackLogic.Interfaces.Logic;
using BlackjackLogic.Interfaces.Repository;
using BlackjackCommon.Models;
using BlackjackCommon.ViewModels;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BlackjackLogic
{
    public class FriendLogic : IFriendLogic
    {
        private readonly IFriendRepository _friendDAL;
        private readonly IUserRepository _userDAL;

        public FriendLogic(IFriendRepository friendDAL, IUserRepository userDAL)
        {
            _friendDAL = friendDAL;
            _userDAL = userDAL;
        }

        public Response<List<FriendRequestModel>> GetFriendRequests(int user_id)
        {
            try
            {
                var data = _friendDAL.GetFriendRequests(user_id)?.Where(item => item != null).Cast<FriendRequestModel>().ToList();

                if (data == null || data.Count == 0)
                {
                    var message = data == null ? "Default" : "NoPendingFriendshipFound";
                    return new Response<List<FriendRequestModel>>(null, message);
                }

                return new Response<List<FriendRequestModel>>(data, "Success");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }

        public Response<List<SearchModel>> FindUser(int user_id, string searchTerm)
        {
            try
            {
                var data = _friendDAL.FindUser(user_id, searchTerm);

                if (data.Count == 0)
                {
                    return new Response<List<SearchModel>>(null, "NoUsersFound");
                }

                if (data == null)
                {
                    return new Response<List<SearchModel>>(null, "Default");
                }

                return new Response<List<SearchModel>>(data, "Success");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }

        public Response<string> RequestFriendship(int user_id, int befriend_user_id)
        {
            if (befriend_user_id < 0)
            {
                return new Response<string>("InvalidFriendId");
            }

            if (!_userDAL.UserIDExists(befriend_user_id))
            {
                return new Response<string>("FriendIdDoesntExist");
            }

            if (_friendDAL.FriendshipExists(user_id, befriend_user_id))
            {
                return new Response<string>("FriendshipExists");
            }

            if (_friendDAL.FriendshipIsPending(user_id, befriend_user_id))
            {
                return new Response<string>("PendingFriendshipFound");
            }

            _friendDAL.RequestFriendship(user_id, befriend_user_id);
            return new Response<string>();
        }

        public Response<string> UpdateFriendStatus(int user_id, int friend_user_id, string status)
        {
            if (friend_user_id < 0)
            {
                return new Response<string>("InvalidFriendId");
            }

            var valStatusResponse = ValidateStatus(status);
            if (valStatusResponse != null)
            {
                return valStatusResponse;
            }

            if (!_friendDAL.FriendshipIsPending(user_id, friend_user_id))
            {
                return new Response<string>("NoPendingFriendshipFound");
            }

            _friendDAL.UpdateFriendStatus(user_id, friend_user_id, status);
            return new Response<string>();
        }

        private static Response<string>? ValidateStatus(string status)
        {
            if (status.ToLower() != "accepted" && status.ToLower() != "rejected")
            {
                return new Response<string>("InvalidFriendStatus");
            }

            return null;
        }
    }
}
