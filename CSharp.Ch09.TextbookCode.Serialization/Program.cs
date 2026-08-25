/*
 * Warning!
 *
 * This is the unedited code downloaded from the Textbook publisher
 * This code does not follow coding standards or best practices and may have bugs/errors
 *
 * Downloaded From:
 * https://media.wiley.com/product_ancillary/94/11186120/DOWNLOAD/MCSD%20Certification%20Code%20and%20Test%20Questions.zip
 *
 * *Migration Note:
 *
 * Two real bugs were found and fixed here during the 2026 migration:
 *
 * 1. The XML section's "streamWriter" (writing Person.xml) was never closed before the
 *    very next lines tried to open that same file for reading via a separate FileStream.
 *    StreamWriter buffers its output and holds the file open until closed, so this could
 *    both leave Person.xml incompletely written AND throw IOException ("The process cannot
 *    access the file... because it is being used by another process") when the read
 *    attempt collided with the still-open write handle. Fixed by adding
 *    "streamWriter.Close();" immediately after the Serialize() call, before "fs" is opened.
 *
 * 2. The very last line originally read "stream.Close();", re-closing the ALREADY-CLOSED
 *    binary-serialization "stream" variable from earlier in Main() (a harmless no-op) while
 *    leaving "stream3" (the JSON read FileStream, opened just above) never closed at all, a
 *    genuine resource leak. This reads as a copy-paste variable-name mistake, elsewhere in
 *    this same file every other stream is correctly closed by its own matching variable
 *    name. Fixed by changing it to "stream3.Close();".
 *
 * 3. "fs" (the XML read FileStream) was also never closed at all, the same class of leak
 *    as #2 above, just less severe since nothing else in this file re-opens Person.xml
 *    afterward. Fixed by adding "fs.Close();" for consistency with every other stream here.
 *
 * See LectureNotes.md for further discussion of both.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Runtime.Serialization.Json;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {
            Person person = new Person();
            person.SetId(1);
            person.FirstName = "Joe";
            person.LastName = "Smith";

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream("Person.bin", FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, person);
            stream.Close();

            stream = new FileStream("Person.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
            Person person2 = (Person)formatter.Deserialize(stream);
            stream.Close();

            Person person3 = new Person();
            person3.SetId(1);
            person3.FirstName = "Joe";
            person3.LastName = "Smith";

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Person));
            StreamWriter streamWriter = new StreamWriter("Person.xml");
            xmlSerializer.Serialize(streamWriter, person);
            streamWriter.Close();

            XmlSerializer xmlSerializer2 = new XmlSerializer(typeof(Person));
            FileStream fs = new FileStream("Person.xml", FileMode.Open);
            Person person4 = (Person)xmlSerializer2.Deserialize(fs);
            fs.Close();

            Person person5 = new Person();
            person5.SetId(1);
            person5.FirstName = "Joe";
            person5.LastName = "Smith";

            Stream stream2 = new FileStream("Person.json", FileMode.Create);
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Person));
            ser.WriteObject(stream2, person);
            stream2.Close();

            Stream stream3 = new FileStream("Person.json", FileMode.Open);
            DataContractJsonSerializer ser2 = new DataContractJsonSerializer(typeof(Person));
            person = (Person)ser2.ReadObject(stream3);
            stream3.Close();

        }
    }
}
