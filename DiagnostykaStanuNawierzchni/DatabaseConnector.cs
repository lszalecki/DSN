using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiagnostykaStanuNawierzchni
{
    public partial class DatabaseConnector : Form
    {

        private String host;
        private String port;
        private String user;
        private String password;
        private String databaseName;
        private DataTable data;
        private SQLiteConnection sqlCon;
        private NpgsqlConnection postgresConnection;


        public DatabaseConnector()
        {
            InitializeComponent();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }


        public Boolean connectToPostgres()
        {

            bool isConnected = false;

            if (!String.IsNullOrEmpty(textBoxHost.Text) && !String.IsNullOrEmpty(textBoxPort.Text) && !String.IsNullOrEmpty(textBoxDatabaseName.Text) && !String.IsNullOrEmpty(textBoxPassword.Text) && !String.IsNullOrEmpty(textBoxUser.Text))
            {
                host = textBoxHost.Text;
                port = textBoxPort.Text;
                databaseName = textBoxDatabaseName.Text;
                password = textBoxPassword.Text;
                user = textBoxUser.Text;

                String connstring = String.Format("Server={0};Port={1};" +
                 "User Id={2};Password={3};Database={4};SyncNotification={5};CommandTimeout={6};", host, port, user, password, databaseName, true, 0);

                try
                {

                    postgresConnection = new NpgsqlConnection(connstring);
                    postgresConnection.Open();
                    isConnected = true;

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    postgresConnection.Close();
                    isConnected = false;
                }

            }
            else
            {
                MessageBox.Show("Not all fields are filled");
                isConnected = false;
            }

            return isConnected;
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            bool c = connectToPostgres();

            if (c)
            {
                richTextBoxConnectionLog.AppendText("Connection to postgres successful." + Environment.NewLine);
            }
            else
            {
                richTextBoxConnectionLog.AppendText("Failed to connect." + Environment.NewLine);
            }
        }

        public Boolean checkIfExist(string tableName)
        {
            if (postgresConnection == null || postgresConnection.State != ConnectionState.Open)
            {
                connectToPostgres();
            }

            bool exist = false;
            NpgsqlDataReader dr = null;
            String sqlExist = null;

            
                sqlExist = "Select * from " + tableName + ";";
         

            try
            {

                NpgsqlCommand cmd = new NpgsqlCommand(sqlExist, postgresConnection);
                dr = cmd.ExecuteReader();

                if (dr.HasRows && dr.FieldCount != 0)
                {
                    if (dr != null && !dr.IsClosed)
                    {
                        dr.Close();
                    }

                    exist = true;
                }

                if (dr != null && !dr.IsClosed)
                {
                    dr.Close();
                }


            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e);

                if (dr != null && !dr.IsClosed)
                {
                    dr.Close();
                }
            }

            return exist;

        }

        private void getDataFromPostgres(NpgsqlConnection postgresConnection)
        {
            if(postgresConnection ==null || postgresConnection.State != ConnectionState.Open){
                connectToPostgres();
            }

            bool messfahrtExist = checkIfExist("messfahrt");

            if (messfahrtExist)
            {
                data = new DataTable();

                NpgsqlCommand cmd = new NpgsqlCommand("Select id, messfahrtnummer from messfahrt order by id;", postgresConnection);
                NpgsqlDataReader reader = cmd.ExecuteReader();
                data.Load(reader);

                reader.Close();
            }


        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            getDataFromPostgres(postgresConnection);

           bool sqliteExist = checkSQLite();

           if (!sqliteExist)
           {
               richTextBoxConnectionLog.AppendText("Sqlite database not exist. Create new database."+Environment.NewLine);
               createDatabaseSqlite();
               createTablesInSqlite(sqlCon);
           }

           if (sqlCon == null || sqlCon.State != ConnectionState.Open)
           {
               sqlCon = SetConnection();
           }

            if(data !=null && data.Rows.Count !=0)
            {
                using (var transaction = sqlCon.BeginTransaction())
                {

                    foreach (DataRow row in data.Rows)
                    {

                        string sql = "insert into dane (id ,messfahrt) values ('" + row[0].ToString() + "', '" + row[1].ToString() + "');";

                        using (var command = new SQLiteCommand(sqlCon))
                        {
                            command.CommandText = sql;
                            command.ExecuteNonQuery();
                        }

                    }

                    transaction.Commit();
                
                }

                sqlCon.Close();
              
            }

            richTextBoxConnectionLog.AppendText("Import finished."+Environment.NewLine);

        }


        private bool checkSQLite()
        {
            bool exist = false;

            String[] files = Directory.GetFiles(Environment.CurrentDirectory, "*.sqlite");

            if (files != null && files.Length != 0)
            {
                foreach (string file in files)
                {
                    if (file.Contains("DSNMessfahrt.sqlite"))
                    {
                        exist = true;
                    }
                }
            }

            return exist;
        }

        private void createDatabaseSqlite()
        {
            SQLiteConnection.CreateFile("DSNMessfahrt.sqlite");
        }

        private void createTablesInSqlite(SQLiteConnection sqlCon)
        {
            string sql3 = "create table dane (id varchar(32) , messfahrt varchar(32) )";

            if (sqlCon == null || sqlCon.State != ConnectionState.Open)
            {
                sqlCon = SetConnection();
            }

           SQLiteCommand sqlCmd = sqlCon.CreateCommand();
            sqlCmd.CommandText = sql3;
            sqlCmd.ExecuteNonQuery();
            sqlCmd.Dispose();
            sqlCon.Close();

        }

        private SQLiteConnection SetConnection()
        {
            string test = "Data Source=" + Environment.CurrentDirectory + "\\DSNMessfahrt.sqlite;Version=3;";
            sqlCon = new SQLiteConnection("Data Source=DSNMessfahrt.sqlite;Version=3;");
            sqlCon.Open();

            return sqlCon;
        }

        private void DatabaseConnector_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sqlCon != null && sqlCon.State == ConnectionState.Open)
            {
                sqlCon.Close();
            }

            if (postgresConnection != null && postgresConnection.State == ConnectionState.Open)
            {
                postgresConnection.Close();
            }
        }


     
    }
}
