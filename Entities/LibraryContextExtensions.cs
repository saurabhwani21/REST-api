using System;
using System.Collections.Generic;

namespace Library.API.Entities
{
    public static class LibraryContextExtensions
    {
        public static void EnsureSeedDataForContext(this LibraryContext context)
        {
            // first, clear the database.  This ensures we can always start 
            // fresh with each demo.  Not advised for production environments, obviously :-)

            context.Users.RemoveRange(context.Users);
            context.SaveChanges();

            // init seed data
            var users = new List<User>()
            {
                new User()
                {
                     Id = new Guid("25320c5e-f58a-4b1f-b63a-8ee07a840bdf"),
                     FirstName = "Stephen",
                     LastName = "King",
                     UserName = "one",
                     Password = "Horror",
                     IMEI = "123456",
                    //DateOfBirth = new DateTimeOffset(new DateTime(1947, 9, 21)),
                     Scores = new List<Score>()
                     {
                         new Score()
                         {
                             Id = new Guid("c7ba6add-09c4-45f8-8dd0-eaca221e5d93"),
                             TimeStamp = new DateTimeOffset(new DateTime(1947, 9, 21)),
                             InstanceScore = 4,
                             //Description = "The Shining is a horror novel by American author Stephen King. Published in 1977, it is King's third published novel and first hardback bestseller: the success of the book firmly established King as a preeminent author in the horror genre. "
                         }
                         //new Score()
                         //{
                         //    Id = new Guid("a3749477-f823-4124-aa4a-fc9ad5e79cd6"),
                         //    IMEI = "Device 2",
                         //    InstanceScore = 3,
                         //    //Description = "Misery is a 1987 psychological horror novel by Stephen King. This novel was nominated for the World Fantasy Award for Best Novel in 1988, and was later made into a Hollywood film and an off-Broadway play of the same name."
                         //}
                     }
                },
                new User()
                {
                     Id = new Guid("76053df4-6687-4353-8937-b45556748abe"),
                     FirstName = "George",
                     LastName = "RR Martin",
                     UserName = "two",
                     Password = "Fantasy",
                     IMEI = "One Plus 5",
                     //DateOfBirth = new DateTimeOffset(new DateTime(1948, 9, 20)),
                     Scores = new List<Score>()
                     {
                         new Score()
                         {
                             Id = new Guid("447eb762-95e9-4c31-95e1-b20053fbe215"),
                             TimeStamp = new DateTimeOffset(new DateTime(1948, 9, 20)),
                             InstanceScore = 7,
                             //Description = "A Game of Thrones is the first novel in A Song of Ice and Fire, a series of fantasy novels by American author George R. R. Martin. It was first published on August 1, 1996."
                         }
                     }
                },
                new User()
                {
                     Id = new Guid("412c3012-d891-4f5e-9613-ff7aa63e6bb3"),
                     FirstName = "Neil",
                     LastName = "Gaiman",
                     UserName = "three",
                     Password = "Fantasy",
                     IMEI = "Moto X",
                     //DateOfBirth = new DateTimeOffset(new DateTime(1960, 11, 10)),
                     Scores = new List<Score>()
                     {
                         new Score()
                         {
                             Id = new Guid("9edf91ee-ab77-4521-a402-5f188bc0c577"),
                             TimeStamp = new DateTimeOffset(new DateTime(1960, 11, 10)),
                             InstanceScore = 6,
                             //Description = "American Gods is a Hugo and Nebula Award-winning novel by English author Neil Gaiman. The novel is a blend of Americana, fantasy, and various strands of ancient and modern mythology, all centering on the mysterious and taciturn Shadow."
                         }
                     }
                },
                new User()
                {
                     Id = new Guid("578359b7-1967-41d6-8b87-64ab7605587e"),
                     FirstName = "Tom",
                     LastName = "Lanoye",
                     UserName = "four",
                     Password = "Various",
                     IMEI = "Micromax A1",
                     //DateOfBirth = new DateTimeOffset(new DateTime(1958, 8, 27)),
                     Scores = new List<Score>()
                     {
                         new Score()
                         {
                             Id = new Guid("01457142-358f-495f-aafa-fb23de3d67e9"),
                             TimeStamp = new DateTimeOffset(new DateTime(1958, 8, 27)),
                             InstanceScore = 5,
                             //Description = "Good-natured and often humorous, Speechless is at times a 'song of curses', as Lanoye describes the conflicts with his beloved diva of a mother and her brave struggle with decline and death."
                         }
                     }
                },
                new User()
                {
                     Id = new Guid("f74d6899-9ed2-4137-9876-66b070553f8f"),
                     FirstName = "Douglas",
                     LastName = "Adams",
                     UserName = "five",
                     Password = "Science fiction",
                     IMEI = "Dabba phone",
                     //DateOfBirth = new DateTimeOffset(new DateTime(1952, 3, 11)),
                     Scores = new List<Score>()
                     {
                         new Score()
                         {
                             Id = new Guid("e57b605f-8b3c-4089-b672-6ce9e6d6c23f"),  
                             TimeStamp = new DateTimeOffset(new DateTime(1952, 3, 11)),
                             InstanceScore = 1,
                             //Description = "The Hitchhiker's Guide to the Galaxy is the first of five books in the Hitchhiker's Guide to the Galaxy comedy science fiction 'trilogy' by Douglas Adams."
                         }
                     }
                },
                new User()
                {
                     Id = new Guid("a1da1d8e-1988-4634-b538-a01709477b77"),
                     FirstName = "Jens",
                     LastName = "Lapidus",
                     UserName = "six",
                     Password = "Thriller",
                     IMEI = "Samsung S8",
                     //DateOfBirth = new DateTimeOffset(new DateTime(1974, 5, 24)),
                     Scores = new List<Score>()
                     {
                         new Score()
                         {
                             Id = new Guid("1325360c-8253-473a-a20f-55c269c20407"),
                             TimeStamp = new DateTimeOffset(new DateTime(1974, 5, 24)),
                             InstanceScore = 4,
                             //Description = "Easy Money or Snabba cash is a novel from 2006 by Jens Lapidus. It has been a success in term of sales, and the paperback was the fourth best seller of Swedish novels in 2007."
                         }
                     }
                }
            };

            context.Users.AddRange(users);
            context.SaveChanges();
        }
    }
}
