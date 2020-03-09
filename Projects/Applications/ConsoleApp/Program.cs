using OnXap;
using System;
using System.Configuration;

namespace ConsoleApp
{
    class app : OnXApplication
    {
        public app(): base(Environment.CurrentDirectory, () => ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString)
        {

        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var app = new app();
                app.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.ReadKey();
        }
    }
}
