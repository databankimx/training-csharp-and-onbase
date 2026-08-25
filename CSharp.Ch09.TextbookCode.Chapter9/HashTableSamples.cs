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
using System.Diagnostics;
using System.Collections;

namespace Chapter9
{
    class HashTableSamples
    {
        static void Main(string[] args)
        {
            Sample1();
        }

        static void Sample1()
        {
            Hashtable myHashtable = new Hashtable();

            myHashtable.Add(1, "one");
            myHashtable.Add("two", 2);
            myHashtable.Add(3, "three");

            Debug.WriteLine(myHashtable[1].ToString());
            Debug.WriteLine(myHashtable["two"].ToString());
            Debug.WriteLine(myHashtable[3].ToString());
        }
    }
}
