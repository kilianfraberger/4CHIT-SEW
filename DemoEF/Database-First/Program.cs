
// See https://aka.ms/new-console-template for more information

using Database;

using (var context = new DemoContext())
{
    foreach (var item in context.Tests)
    {
        Console.WriteLine(item);
    }
}

//dotnet ef dbcontext scaffold "Server=127.0.0.1;uid=root;pwd=insy;database=demo" MySql.EntityFrameworkCore