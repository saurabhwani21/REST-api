using Library.API.Entities;
using Library.API.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Library.API.Services
{
    public class LibraryRepository : ILibraryRepository
    {
        private LibraryContext _context;

        public LibraryRepository(LibraryContext context)
        {
            _context = context;
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
                user.Scores.Add(score);
            }
        }

        public bool UserExists(Guid userId)
        {
            return _context.Users.Any(a => a.Id == userId);
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
            var collectionBeforePaging = _context.Users
                .OrderBy(a => a.FirstName)
                .ThenBy(a => a.LastName).AsQueryable();

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
