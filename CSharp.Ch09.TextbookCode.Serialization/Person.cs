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
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace ConsoleApplication2
{
    [Serializable]
    public class Person : ISerializable
    {
        
        private int _id;        
        
        public string FirstName;
        
        public string LastName;

        public void SetId(int id)
        {
            _id = id;
        }

        public Person() { }

        public Person(SerializationInfo info, StreamingContext context)
        {
            FirstName = info.GetString("custom field 1");
            LastName = info.GetString("custom field 2");
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("custom field 1", FirstName);
            info.AddValue("custom field 2", LastName);
        }
    }
}
