using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Linq;
using IMongoDatabase = MongoDB.Driver.IMongoDatabase;


namespace MongoDBBlog.Tester
{
    public class Startup
    {
        [BsonIgnoreExtraElements]
        internal class Student
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Class { get; set; }
            public int Age { get; set; }
            public IEnumerable<string> Subjects { get; set; }
        }

        

        static void Main(string[] args)
        {
            MainAsync().Wait();

            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }

        static async Task MainAsync()
        {

            var client = new MongoClient("mongodb://192.168.8.5");

            IMongoDatabase db = client.GetDatabase("schoool3");

            var collection = db.GetCollection<Student>("students2");
            var newStudents = CreateNewStudents();

            await collection.InsertManyAsync(newStudents);

            var filter = "{ FirstName: 'Gregor'}";

            await collection.Find(filter)
                .ForEachAsync(document => Console.WriteLine(document));
               await collection.Find(student => student.Age < 25 && student.FirstName != "Peter")
                 .ForEachAsync(student => Console.WriteLine(student.FirstName + " " + student.LastName));
        }

        private static IEnumerable<Student> CreateNewStudents()
        {
            var student1 = new Student
            {
                FirstName = "Gregor",
                LastName = "Felix",
                Subjects = new List<string>() {"English", "Mathematics", "Physics", "Biology"},
                Class = "JSS 3",
                Age = 23
            };

            var student2 = new Student
            {
                FirstName = "Machiko",
                LastName = "Elkberg",
                Subjects = new List<string> {"English", "Mathematics", "Spanish"},
                Class = "JSS 3",
                Age = 23
            };

            var student3 = new Student
            {
                FirstName = "Julie",
                LastName = "Sandal",
                Subjects = new List<string> {"English", "Mathematics", "Physics", "Chemistry"},
                Class = "JSS 1",
                Age = 25
            };

            var newStudents = new List<Student> {student1, student2, student3};

            return newStudents;
        }
    }
}