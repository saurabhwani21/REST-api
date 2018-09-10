using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Library.API.Services
{
    public class LibraryRepository : ILibraryRepository
    {
        private LibraryContext _context;
        private IPropertyMappingService _propertyMappingService;

        public LibraryRepository(LibraryContext context, IPropertyMappingService propertyMappingService)
        {
            _context = context;
            _propertyMappingService = propertyMappingService;
        }

        public Uid ValidateUser(string username, string password)
        {
            var results = _context.Users.Where(a => a.UserName == username && a.Password == Helpers.GenerateHash.encryptPassword(password));
            User result = null;
            if (results.Any())
            { result = (User)results.First(); }
            return (result != null) ? new Uid(result.Id.ToString(), result.UserType) : null;            
        }

        public void AddUser(User user)
        {
            user.Id = Guid.NewGuid();
            _context.Users.Add(user);
            // the repository fills the id (instead of using identity columns)
            if (user.Scores.Any())
            {
                foreach (var score in user.Scores)
                {
                    score.Id = Guid.NewGuid();
                }
            }
        }

        public void AddScoreForUser(Guid userId, Score score)
        {
            var user = GetUser(userId);
            if (user != null)
            {
                // if there isn't an id filled out (ie: we're not upserting),
                // we should generate one
                if (score.Id == Guid.Empty)
                {
                    score.Id = Guid.NewGuid();
                }
                if (_context.Scores.Any((a => a.UserId == userId)))
                {
                    IEnumerable<Score> previousScore = _context.Scores.Where(a => a.UserId == userId);
                    Parallel.ForEach(previousScore, (scr) =>
                    {
                        scr.LatestScore = false;
                    });
                }
                //later change imei to collection*****************************
                score.IMEI = user.IMEI;
                score.LatestScore = true;
                score.TimeStamp = System.DateTime.Now;
                user.Scores.Add(score);
            }
        }

        public void CompareScoreForUser(Guid userId, Score score)
        {            
            //Get the latest score for all the users in descending order. 
            var allLatestScores = _context.Scores.Where(a => a.LatestScore == true)
                                          .Where(b => (b.TimeStamp - DateTime.Now).TotalDays < 30)
                                          .OrderByDescending(c => c.InstanceScore).AsQueryable().ToList();
            Parallel.ForEach(allLatestScores, (records, ParallelLoopState) =>
            {
                if (records.Id == score.Id)
                {
                    Score temp = records;
                    var scoreToAdd = score;
                    scoreToAdd.ScoreComparison = (allLatestScores.IndexOf(records) + 1 + " / " + allLatestScores.Count);
                    AddScoreForUser(score.UserId, score);                   
                    ParallelLoopState.Break();
                }
            });
        }

        public bool UserExists(Guid userId)
        {
            return _context.Users.Any(a => a.Id == userId);
        }

        public bool UserExists(string username)
        {
            return _context.Users.Any(a => a.UserName == username);
        }

        public void DeleteUser(User user)
        {
            _context.Users.Remove(user);
        }

        public void DeleteScore(Score score)
        {
            _context.Scores.Remove(score);
        }

        public User GetUser(Guid userId)
        {
            return _context.Users.FirstOrDefault(a => a.Id == userId);
        }

        public PagedList<User> GetUsers(
            UserResourceParameters userResourceParameters)
        {
            var collectionBeforePaging =
                _context.Users.ApplySort(userResourceParameters.OrderBy,
                _propertyMappingService.GetPropertyMapping<UserDto, User>());

            //Search using search query (Firstname and Lastname)
            if (!string.IsNullOrEmpty(userResourceParameters.SearchQuery))
            {
                //trim & ignore casing
                var searhcQueryForWhereClause = userResourceParameters.SearchQuery
                    .Trim().ToLowerInvariant();
                collectionBeforePaging = collectionBeforePaging
                    .Where(a => a.FirstName.ToLowerInvariant().Contains(searhcQueryForWhereClause)
                    || a.LastName.ToLowerInvariant().Contains(searhcQueryForWhereClause)); 
            }

            //Filter suing firstname
            if (!string.IsNullOrEmpty(userResourceParameters.FirstName))
            {
                //trim & ignore casing
                var firstnameForWhereClause = userResourceParameters.FirstName
                    .Trim().ToLowerInvariant();
                collectionBeforePaging = collectionBeforePaging
                    .Where(a => a.FirstName.ToLowerInvariant() == firstnameForWhereClause);
            }

            //filter using lastname
            if (!string.IsNullOrEmpty(userResourceParameters.LastName))
            {
                //trim & ignore casing
                var lastnameForWhereClause = userResourceParameters.LastName
                    .Trim().ToLowerInvariant();
                collectionBeforePaging = collectionBeforePaging
                    .Where(a => a.LastName.ToLowerInvariant() == lastnameForWhereClause);
            }

            //filter using imei
            if (!string.IsNullOrEmpty(userResourceParameters.IMEI))
            {
                //trim & ignore casing
                var imeiForWhereClause = userResourceParameters.IMEI
                    .Trim().ToLowerInvariant();
                collectionBeforePaging = collectionBeforePaging
                    .Where(a => a.IMEI.ToLowerInvariant() == imeiForWhereClause);
            }

            return PagedList<User>.Create(collectionBeforePaging,
                userResourceParameters.PageNumber,
                userResourceParameters.PageSize);
            
        }

        public IEnumerable<User> GetUsers(IEnumerable<Guid> userIds)
        {
            return _context.Users.Where(a => userIds.Contains(a.Id))
                .OrderBy(a => a.FirstName)
                .OrderBy(a => a.LastName)
                .ToList();
        }

        public void UpdateUser(User user)
        {
            // no code in this implementation
        }

        public Score GetScoreForUser(Guid userId, Guid scoreId)
        {
            return _context.Scores
              .Where(b => b.UserId == userId && b.Id == scoreId).FirstOrDefault();
        }

        public PagedList<Score> GetScoresForUser(Guid userId, ScoresResourceParameters scoresResourceParameters)
        {
            var collectionBeforePaging = _context.Scores
                .Where(b => b.UserId == userId)
                .OrderBy(a => a.TimeStamp)
                .ThenBy(a => a.InstanceScore).AsQueryable();

            ////Search using search query (Timestamp)
            //if (!string.IsNullOrEmpty(scoresResourceParameters.SearchQuery))
            //{
                
            //    var searhcQueryForWhereClause = scoresResourceParameters.SearchQuery;
            //    collectionBeforePaging = collectionBeforePaging
            //        .Where(a => a.TimeStamp.ToString("dddd, MMM dd yyyy HH:mm:ss zzz").Contains(searhcQueryForWhereClause));
            //}

            //Filter suing InstanceScore
            //if (scoresResourceParameters.InstanceScore != null)
            //{                
            //    var instanceScoreForWhereClause = scoresResourceParameters.InstanceScore;                   
            //    collectionBeforePaging = collectionBeforePaging
            //        .Where(a => a.InstanceScore == instanceScoreForWhereClause);
            //}

            return PagedList<Score>.Create(collectionBeforePaging,
                scoresResourceParameters.PageNumber,
                scoresResourceParameters.PageSize);
        }

        public void UpdateScoreForUser(Score book)
        {
            // no code in this implementation
        }

        public bool Save()
        {
            return (_context.SaveChanges() >= 0);
        }
    }
}
