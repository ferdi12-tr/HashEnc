using System;
using System.Text;
using System.Security.Cryptography;
using System.Data;
using System.Data.SqlClient;

namespace HashConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Data Source=DESKTOP-H100B8S;Initial Catalog=UsersDB;Integrated security=true";

            Console.WriteLine("Lütfen Kullanıcı Adınızı Giriniz: ");
            string username = Console.ReadLine();
            Console.WriteLine("Lütfen Parolanızı Giriniz: ");
            string password = Console.ReadLine();
            WriteToDB(connectionString, username, password);

            Console.WriteLine("Lütfen Sisteme Kayıtlı Kullanıcı Adınızı Giriniz: ");
            username = Console.ReadLine();
            Console.WriteLine("Lütfen Sisteme Kayıtlı Parolanızı Giriniz: ");
            password = Console.ReadLine();
            CheckHash(connectionString, username, password);
        }

        static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private static void WriteToDB(string connectionString, string username, string password)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand("INSERT INTO Users VALUES(@Username, @Password)"))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", ComputeSha256Hash(password));
                    command.Connection = connection;
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }

            }

            Console.WriteLine("{0} kullanıcısı başarılı bir şekilde veritabanına eklendi.", username);
        }

        static void CheckHash(string connectionString, string username, string password)
        {
            string queryString = "SELECT * FROM Users;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (ComputeSha256Hash(password) == (string) reader[1] && username == (string)reader[0])
                        {
                            Console.WriteLine("Sisteme Giriş Başarılı.");
                        }
                        else 
                        {
                            Console.WriteLine("Doğrulama Sağlanamadı, Lütfen Tekrar Deneyiniz.");
                         }
                    }
                }
            }
        }
    }
}

