using System;
using EFCore_DBLibrary;
using InventoryHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace EFCore_Activity0801
{
    class Program
    {
        private static IConfigurationRoot _configuration;
        private static DbContextOptionsBuilder<AdventureWorksContext> _optionsBuilder;

        static void Main(string[] args)
        {
            BuildOptions();

            //ListPeople();

            //SearchPeople();

            FilterAndPageResults();

        }

        private static void FilterAndPageResults()
        {
            Console.WriteLine("Please Enter the partial First or Last Name, or the Person Type to search for:");
            var result = Console.ReadLine();

            int pageSize = 10;
            for (int pageNumber = 0; pageNumber < 10; pageNumber++)
            {
                Console.WriteLine($"Page {pageNumber + 1}");
                FilteredAndPagedResult(result, pageNumber, pageSize);
            }
        }

        private static void FilteredAndPagedResult(string filter, int pageNumber, int pageSize)
        {
            using var db = new AdventureWorksContext(_optionsBuilder.Options);
            var searchItem = filter.ToLower();
            var people = db.People.Where(x =>  x.FirstName.ToLower().Contains(searchItem)
                                            || x.LastName.ToLower().Contains(searchItem)
                                            || x.PersonType.ToLower().Equals(searchItem))
                                            .Skip(pageNumber * pageSize)
                                            .Take(pageSize);
            foreach (var person in people)
            {
                Console.WriteLine($"{person.FirstName} {person.LastName}, {person.PersonType}");
            }
        }

        private static void SearchPeople()
        {
            Console.WriteLine("Please Enter the partial First or Last Name, or the Person Type to search for:");
            var result = Console.ReadLine();
            FilteredPeople(result);
        }

        private static void FilteredPeople(string filter)
        {
            using var db = new AdventureWorksContext(_optionsBuilder.Options);
            //var people = db.People.Where(x => x.FirstName.Contains(filter) || x.LastName.Contains(filter) || x.PersonType.Contains(filter));
            var searchItem = filter.ToLower();
            var people = db.People.Where(x =>  x.FirstName.ToLower().Contains(searchItem) 
                                            || x.LastName.ToLower().Contains(searchItem) 
                                            || x.PersonType.ToLower().Equals(searchItem));
            foreach (var person in people)
            {
                Console.WriteLine($"{person.FirstName} {person.LastName}, {person.PersonType}");
            }
        }

        private static void ListPeople()
        {
            Console.WriteLine("List People Then Order and Take");
            ListPeopleThenOrderAndTake();
            Console.WriteLine("Query People, order, then list and take");
            QueryPeopleOrderedToListAndTake();
        }

        private static void QueryPeopleOrderedToListAndTake()
        {
            using var db = new AdventureWorksContext(_optionsBuilder.Options);
            var query = db.People.OrderByDescending(x => x.LastName);
            var people = query.Take(10);
            foreach (var person in people)
            {
                Console.WriteLine($"{person.FirstName} {person.LastName}");
            }
        }

        private static void ListPeopleThenOrderAndTake()
        {
            using var db = new AdventureWorksContext(_optionsBuilder.Options);
            var people = db.People.ToList().OrderByDescending(x => x.LastName);
            foreach (var person in people.Take(10))
            {
                Console.WriteLine($"{person.FirstName} {person.LastName}");
            }
        }

        static void BuildOptions()
        {
            _configuration = ConfigurationBuilderSingleton.ConfigurationRoot;
            _optionsBuilder = new DbContextOptionsBuilder<AdventureWorksContext>();
            _optionsBuilder.UseSqlServer(_configuration.GetConnectionString("AdventureWorks"));
        }
    }
}


