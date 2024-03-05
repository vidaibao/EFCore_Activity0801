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

            //ListAllSalespeople();

            ShowAllSalespeopleUsingProjection();




            //ListPeople();

            //SearchPeople();

            //FilterAndPageResults();

        }
        //--------------------------------------------------------------------------------
        // Activity 9.1

        // 4
        private static void ShowAllSalespeopleUsingProjection()
        {
            using var db = new AdventureWorksContext(_optionsBuilder.Options);
            var salesPeople = db.SalesPeople
                                //.Include(s => s.BusinessEntity)  //This is not needed because the projection is used
                                //.ThenInclude(b => b.BusinessEntity)
                                .AsNoTracking()
                                // Using the projection, EF was able to interpret exactly what you needed and
                                // only queried for those results
                                .Select(x => new
                                {
                                    x.BusinessEntityId,
                                    x.BusinessEntity.BusinessEntity.FirstName,
                                    x.BusinessEntity.BusinessEntity.LastName,
                                    x.SalesQuota,
                                    x.SalesYtd,
                                    x.SalesLastYear
                                }).ToList();

            Console.WriteLine();
            Console.WriteLine("ShowAllSalespeopleUsingProjection:");
            foreach (var sp in salesPeople)
            {
                Console.WriteLine($"BID: {sp.BusinessEntityId} | Name: {sp.LastName}" +
                    $", {sp.FirstName} | Quota: {sp.SalesQuota} | " +
                    $"YTD Sales: {sp.SalesYtd} | SalesLastYear: {sp.SalesLastYear}");
            }
        }



        private static string GetSalesPersonDetails(SalesPerson sp)
        {
            return $"ID: {sp.BusinessEntityId}\t|TID: {sp.TerritoryId}\t\t|Quota:{sp.SalesQuota}\t"
                    + $"|Bonus: {sp.Bonus}\t|YTDSales: {sp.SalesYtd}\t|Name: \t"
                    + $"{sp?.BusinessEntity?.BusinessEntity?.FirstName ?? ""}, "
                    + $"{sp?.BusinessEntity?.BusinessEntity?.LastName ?? ""}";
        }

        // 72
        private static void ListAllSalespeople()
        {
            using var db = new AdventureWorksContext(_optionsBuilder.Options);
            var salesPeople = db.SalesPeople
                                .Include(s => s.BusinessEntity)
                                .ThenInclude(b => b.BusinessEntity)
                                .AsNoTracking().ToList();
            foreach (var salesPerson in salesPeople)
            {
                var person = db.People.AsNoTracking().FirstOrDefault(p => p.BusinessEntityId == salesPerson.BusinessEntityId);
                Console.WriteLine(GetSalesPersonDetails(salesPerson));
            }
        }


        // 384
        private static string GetSalesPersonDetails0(SalesPerson sp, Person p)
        {
            return $"ID: {sp.BusinessEntityId}\t|TID: {sp.TerritoryId}\t\t|Quota:{sp.SalesQuota}\t"
                    + $"|Bonus: {sp.Bonus}\t|YTDSales: {sp.SalesYtd}\t|Name: \t"
                    + $"{p?.FirstName ?? ""}, {p?.LastName ?? ""}";
        }

        private static void ListAllSalespeople0()
        {
            using var db = new AdventureWorksContext(_optionsBuilder.Options);
            var salesPeople = db.SalesPeople.AsNoTracking().ToList();
            foreach (var salesPerson in salesPeople)
            {
                var person = db.People.AsNoTracking().FirstOrDefault(p => p.BusinessEntityId == salesPerson.BusinessEntityId);
                Console.WriteLine(GetSalesPersonDetails0(salesPerson, person));
            }
        }













        //--------------------------------------------------------------------------------
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


