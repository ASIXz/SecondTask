using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Data.SqlClient;
using System.Diagnostics;

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
        [XmlIgnore]
        SqlConnection cn = new SqlConnection("Data Source=(LocalDB)\\v11.0;AttachDbFilename=" + Environment.CurrentDirectory + "\\SecondTask.mdf;Integrated Security=True;Connect Timeout=30");

        public void close()
        {
            cn.Close();
        }
        public void writeToXML(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(formatter));
            StreamWriter itemStream = new StreamWriter(path);
            serializer.Serialize(itemStream, this);
            itemStream.Close();
        }
        public void readFromXML(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(formatter));
            StreamReader itemStream = new StreamReader(path);
            items = ((formatter)serializer.Deserialize(itemStream)).items;
            itemStream.Close();
        }
        public void writeToDataBase()
        {
            SqlCommand jobCM = new SqlCommand(string.Format("Insert Into Jobs(Time, Description, Phone, UserId, Item) Values(@Time, @Description, @Phone, @UserId, @Item)"),cn);
            SqlCommand pointCM = new SqlCommand(string.Format("Insert Into Positions(Latitude, Longitude, Accuracy, Time, Item) Values(@Latitude, @Longitude, @Accuracy, @Time, @Item)"), cn);
            SqlCommand itemCM = new SqlCommand(string.Format("Insert Into Items(Id, FirstName, SecondName) Values(@Id, @FirstName, @SecondName)"), cn);
            foreach(item a in items)
            {
                
                itemCM.Parameters.AddWithValue("@Id", a.id);
                itemCM.Parameters.AddWithValue("@FirstName", a.firstName);
                itemCM.Parameters.AddWithValue("@SecondName", a.lastName);
                try
                {
                    itemCM.ExecuteNonQuery();
                }
                catch(SqlException)
                {
                    Console.WriteLine("Duplication in item id ({0})", a.id);
                    continue;
                }
                itemCM.Parameters.Clear();
                foreach (job j in a.jobs)
                {
                    //for (int i = 0; i < 1000; i++) {
                        jobCM.Parameters.AddWithValue("@Time", j.time);
                        jobCM.Parameters.AddWithValue("@Description", j.description);
                        jobCM.Parameters.AddWithValue("@Phone", j.phone);
                        jobCM.Parameters.AddWithValue("@UserId", j.userId);
                        jobCM.Parameters.AddWithValue("@Item", a.id);
                        jobCM.ExecuteNonQuery();
                        jobCM.Parameters.Clear();
                    //}
                }
                foreach (position p in a.positions)
                {
                    
                   // for (int i = 0; i < 1000; i++) {
                    pointCM.Parameters.AddWithValue("@Latitude", p.latitude);
                    pointCM.Parameters.AddWithValue("@Longitude", p.longitude);
                    pointCM.Parameters.AddWithValue("@Accuracy", p.accuracy);
                    pointCM.Parameters.AddWithValue("@Time", p.time);
                    pointCM.Parameters.AddWithValue("@Item", a.id);
                    pointCM.ExecuteNonQuery();
                    pointCM.Parameters.Clear();
                 //   }
                }
            }
        }
        public void readFromDataBase()
        {
            items.Clear();
            SqlDataReader reader = new SqlCommand(string.Format("SELECT * FROM Items ORDER BY Id;SELECT * FROM Jobs ORDER BY Item;SELECT * FROM Jobs ORDER BY Item;"), cn).ExecuteReader();
            //читаем лист items
            while (reader.Read())
            {
                item newItem = new item();
                newItem.id = reader.GetInt32(0);
                newItem.firstName = reader.GetString(1);
                newItem.lastName = reader.GetString(2);
                items.Add(newItem);
            }

            int i;
            int lastid;

            //читаем лист jobs
            reader.NextResult();
            lastid = i = -1;
            while (reader.Read())
            {
                int k = reader.GetInt32(4);
                if (lastid != k)
                {
                    do ++i;
                    while (k != items[i].id);
                    lastid = reader.GetInt32(4);
                }
                items[i].jobs.Add(new job(reader.GetDateTime(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
            }


            //читаем лист positions
            reader.NextResult();
            lastid = i = -1;
            while (reader.Read())
            {
                if (lastid != reader.GetInt32(4))
                {
                    do i++;
                    while (reader.GetInt32(4) != items[i].id);
                    lastid = reader.GetInt32(4);
                }
                items[i].positions.Add(new position(reader.GetInt64(0),reader.GetInt64(1),reader.GetInt32(2),reader.GetDateTime(3)));
            }
            reader.Close();
        }
        public void clearBase()
        {
            SqlCommand jobCM = new SqlCommand(string.Format("Delete From Jobs"),cn);
            SqlCommand pointCM = new SqlCommand(string.Format("Delete From Positions"), cn);
            SqlCommand itemCM = new SqlCommand(string.Format("Delete From Items"), cn);
            jobCM.ExecuteNonQuery();
            pointCM.ExecuteNonQuery();
            itemCM.ExecuteNonQuery();
        }
        private static string generateRandomString()
        {
            Random r = new Random();
            int maxChars = r.Next(1, 30);
            StringBuilder stg = new StringBuilder(maxChars);
            for (int i = 0; i < maxChars; i++)
            {
                stg.Append((char)r.Next((int)'a', (int)'z'));
            }
            return stg.ToString();
        }
        public void generateRandomList(int maxItems, int maxJobs, int maxPoints)
        {
            items.Clear();
            Random r = new Random();
            int maxi = r.Next(1, maxItems);
            for (int i = 0; i < maxi; i++)
            {
                item newItem = new item();
                newItem.id = r.Next();
                newItem.firstName = generateRandomString();
                newItem.lastName = generateRandomString();

                int maxj = r.Next(1, maxJobs);
                for (int j = 0; j < maxj; j++)
                {
                    newItem.jobs.Add(new job(DateTime.Now,generateRandomString(),generateRandomString(),generateRandomString()));
                }

                int maxp = r.Next(1, maxPoints);
                for (int p = 0; p < maxp; p++)
                {
                    newItem.positions.Add(new position(r.Next(), r.Next(), r.Next(), DateTime.Now));
                }
                items.Add(newItem);
            }
        }
        public formatter()
        {
            cn.Open();
        }
    }
    class Program
    {
        static item a;
        static void Main(string[] args)
        {
            formatter a = new formatter();
            Console.WriteLine("Русиш консоле активировался!\nКричать Help чтобы получить от спящего соседа.");
            string command;
            do
            {
                command = Console.ReadLine();
                command.ToLower();
                switch (command)
                {
                    case "generate":
                        a.generateRandomList(10, 100, 100);
                        break;
                    case "clear":
                        a.clearBase();
                        break;
                    case "exit":
                        break;
                    case "to xml":
                        a.writeToXML("items.xml");
                        break;
                    case "to base":
                        a.writeToDataBase();
                        break;
                    case "from xml":
                        a.readFromXML("items.xml");
                        break;
                    case "from base":
                        a.readFromDataBase();
                        break;
                    case "help":
                        Console.WriteLine("generate\nclear\nexit\nto xml\nto base\nfrom xml\nfrom base\nhelp");
                        break;
                    case "cd1":
                        cd1();
                        break;
                    default:
                        Console.WriteLine("Чё?");
                        break;
                }
            }
            while (command != "exit");






            a.close();
            Console.WriteLine("END, press key to exit.");
            Console.ReadKey();
        }
        public void cd1()
        {

        }
    }
}
