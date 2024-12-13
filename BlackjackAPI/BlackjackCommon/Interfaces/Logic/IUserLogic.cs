using BlackjackCommon.Models;

namespace BlackjackCommon.Interfaces.Logic
{
    public interface IUserLogic
    {
        Response<string> CreateAccount(string username, string password);
        int ValidateUser(string username, string password);
        Response<string> ChangePassword(int user_id, string old_password, string new_password, string repeat_new_password);
        Response<string> ChangeUsername(int user_id, string user_name);
        string CreateJWT(int user_id, string username);
        int GetUserIDFromJWT(string jwt_token);
    }
}
