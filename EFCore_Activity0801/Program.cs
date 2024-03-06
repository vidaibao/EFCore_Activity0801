using System;
using EFCore_DBLibrary;
using InventoryHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq;
using EFCore_DBLibrary.DTOs;

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

            //ShowAllSalespeopleUsingProjection();

            // Anonymous class makes unexpected results likes id 220
            //GenerateSalesReportData();
            GenerateSalesReportDataToDTO();

            //ListPeople();

            //SearchPeople();

            //FilterAndPageResults();

        }
        //--------------------------------------------------------------------------------
        // Activity 9.2 AUTO MAPPER
        private static void GenerateSalesReportDataToDTO()
        {
            Console.WriteLine("What is the minimum amount of sales?");
            var input = Console.ReadLine();
            if (!decimal.TryParse(input, out decimal filter))
            {
                Console.WriteLine("Bad input");
                return;
            }

            using var db = new AdventureWorksContext(_optionsBuilder.Options);
            var salesReportData = db.SalesPeople.Select(sp => new SalesReportListingDto
            {
                BusinessEntityId = sp.BusinessEntityId,
                FirstName = sp.BusinessEntity.BusinessEntity.FirstName,
                LastName = sp.BusinessEntity.BusinessEntity.LastName,
                SalesYtd = sp.SalesYtd,
                Territories = sp.SalesTerritoryHistories.Select(y => y.Territory.Name),
                TotalOrders = sp.SalesOrderHeaders.Count(),
                TotalProductsSold = sp.SalesOrderHeaders
                                    .SelectMany(soh => soh.SalesOrderDetails)   // id disapeare 220 ???
                                    .Sum(sod => sod.OrderQty)
            }).Where(srd => srd.SalesYtd > filter)
                .OrderBy(srds => srds.LastName)
                .ThenBy(srds => srds.FirstName)
                .ThenByDescending(srds => srds.SalesYtd);
                //.ToList(); // force the system to evaluate the results
                // database call is not made until after you try to iterate the query, not as the query was built

            foreach (var srd in salesReportData)
            {
                Console.WriteLine(srd.ToString());
            }
        }











        //--------------------------------------------------------------------------------
        // Activity 9.1
        private static void GenerateSalesReportData()
        {
            Console.WriteLine("What is the minimum amount of sales?");
            var input = Console.ReadLine();
            if (!decimal.TryParse(input, out decimal filter))
            {
                Console.WriteLine("Bad input");
                return;
            }

            using var db = new AdventureWorksContext(_optionsBuilder.Options);
            var salesReportData = db.SalesPeople.Select(sp => new 
            {
                beid = sp.BusinessEntityId,
                sp.BusinessEntity.BusinessEntity.FirstName,
                sp.BusinessEntity.BusinessEntity.LastName,
                sp.SalesQuota,
                sp.SalesYtd,
                Territories = sp.SalesTerritoryHistories.Select(y => y.Territory.Name),
                OrderCount = sp.SalesOrderHeaders.Count(),   // get all the sales orders for the sales person
                /* Get all of the order details for each order header and then sum up the quantity of products
                 * sold across all of those order details */
                TotalProductsSold = sp.SalesOrderHeaders
                                    .SelectMany(soh => soh.SalesOrderDetails)   // id 220 ???
                                    .Sum(sod => sod.OrderQty)
            })  .Where(srd => srd.SalesYtd > filter)
                .OrderBy(srds => srds.LastName)
                .ThenBy(srds => srds.FirstName)
                .ThenByDescending(srds => srds.SalesYtd)
                .ToList();

            foreach (var srd in salesReportData)
            {
                Console.WriteLine($"{srd.beid}| {srd.LastName, -18}, {srd.FirstName, -10}| " +
                    $"YTD Sales: {srd.SalesYtd} |" +
                    $"{string.Join(',', srd.Territories)} |" +
                    $"Order Count: {srd.OrderCount} |" +
                    $"Products Sold: {srd.TotalProductsSold}");
            }
        }


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


