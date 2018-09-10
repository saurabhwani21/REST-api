using Library.API.Entities;
using System;
using System.Collections.Generic;
using Library.API.Helpers;

namespace Library.API.Services
{
    public interface ILibraryRepository
    {
        PagedList<User> GetUsers(UserResourceParameters userResourceParameters);
        User GetUser(Guid userId);
        IEnumerable<User> GetUsers(IEnumerable<Guid> userIds);
        Uid ValidateUser(string username, string password);
        void AddUser(User user);
        void DeleteUser(User user);
        void UpdateUser(User user);
        bool UserExists(Guid userId);
        bool UserExists(string username);
        PagedList<Score> GetScoresForUser(Guid userId, ScoresResourceParameters scoresResourceParameters);
        Score GetScoreForUser(Guid userId, Guid scoreId);
        void AddScoreForUser(Guid userId, Score score);
        void CompareScoreForUser(Guid userId, Score score);
        void UpdateScoreForUser(Score score);
        void DeleteScore(Score score);
        bool Save();
    }
}
