using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HW_XMLCurrencyProject
{
    class Program
    {
        private static Model1 db;

        static void Main(string[] args)
        {
            do
            {
                try
                {
                    CheckRates();
                    Thread.Sleep(5000);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                
            } while (true);
        }

        private static List<Currency> GetCurrency()
        {
            List<Currency> currencies = new List<Currency>();
            try
            {
                XDocument xdoc = XDocument.Load("http://www.nationalbank.kz/rss/rates.xml");

                foreach (XElement item in xdoc.Element("rss").Element("channel").Elements("item"))
                {
                    Currency currency = new Currency();
                    currency.Title = item.Element("title").Value;
                    currency.PubDate = DateTime.Now;
                    currency.CurrDescription = Convert.ToDouble(item.Element("description").Value.Replace(".", ","));

                    currencies.Add(currency);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.White;
            }

            return currencies;
        }

       
        private static List<Currency> GetCurrencyFromDB()
        {

            List<Currency> currencies = new List<Currency>();
            SqlConnection con = new SqlConnection();

            con.ConnectionString = @"data source=LAPTOP-NSMJD0QF\SQLEXPRESS;initial catalog=CurrencyBD;integrated security=True";

            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "Select * From Currency";
            cmd.Connection = con;

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            foreach (DataTable table in ds.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    Currency currency = new Currency();
                    currency.Title = row["Title"].ToString();
                    currency.CurrDescription = double.Parse(row["CurrDescription"].ToString());
                    currency.PubDate = Convert.ToDateTime(row["PubDate"].ToString());

                    currencies.Add(currency);
                }
            }

            return currencies;
        }

        private static bool CheckRates()
        {
            //1 получить курсы на текущее время
            List<Currency> currRates = GetCurrency();

            //2 получить курсы валют из базы
            // 2.1 данных нет в БД - просто добавить
            // 2.2 если много - взять курс с последней датой

            //2.
            List<Currency> existRates = GetCurrencyFromDB();

            //2.1
            if (!existRates.Any(a => a.PubDate.Date == DateTime.Now.Date))
            {

                db = new Model1();                              

                string connectionString = @"data source=LAPTOP-NSMJD0QF\SQLEXPRESS;initial catalog=CurrencyBD;integrated security=True";

                SqlConnection con = new SqlConnection(connectionString);

                var query = "Select * From Currency";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlParameter par = new SqlParameter
                {
                    ParameterName = "@title",
                    Value = currRates.Select(s => s.Title)
                };
                // добавляем параметр
                cmd.Parameters.Add(par);
                SqlParameter parDate = new SqlParameter
                {
                    ParameterName = "@pubDate",
                    Value = currRates.Select(s => s.PubDate)
                };
                // добавляем параметр
                cmd.Parameters.Add(parDate);
                SqlParameter parDesc = new SqlParameter
                {
                    ParameterName = "@CurrDescription",
                    Value = currRates.Select(s => s.CurrDescription)
                };
                // добавляем параметр
                cmd.Parameters.Add(parDesc);

            }
            //2.2
            else if (existRates.Count(a => a.PubDate.Date == DateTime.Now.Date) > 1)
            {
                //1 variant Max date
                DateTime maxDate = existRates.Max(m => m.PubDate);

                existRates = existRates.Where(w => w.PubDate == maxDate).ToList();

                ////2 variant Sort
                //existRates = existRates.OrderBy(o => o.PubDate).ToList();
                //existRates = existRates.Take(3).ToList();

                foreach (var curR in currRates)
                {
                    Currency cur = existRates.FirstOrDefault(f => f.Title == curR.Title);

                    if (cur.CurrDescription != curR.CurrDescription)
                    {
                        db = new Model1();

                        string connectionString = @"data source=LAPTOP-NSMJD0QF\SQLEXPRESS;initial catalog=CurrencyBD;integrated security=True";

                        SqlConnection con = new SqlConnection(connectionString);

                        var query = "Select * From Currency";

                        SqlCommand cmd = new SqlCommand(query, con);
                        cmd.CommandType = CommandType.StoredProcedure;
                        SqlParameter par = new SqlParameter
                        {
                            ParameterName = "@title",
                            Value = currRates.Select(s => s.Title)
                        };
                        // добавляем параметр
                        cmd.Parameters.Add(par);
                        SqlParameter parDate = new SqlParameter
                        {
                            ParameterName = "@pubDate",
                            Value = currRates.Select(s => s.PubDate)
                        };
                        // добавляем параметр
                        cmd.Parameters.Add(parDate);
                        SqlParameter parDesc = new SqlParameter
                        {
                            ParameterName = "@CurrDescription",
                            Value = currRates.Select(s => s.CurrDescription)
                        };
                        // добавляем параметр
                        cmd.Parameters.Add(parDesc);
                    }
                }
            }

            return true;

        }

    }
}
