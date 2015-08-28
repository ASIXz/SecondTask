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
            SqlConnection cn = new SqlConnection();
            cn.ConnectionString = "Data Source=(LocalDB)\\v11.0;AttachDbFilename=" + Environment.CurrentDirectory + "\\SecondTask.mdf;Integrated Security=True;Connect Timeout=30";
            cn.Open();
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
            SqlConnection cn = new SqlConnection();
            cn.ConnectionString = "Data Source=(LocalDB)\\v11.0;AttachDbFilename=" + Environment.CurrentDirectory + "\\SecondTask.mdf;Integrated Security=True;Connect Timeout=30";
            cn.Open();


            Stopwatch a = new Stopwatch();
            /*a.Start();
            for (int i = 0; i < 10; i++ )
            {

                SqlCommand jobCM = new SqlCommand(string.Format("SELECT * FROM Jobs ORDER BY Item"), cn);
                SqlDataReader reader = jobCM.ExecuteReader();

                reader.Close();
                SqlCommand pointCM = new SqlCommand(string.Format("SELECT * FROM Positions ORDER BY Item"), cn);
                reader = pointCM.ExecuteReader();

                reader.Close();
                SqlCommand itemCM = new SqlCommand(string.Format("SELECT * FROM Items ORDER BY Id"), cn);
                reader = itemCM.ExecuteReader();

                reader.Close();
            }
            a.Stop();*/
            a.Start();
             for (int i = 0; i < 10; i++)
             {

                // SqlCommand jobCM = new SqlCommand(string.Format("SELECT * FROM Positions INNER JOIN (Items INNER JOIN Jobs ON Jobs.Item = Items.Id) ON Positions.Item = Items.Id ORDER BY Id"), cn);
                 SqlCommand jobCM = new SqlCommand(string.Format(@"SELECT *
FROM Jobs INNER JOIN Items
   ON Jobs.Item = Items.Id JOIN Positions
   ON Positions.Item = Items.Id
ORDER BY ID ASC"), cn);
                 SqlDataReader reader = jobCM.ExecuteReader();

                 reader.Close();
             }
             a.Stop();



            Console.WriteLine(a.Elapsed);

            cn.Close();
        }
        public void clearBase()
        {
        
        }

    }
    class Program
    {
        static item a;
        static void Main(string[] args)
        {
            //generateXML();
           /* formatter a = new formatter();
            a.readFromXML("items.xml");
            a.writeToDataBase();*/

            formatter a = new formatter();
            a.readFromDataBase();
            

 


            /*using (SqlConnection cn = new SqlConnection())
            {
                cn.ConnectionString = "Data Source=(LocalDB)\\v11.0;AttachDbFilename=" + Environment.CurrentDirectory + "\\SecondTask.mdf;Integrated Security=True;Connect Timeout=30";
                cn.Open();
                string wrCommandJob = string.Format("Insert Into Jobs(Time, Description, Phone, UserId, Item) Values(@Time, @Description, @Phone, @UserId, @Item)");
                SqlCommand cm = new SqlCommand(wrCommandJob, cn);
                cm.Parameters.AddWithValue("@Time", a.jobs[0].time);
                cm.Parameters.AddWithValue("@Description", a.jobs[0].description);
                cm.Parameters.AddWithValue("@Phone", a.jobs[0].phone);
                cm.Parameters.AddWithValue("@UserId", a.jobs[0].userId);
                cm.Parameters.AddWithValue("@Item", 10);
                cm.ExecuteNonQuery();
             
                cn.Close();
            }*/
            //readXML();
            Console.ReadKey();
        }




        #region testing codes
        static void generateXML()
        {
           item a = new item(15, "ASIX", "Z");
            a.jobs.Add(new job(DateTime.Now, "hello my darling", "YAHOOOO!", "23423a"));
            a.jobs.Add(new job(DateTime.Now, "hello my fri", "aaa!", "23sdf23a"));
            a.jobs.Add(new job(DateTime.Now, "hello my yee", "sss!", "2a3423a"));
            a.positions.Add(new position(2342, 234234, 23423, DateTime.Now));
            a.positions.Add(new position(34345, 3645, 45646456, DateTime.Now));
            a.positions.Add(new position(5646, 5656346, 5567567, DateTime.Now));

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
        #endregion
    }
}
