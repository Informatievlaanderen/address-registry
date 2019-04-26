namespace EF.MigrationHelper
{
    using System;
    
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Migrations!");
            Console.WriteLine("Execute from console: 'dotnet ef migrations add <MIGRATION_NAME> -p <PROJECT_NAME_TO_ADD_MIGRATION>'");
        }
    }
}
