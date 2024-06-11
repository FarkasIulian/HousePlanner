using System;

namespace FunctionApp1
{
    public class Demo
    {
        public Guid Id;
        public string FirstName;
        public string LastName;

        public override string ToString()
        {
            return FirstName + " " + LastName;
        }

    }
}