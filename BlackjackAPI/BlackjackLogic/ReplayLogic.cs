﻿using BlackjackCommon.Interfaces.Logic;
using BlackjackCommon.Interfaces.Repository;
using BlackjackCommon.Models;
using BlackjackCommon.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace BlackjackLogic
{
	public class ReplayLogic : IReplayLogic
	{
		private readonly IReplayRepository _replayDAL;
		public ReplayLogic(IReplayRepository replayDAL)
		{
			_replayDAL = replayDAL;

		}

		public async Task<Response<List<ReplayModel>>> RetrieveReplayAsync(int user_id, string group_id)
		{
			try
			{
				//retrieve rounds the player was active in
				var rounds = await _replayDAL.RetrieveGameRoundsAsync(user_id, group_id);

				if (rounds == null || rounds.Count == 0)
				{
					return new Response<List<ReplayModel>>(null, "Default");
				}

				//retrieve lobby members
				var lobby = await _replayDAL.RetrieveLobbyMembersAsync(rounds, user_id, group_id);

				if (lobby == null || lobby.Count == 0)
				{
					return new Response<List<ReplayModel>>(null, "Default");
				}

				//retrieve game per round
				var gamereplay = await _replayDAL.RetrieveGameReplayAsync(rounds, group_id);

				if (gamereplay == null || gamereplay.Count == 0)
				{
					return new Response<List<ReplayModel>>(null, "Default");
				}

				var combinedData = new List<ReplayModel>();

				combinedData.AddRange(lobby);
				combinedData.AddRange(gamereplay);

				//retrieve chat 
				var chatmessages = await _replayDAL.RetrieveChatReplayAsync(rounds, group_id);

				if (chatmessages != null && chatmessages.Count > 0)
				{
					combinedData.AddRange(chatmessages);
				}

				var sortedData = combinedData
					.OrderBy(item => item.round) 
					.ThenBy(item => item.datetime) 
					.ToList();

				// Return the combined data
				return new Response<List<ReplayModel>>(sortedData, "Success");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
				throw;
			}
		}


	}
}