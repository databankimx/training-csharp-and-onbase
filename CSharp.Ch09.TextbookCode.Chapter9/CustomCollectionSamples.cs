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

namespace Chapter9
{
    class CustomCollectionSamples
    {
        static void Main(string[] args)
        {
            PersonCollection persons = new PersonCollection();

            persons.Add(new Person()
            {
                PersonId = 1,
                FName = "John",
                LName = "Smith"
            });

            persons.Add(new Person()
            {
                PersonId = 2,
                FName = "Jane",
                LName = "Doe"
            });

            persons.Add(new Person()
            {
                PersonId = 3,
                FName = "Bill Jones",
                LName = "Smith"
            });

            foreach (Person person in persons)
            {
                Debug.WriteLine(person.FName);
            }
        }
    }
}
