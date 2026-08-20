/*
 * Warning!
 *
 * This is the unedited code downloaded from the Textbook publisher
 * This code does not follow coding standards or best practices and may have bugs/errors
 *
 * Downloaded From:
 * https://media.wiley.com/product_ancillary/94/11186120/DOWNLOAD/MCSD%20Certification%20Code%20and%20Test%20Questions.zip
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.Ch06.TextbookCode.StaticAndInstanceDelegates
{
    class Person
    {
        public string Name;

        // A method that returns a string.
        public delegate string GetStringDelegate();

        // A static method.
        public static string StaticName()
        {
            return "Static";
        }

        // Return this instance's Name.
        public string GetName()
        {
            return Name;
        }

        // Variables to hold GetStringDelegates.
        public GetStringDelegate StaticMethod;
        public GetStringDelegate InstanceMethod;
    }
}
