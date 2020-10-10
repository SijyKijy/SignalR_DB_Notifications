using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Server.Hubs;
using Server.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Server.Services
{
    public class DbRepository : IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly IHubContext<TestHub> _testHub;

        public DbRepository(IConfiguration configuration, IHubContext<TestHub> hub = null)
        {
            _configuration = configuration;
            _testHub = hub;

            SqlDependency.Start(configuration.GetConnectionString("TempDb")); // Cоздание сервиса, хранимки, а затем очереди в БД
        }

        #region SqlDependency
        // Регистрация SqlDependency
        public void Registration()
        {
            using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("TempDb"));

            using SqlCommand command = new SqlCommand("SELECT [UName], [Money] FROM [dbo].[Users]", connection) // Селектом (!) указываем нужную колонку для создания подписки
            {
                Notification = null
            };

            var dependency = new SqlDependency(command);
            dependency.OnChange += dependency_OnChange;

            if (connection.State != ConnectionState.Open)
                connection.Open();

            command.ExecuteReader(CommandBehavior.CloseConnection); // Подписка на события по заданной колонке
        }

        // Callback при изменении данных в таблице
        private void dependency_OnChange(object sender, SqlNotificationEventArgs e)
        {
            SqlDependency dependency = sender as SqlDependency;
            dependency.OnChange -= dependency_OnChange;

            Console.WriteLine($"[OnChange] {e.Type}");

            foreach (var lastEvent in GetLastEvents())
                _testHub.Clients.User(lastEvent.UserId).SendAsync("DbNotify", GetDataById(lastEvent.UserId));

            Registration(); // Повторная регистрация
        }
        #endregion

        public User GetDataById(string id)
        {
            using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("TempDb"));

            using SqlCommand command = new SqlCommand($"SELECT [UName], [Money] FROM [dbo].[Users] WHERE Id = {id}", connection)
            {
                Notification = null
            };

            if (connection.State != ConnectionState.Open)
                connection.Open();

            SqlDataAdapter da = new SqlDataAdapter(command);
            DataSet ds = new DataSet();
            da.Fill(ds);
            return new User
            {
                Id = id,
                Name = ds.Tables[0].Rows[0][0].ToString(),
                Money = Convert.ToInt32(ds.Tables[0].Rows[0][1])
            };
        }

        public List<Event> GetLastEvents()
        {
            using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("TempDb"));

            using SqlCommand command = new SqlCommand($"SELECT UserId, [Info] FROM [dbo].[Events] WHERE Date IN (SELECT TOP 1 Date FROM [dbo].[Events] ORDER BY Date DESC)", connection)
            {
                Notification = null
            };

            if (connection.State != ConnectionState.Open)
                connection.Open();

            SqlDataAdapter da = new SqlDataAdapter(command);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<Event> events = new List<Event>();
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                events.Add(new Event
                {
                    UserId = ds.Tables[0].Rows[i][0].ToString(),
                    Info = ds.Tables[0].Rows[i][1].ToString()
                });
            }

            return events;
        }

        public void Dispose()
        {
            SqlDependency.Stop(_configuration.GetConnectionString("TempDb"));
        }
    }
}
