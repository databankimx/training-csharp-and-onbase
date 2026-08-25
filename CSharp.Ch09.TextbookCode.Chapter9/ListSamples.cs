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

namespace Chapter9
{
    class ListSamples
    {
        static void Main(string[] args)
        {
            Sample1();
        }

        static void Sample1()
        {
            List<int> myList = new List<int>();

            myList.Add(1);
            myList.Add(2);
            myList.Add(3);
        }
    }
}
