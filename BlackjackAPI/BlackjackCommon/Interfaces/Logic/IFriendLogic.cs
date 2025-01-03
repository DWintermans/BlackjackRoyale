﻿using BlackjackCommon.Models;
using BlackjackCommon.ViewModels;

namespace BlackjackCommon.Interfaces.Logic
{
    public interface IFriendLogic
    {
		Response<List<SearchModel>> FindUser(int user_id, string searchTerm);
		Response<List<FriendRequestModel>> GetFriendRequests(int user_id);
        Response<string> RequestFriendship(int user_id, int befriend_user_id);
        Response<string> UpdateFriendStatus(int user_id, int friend_user_id, string status);
    }
}
