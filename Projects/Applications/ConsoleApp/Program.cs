using OnXap;
using System;

namespace ConsoleApp
{
    class app : OnXApplication
    {
        public app(): base(Environment.CurrentDirectory, () => "Data Source=localhost;Initial Catalog=TestWeb;Integrated Security=true;")// ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString)
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
