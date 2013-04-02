using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiscUtil.Reflection;

namespace ExpressionTests
{
    class Program
    {
        static void Main(string[] args)
        {
            var p1 = new Person {FirstName = "first", LastName = "last"};
            var p2 = new Person { FirstName = "first", LastName = "last" };
            Console.WriteLine(ProperyCompare.Compare(p1, p2));
        }
    }

    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    
}
