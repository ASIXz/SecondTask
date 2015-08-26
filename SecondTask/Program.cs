using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;

namespace SecondTask
{
    public class position
    {
        public long latitude;
        public long longitude;
        public int accuracy;
        public DateTime time;

        public position() { }
        public position(long latitude, long longitude, int accuracy, DateTime time)
        {
            this.latitude = latitude;
            this.longitude = longitude;
            this.accuracy = accuracy;
            this.time = time;
        }
    }
    public class job
    {
        public DateTime time;
        public string description;
        public string phone;
        public string userId;

        public job() { }
        public job(DateTime time, string description, string phone, string userId)
        {
            this.time = time;
            this.description = description;
            this.phone = phone;
            this.userId = userId;
        }
    }
    public class item
    {
        public int id;
        public string firstName;
        public string lastName;
        [XmlArray("positionsHistory")]
        public List<position> positions = new List<position>();
        [XmlArray("jobHistory")]
        public List<job> jobs = new List<job>();
        public item() { }
        public item(int id, string firstName, string lastName)
        {
            this.id = id;
            this.firstName = firstName;
            this.lastName = lastName;
        }
    }

    [XmlRoot("items")]
    public class formatter
    {
        [XmlElement("item")]
        public List<item> items = new List<item>();
        public void writeToXML(string path)
        {

        }
        public void readFromXML(string path)
        {

        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            //generateXML();
            //readXML();
            Console.ReadKey();

        }
        static void generateXML()
        {
            item a = new item(15, "ASIX", "Z");
            a.jobs.Add(new job(new DateTime(213123), "hello my darling", "YAHOOOO!", "23423a"));
            a.jobs.Add(new job(new DateTime(5667), "hello my fri", "aaa!", "23sdf23a"));
            a.jobs.Add(new job(new DateTime(87689), "hello my yee", "sss!", "2a3423a"));
            a.positions.Add(new position(2342, 234234, 23423, new DateTime(345345)));
            a.positions.Add(new position(34345, 3645, 45646456, new DateTime(213333)));
            a.positions.Add(new position(5646, 5656346, 5567567, new DateTime(533345)));

            formatter test = new formatter();
            test.items.Add(a);
            XmlSerializer serializer = new XmlSerializer(typeof(item));
            FileStream itemStream = new FileStream("itemFile.xml", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            serializer.Serialize(itemStream, a);
            item b;
            itemStream.Close();
            itemStream = new FileStream("itemFile.xml", FileMode.Open, FileAccess.Read);
            b = (item)serializer.Deserialize(itemStream);
            b.id = 20;
            test.items.Add(b);
            StreamWriter itemsStream = new StreamWriter("items.xml");
            serializer = new XmlSerializer(typeof(formatter));
            serializer.Serialize(itemsStream, test);
        }
        static void readXML()
        {
            StreamReader st = new StreamReader("items.xml");
            XmlSerializer ser = new XmlSerializer(typeof(formatter));
            formatter test2 = (formatter)ser.Deserialize(st);
            st.Close();
        }
    }
}
