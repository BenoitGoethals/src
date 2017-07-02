using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using MongoDB;
using MongoDB.Driver;
using MongoDB.Linq;

namespace MongoDBBlog.Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            //Create a default mongo object.  This handles our connections to the database.
            //By default, this will connect to localhost, port 27017 which we already have running from earlier.
            var mongo = new Mongo("mongodb://192.168.8.5");
            mongo.Connect();

            //Get the blog database.  If it doesn't exist, that's ok because MongoDB will create it 
            //for us when we first use it. Awesome!!!
            var db = mongo.GetDatabase("blog");

            //Get the Post collection.  By default, we'll use the name of the class as the collection name. Again,
            //if it doesn't exist, MongoDB will create it when we first use it.
            var collection = db.GetCollection<Post>();

            //this deletes everything out of the collection so we can run this over and over again.
            collection.Remove(p => true);

            //Create posts to enter into the database.
            CreatePosts(collection);

            //count all the Posts
            var totalNumberOfPosts = collection.Count();

            //count only the Posts that have 1 comment
            var numberOfPostsWith1Comment = collection.Count(p => p.Comments.Count == 2);

            //find the titles of the posts that Jane commented on...
            var postsThatJaneCommentedOn = from p in collection.Linq()
                                           where p.Comments.Any(c => c.Email.StartsWith("Jane"))
                                           select p.Title;

            //find the titles and comments of the posts that have comments after January First.
            var postsWithJanuary1st = from p in collection.Linq()
                                      where p.Comments.Any(c => c.TimePosted > new DateTime(2010, 1, 1))
                                      select new { Title = p.Title, Comments = p.Comments };

            //find posts with less than 40 characters
            var postsWithLessThan10Words = from p in collection.Linq()
                                           where p.CharCount < 40
                                           select p;


            //get the total character count for all posts...
            var sum = Convert.ToInt32(collection.MapReduce()
                .Map(new Code(@"
                    function() {
                        emit(1, this.CharCount);
                    }"))
                .Reduce(new Code(@"
                    function(key, values) {
                        var sum = 0;
                        values.forEach(function(prev) {
                            sum += prev;
                        });
                        return sum;
                    }"))
                .Documents.Single()["value"]);

            //Using Linq to automatically build the above query. Awesome!!!
            var linqSum = (int)collection.Linq().Sum(p => p.CharCount);

            //Now imagine about doing this by hand...
            var stats = from p in collection.Linq()
                        where p.Comments.Any(c => c.Email.StartsWith("bob"))
                        group p by p.CharCount < 40 into g
                        select new
                        {
                            LessThan40 = g.Key,
                            Sum = g.Sum(x => x.CharCount),
                            Count = g.Count(),
                            Average = g.Average(x => x.CharCount),
                            Min = g.Min(x => x.CharCount),
                            Max = g.Max(x => x.CharCount)
                        };  
        }

        private static void CreatePosts(MongoDB.IMongoCollection<Post> collection)
        {
            var post = new Post()
            {
                Title = "My First Post",
                Body = "This isn't a very long post.",
                CharCount = 27,
                Comments = new List<Comment>
                {
                    { new Comment() { TimePosted = new DateTime(2010,1,1), Email = "bob_mcbob@gmail.com", Body = "This article is too short!" } },
                    { new Comment() { TimePosted = new DateTime(2010,1,2), Email = "Jane.McJane@gmail.com", Body = "I agree with Bob." } }
                }
            };

            //Save the post.  This will perform an upsert.  As in, if the post already exists, update it, otherwise insert it.
            collection.Save(post);

            //Get the first post that is not matching correctly...
            post = collection.Linq().First(x => x.CharCount != x.Body.Length);

            post.CharCount = post.Body.Length;

            //this will perform an update this time because we have already inserted it.
            collection.Save(post);

            post = new Post()
            {
                Title = "My Second Post",
                Body = "This still isn't a very long post.",
                CharCount = 34,
                Comments = new List<Comment>
                {
                    { new Comment() { TimePosted = new DateTime(2010,1,1), Email = "bob_mcbob@gmail.com", Body = "This isn't any better" } },
                }
            };

            //Save the post.  This will perform an upsert.  As in, if the post already exists, update it, otherwise insert it.
            collection.Save(post);

            post = new Post()
            {
                Title = "My Third Post",
                Body = "Ok, fine.  I'm writing a longer post so that Bob will leave me alone.",
                CharCount = 69,
                Comments = new List<Comment>
                {
                    { new Comment() { TimePosted = new DateTime(2010,1,1), Email = "bob_mcbob@gmail.com", Body = "Yeah, well, you are a terrible blogger." } },
                }
            };

            //Save the post.  This will perform an upsert.  As in, if the post already exists, update it, otherwise insert it.
            collection.Save(post);
        }
    }
}