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
    class Person
    {
        public int PersonId { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
    }

    class PersonCollection : CollectionBase
    {
        public void Add(Person person)
        {
            List.Add(person);
        }

        public void Insert(int index, Person person)
        {
            List.Insert(index, person);
        }

        public Person this[int index]
        {
            get
            {
                return (Person)List[index];
            }

            set
            {
                List[index] = value;
            }
        }

        public void Remove(Person person)
        {
            List.Remove(person);
        }
    }
}
