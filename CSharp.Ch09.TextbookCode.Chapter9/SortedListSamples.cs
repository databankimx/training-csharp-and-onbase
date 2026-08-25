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
using System.Collections;
using System.Diagnostics;

namespace Chapter9
{
    class SortedListSamples
    {
        static void Main(string[] args)
        {
            Sample1();
        }

        static void Sample1()
        {
            SortedList mySortedList = new SortedList();

            mySortedList.Add(3, "three");
            mySortedList.Add(2, "second");
            mySortedList.Add(1, "first");

            foreach (DictionaryEntry item in mySortedList)
            {
                Debug.WriteLine(item.Value);
            }
        }
    }
}
