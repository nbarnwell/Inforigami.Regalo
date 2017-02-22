using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inforigami.Regalo.Core.ConsoleLoggerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().Run();
        }

        private void Run()
        {
            var l = new ConsoleLogger { ShowDebugMessages = true };

            l.Debug(this, "Debug message with {0}", "argument");
            l.Info(this, "Information message with {0}", "argument");
            l.Warn(this, "Warning message with {0}", "argument");
            l.Error(this, "Error message with {0}", "argument");

            try
            {
                throw new Exception("Test Exception");
            }
            catch (Exception e)
            {
                l.Error(this, e, "Exception message with {0}", "argument");
            }

            Console.Write("Complete. Press ENTER to close.");
            Console.ReadLine();
        }
    }
}
