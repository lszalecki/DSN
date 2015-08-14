using IniParser;
using IniParser.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace DiagnostykaStanuNawierzchni
{
    public partial class Form1 : Form
    {

        private XmlDocument doc;
        private FileIniDataParser parser = null;
        private IniData parsedData;

        private String numerJezdni;
        private String kierunek;
        private String pasRuchu;
        private String kategoriaDrogi;
        private String powiat;
        private String gmina;
        private String picturesPath;
        private String makroPath;
        private XmlDocument tp1aHeader;
        private XmlDocument tp1bHeader;
        private XmlDocument tp3Header;
        private XmlDocument fotoHeader;

        private SQLiteConnection sqlCon;
        private SQLiteConnection sqlConMessfahrt;
        private SQLiteCommand sqlCmd;
        private SQLiteDataAdapter DB;
        private DataSet DS = new DataSet();
        private DataTable DT = new DataTable();
        private DataTable loadedData;
        private String outputFolder;
        private Dictionary<String, String> filesList;

        private Dictionary<String, String> roadDictionary;
        private List<String> roadList;
        private XmlSchemaSet schemaDSN;
        private XmlSchemaSet schemaGML;
        private bool validDSN = true;
        private List<int> odleglosci;
        private String directoryPhoto;
 

        public Form1()
        {
            InitializeComponent();

            //Create an instance of a ini file parser
            parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";

            String configINI = Environment.CurrentDirectory + "\\config.ini";
            parsedData = parser.ReadFile(configINI);

            //This line gets the SectionData from the section "global"
            KeyDataCollection keyCol = parsedData["global"];

            numerJezdni = keyCol["numerJezdni"];
            kierunek = keyCol["kierunek"];
            pasRuchu = keyCol["pasRuchu"];
            kategoriaDrogi = keyCol["kategoriaDrogi"];
            picturesPath = keyCol["picturesPath"];
            makroPath = keyCol["makroPath"];
            powiat = keyCol["powiat"];
            gmina = keyCol["gmina"];

            if (Directory.Exists(Environment.CurrentDirectory + "\\Headers"))
            {
                String[] headerFiles = Directory.GetFiles(Environment.CurrentDirectory + "\\Headers", "*.xml");
                foreach(String headerPath in headerFiles){

                    if (headerPath.ToLower().Contains("tp1a"))
                    {
                        tp1aHeader = new XmlDocument();                 
                        tp1aHeader.Load(headerPath);
                    }
                    else if (headerPath.ToLower().Contains("tp1b"))
                    {
                        tp1bHeader = new XmlDocument();
                        tp1bHeader.Load(headerPath);
                    }
                    else if (headerPath.ToLower().Contains("tp3"))
                    {
                        tp3Header = new XmlDocument();
                        tp3Header.Load(headerPath);
                    }
                    else if(headerPath.ToLower().Contains("foto"))
                    {
                        fotoHeader = new XmlDocument();
                        fotoHeader.Load(headerPath);
                    }

                }
            }


            bool sqliteExist = checkSQLite();

            if (!sqliteExist)
            {
                createDatabaseSqlite();

                createTables(sqlCon);
            }
            else
            {
                if (sqlCon == null || sqlCon.State != ConnectionState.Open)
                {
                   sqlCon = SetConnection();
                }

                SQLiteCommand cmd = sqlCon.CreateCommand();
                cmd.CommandText = "SELECT count(*) FROM dane";
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                
                if(count !=0){
                    richTextBox1.SelectionFont = new Font(richTextBox1.SelectionFont, FontStyle.Bold);
                    richTextBox1.AppendText("SQLite database exist and contains data."+Environment.NewLine);
                }
                else
                {
                    richTextBox1.SelectionFont = new Font(richTextBox1.SelectionFont, FontStyle.Bold);
                    richTextBox1.AppendText("SQLite database exist with no data." + Environment.NewLine);
                }
               
            }

            if (Directory.Exists(Environment.CurrentDirectory + "\\xsd"))
            {
                String[] filesXSD = Directory.GetFiles(Environment.CurrentDirectory + "\\xsd");

                if (filesXSD != null && filesXSD.Length != 0)
                {
                    schemaDSN = new XmlSchemaSet();
                    schemaDSN.Add("http://www.gddkia.gov.pl/dsn/1.0.0", Environment.CurrentDirectory + "\\xsd\\dane_elementarne.xsd");

                    schemaGML = new XmlSchemaSet();
                    schemaGML.Add("http://www.opengis.net/gml", Environment.CurrentDirectory + "\\xsd\\gml\\gmlProfileDSN.xsd");

                    richTextBox1.SelectionFont = new Font(richTextBox1.SelectionFont, FontStyle.Bold);
                    richTextBox1.AppendText("XSD Schema loaded." + Environment.NewLine);
                }
                else
                {
                    richTextBox1.SelectionFont = new Font(richTextBox1.SelectionFont, FontStyle.Bold);
                    richTextBox1.AppendText("XSD Schema is not present." + Environment.NewLine);
                }
            }

        }

        private void createTables(SQLiteConnection sqlCon)
        {
            if (sqlCon == null || sqlCon.State != ConnectionState.Open)
            {
               sqlCon = SetConnection();
            }

            //string sql = "create table lokalizacja_siec (id varchar(32) ,numer_drogi varchar(20), pref varchar(20), nref varchar(20),  odleglosc int, dlugosc int, numer_jezdni int, pas_ruchu int, kierunek varchar(20) )";
            //sqlCmd = sqlCon.CreateCommand();
            //sqlCmd.CommandText = sql;
            //sqlCmd.ExecuteNonQuery();

            //string sql2 = "create table atrybuty (id varchar(32), wojewodztwo int, powiat int, oddzial varchar(200), rejon varchar(200), rodzaj_obszaru varchar(2))";
            //sqlCmd = sqlCon.CreateCommand();
            //sqlCmd.CommandText = sql2;
            //sqlCmd.ExecuteNonQuery();

            string sql3 = "create table dane (id varchar(32) ,numer_drogi varchar(20), pref varchar(20), nref varchar(20),  odleglosc int, dlugosc int, numer_jezdni int, pas_ruchu int, kierunek varchar(20), wojewodztwo int, powiat int, oddzial varchar(200), rejon varchar(200), rodzaj_obszaru varchar(2) ) ";
            sqlCmd = sqlCon.CreateCommand();
            sqlCmd.CommandText = sql3;
            sqlCmd.ExecuteNonQuery();
            sqlCmd.Dispose();
            sqlCon.Close();
        }

        private void toolStripMenuItemOpenFile_Click(object sender, EventArgs e)
        {
            openFileDialog1 = new OpenFileDialog();
            toolStripProgressBar1.Value = 0;
            filesList = new Dictionary<string, string>();

             DialogResult result = openFileDialog1.ShowDialog();

             if (result == DialogResult.OK)
             {
                 String fileName = openFileDialog1.FileName;
                 String fileNameWithoutPath = openFileDialog1.SafeFileName;
                 //toolStripStatusLabel1.Text = "Prepare to open file: " + fileNameWithoutPath;
                 DisplayData("Prepare to open file: " + fileNameWithoutPath);
                 filesList.Add(fileName, fileNameWithoutPath);

                 testStartButton();


             }
        }


        private bool checkSQLite()
        {
            bool exist = false;

            String[] files = Directory.GetFiles(Environment.CurrentDirectory, "*.sqlite");

            if (files !=null && files.Length !=0)
            {
                foreach(string file in files){
                    if (file.Contains("DSNLocalDatabase.sqlite"))
                    {
                        exist = true;
                    }
                }   
            }

            return exist;
        }

        private void createDatabaseSqlite()
        {  
                SQLiteConnection.CreateFile("DSNLocalDatabase.sqlite");      
        }

        private void importResultingData_Click(object sender, EventArgs e)
        {
            openFileDialog1 = new OpenFileDialog();

            DialogResult result = openFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                String fileName = openFileDialog1.FileName;
                String fileNameWithoutPath = openFileDialog1.SafeFileName;
                toolStripStatusLabel1.Text = "Opened file: " + fileNameWithoutPath;

                if (fileName != null && fileName.ToLower().EndsWith(".xml"))
                {
                    XmlDocument dataToImport = new XmlDocument();
                    dataToImport.Load(fileName);

                    importToDatabase(dataToImport);

                }
               
            }
        }

        private void importToDatabase(XmlDocument dataToImport)
        {
            if (sqlCon == null || sqlCon.State != ConnectionState.Open)
            {
                sqlCon = SetConnection();
            }

            if (dataToImport != null)
            {
                XmlNodeList list = dataToImport.GetElementsByTagName("dsn:odcinekDiagnostyczny");
                int maxCount = list.Count;
                int count = 0;
                toolStripProgressBar1.Maximum = maxCount;
                using (var transaction = sqlCon.BeginTransaction())
                {
                    foreach (XmlElement el in list)
                    {
                        string id = el.GetAttribute("gml:id");

                        XmlElement lokSiec = (XmlElement)el.FirstChild;
                        XmlElement atrybuty = (XmlElement)lokSiec.NextSibling;


                        //string sqlSiec = "insert into lokalizacja_siec (id ,numer_drogi, pref, nref,  odleglosc, dlugosc, numer_jezdni, pas_ruchu, kierunek ) values('" + id+"','" +
                        //                        lokSiec.GetAttribute("numerDrogi") + "', '" + lokSiec.GetAttribute("kodPRef") + "', '" + lokSiec.GetAttribute("kodNRef") + "'," + lokSiec.GetAttribute("odleglosc") + ", " + lokSiec.GetAttribute("dlugosc") + ", " + lokSiec.GetAttribute("numerJezdni") + ", " + lokSiec.GetAttribute("pasRuchu") + ", '" + lokSiec.GetAttribute("kierunek") + "')";
                        //using (var command = new SQLiteCommand(sqlCon))
                        //{
                        //    command.CommandText = sqlSiec;
                        //    command.ExecuteNonQuery();
                        //}

                        //XmlElement atrybuty = (XmlElement)lokSiec.NextSibling;
                        //string sqlAttr = "insert into atrybuty (id, wojewodztwo , powiat, oddzial, rejon, rodzaj_obszaru) values('" + id+"','"+
                        //                    atrybuty.GetAttribute("wojewodztwo") + "', " + atrybuty.GetAttribute("powiat") + ", '" + atrybuty.GetAttribute("oddzial") + "', '" + atrybuty.GetAttribute("rejon") + "','" + atrybuty.GetAttribute("rodzajObszaru") + "')";

                        //using (var command = new SQLiteCommand(sqlCon))
                        //{
                        //    command.CommandText = sqlAttr;
                        //    command.ExecuteNonQuery();
                        //}


                        string sql = "insert into dane (id ,numer_drogi, pref, nref,  odleglosc, dlugosc, numer_jezdni, pas_ruchu, kierunek, wojewodztwo , powiat, oddzial, rejon, rodzaj_obszaru ) values('" + id + "','" + lokSiec.GetAttribute("numerDrogi") + "', '" + lokSiec.GetAttribute("kodPRef") + "', '" + lokSiec.GetAttribute("kodNRef") + "'," + lokSiec.GetAttribute("odleglosc") + ", " + lokSiec.GetAttribute("dlugosc") + ", " + lokSiec.GetAttribute("numerJezdni") + ", " + lokSiec.GetAttribute("pasRuchu") + ", '" + lokSiec.GetAttribute("kierunek")+"','" + atrybuty.GetAttribute("wojewodztwo") + "', " + atrybuty.GetAttribute("powiat") + ", '" + atrybuty.GetAttribute("oddzial") + "', '" + atrybuty.GetAttribute("rejon") + "','" + atrybuty.GetAttribute("rodzajObszaru") + "')";

                        using (var command = new SQLiteCommand(sqlCon))
                        {
                            command.CommandText = sql;
                            command.ExecuteNonQuery();
                        }

                        count++;
                        toolStripProgressBar1.Value = count;
                    }


                    transaction.Commit();
                
                }

                toolStripStatusLabel1.Text = "Finished";
                if (sqlCon != null && sqlCon.State == ConnectionState.Open)
                {
                    sqlCon.Close();
                }
                
            }
        }

        private SQLiteConnection SetConnection()
        {
            string test = "Data Source=" + Environment.CurrentDirectory + "\\DSNLocalDatabase.sqlite;Version=3;";
            sqlCon = new SQLiteConnection("Data Source=DSNLocalDatabase.sqlite;Version=3;");
            sqlCon.Open();

            return sqlCon;
        }


        private SQLiteConnection SetConnectionMessfahrt()
        {
            string test = "Data Source=" + Environment.CurrentDirectory + "\\DSNMessfahrt.sqlite;Version=3;";
            sqlConMessfahrt = new SQLiteConnection("Data Source=DSNMessfahrt.sqlite;Version=3;");
            sqlConMessfahrt.Open();

            return sqlConMessfahrt;
        }


        private void ExecuteQuery(SQLiteConnection sqlCon, string txtQuery)
        {

            if (sqlCon == null || sqlCon.State != ConnectionState.Open)
            {
                if (sqlCon.DataSource.Contains("DSNLocalDatabase"))
                {
                    sqlCon = SetConnection();
                }
                else if (sqlCon.DataSource.Contains("DSNMessfahrt"))
                {
                    sqlCon = SetConnectionMessfahrt();
                }

            }

            sqlCmd = sqlCon.CreateCommand();
            sqlCmd.CommandText = txtQuery;
            sqlCmd.ExecuteNonQuery();
            //sqlCon.Close();
        }


        private DataTable LoadData(SQLiteConnection sqlCon, String sqlText)
        {
 
            if (sqlCon == null || sqlCon.State != ConnectionState.Open)
            {
                if (sqlCon.DataSource.Contains("DSNLocalDatabase"))
                {
                    sqlCon = SetConnection();
                }
                else if (sqlCon.DataSource.Contains("DSNMessfahrt"))
                {
                    sqlCon = SetConnectionMessfahrt();
                }
               
            }

            sqlCmd = sqlCon.CreateCommand();

            DB = new SQLiteDataAdapter(sqlText, sqlCon);
            DS.Reset();
            DB.Fill(DS);
            DT = DS.Tables[0];
            //Grid.DataSource = DT;
           // sqlCon.Close();

            return DT;
        }


        /*
         *   if messfahrt is not present in MessfahrtDatabase return null 
         */
        private String GenerateGmlId(String messfahrt)
        {

            String id = null;
            String sql = "Select id from dane where messfahrt = '"+messfahrt+"';";
            sqlConMessfahrt = SetConnectionMessfahrt();
            SQLiteCommand cmd = sqlConMessfahrt.CreateCommand();
            cmd.CommandText = sql;
            Object tempObject = cmd.ExecuteScalar();
            String temp = tempObject == null ? "" : tempObject.ToString();

            if (temp != null && temp.Length !=0)
            {
                switch(temp.Length)
                {
                    case 1:
                        {
                            id = "ID00000" + temp;
                            break;
                        }
                    case 2:
                        {
                            id = "ID0000" + temp;
                            break;
                        }
                    case 3:
                        {
                            id = "ID000" + temp;
                            break;
                        }
                    case 4:
                        {
                            id = "ID00" + temp;
                            break;
                        }
                    case 5:
                        {
                            id = "ID0" + temp;
                            break;
                        }
                    case 6:
                        {
                            id = "ID" + temp;
                            break;
                        }
                }
            }

            return id;
        }

        private String increaseGmlID(String id)
        {
            if (id != null) {

                String test = id.Replace("ID", "");
                Int32 temp = Int32.Parse(test);
                temp++;


                switch (temp.ToString().Length)
                {
                    case 1:
                        {
                            id = "ID00000" + temp;
                            break;
                        }
                    case 2:
                        {
                            id = "ID0000" + temp;
                            break;
                        }
                    case 3:
                        {
                            id = "ID000" + temp;
                            break;
                        }
                    case 4:
                        {
                            id = "ID00" + temp;
                            break;
                        }
                    case 5:
                        {
                            id = "ID0" + temp;
                            break;
                        }
                    case 6:
                        {
                            id = "ID" + temp;
                            break;
                        }
                }
            }
           

            return id;
        }


        private void openXmlFile(String fileName)
        {
            if (fileName != null && fileName.ToLower().EndsWith(".xml"))
            {
                DisplayData("Open file: " + fileName);

                String[] splited = fileName.Split('\\');
                String messfahrt = splited[splited.Length - 2];

                DisplayData("Generate GML ID...");
                String gmlID = GenerateGmlId(messfahrt);

                
                doc = new XmlDocument();
                doc.Load(fileName);
                String type = checkXmlType(doc);


                convertXmlFile(doc, type, fileName, gmlID);
            }    

        }

        private String checkXmlType(XmlDocument doc)
        {
            String type = null;
            if (doc != null)
            {
                if (doc.FirstChild.NextSibling.Name.ToLower().Contains("RohdatenTP1aNetz".ToLower()) || doc.FirstChild.NextSibling.Name.ToLower().Contains("RohdatenTP1aGeo".ToLower()))
                {
                    type = "TP1a";

                }
                else if (doc.FirstChild.NextSibling.Name.ToLower().Contains("RohdatenTP1bNetz".ToLower()) || doc.FirstChild.NextSibling.Name.ToLower().Contains("RohdatenTP1bGeo".ToLower()))
                {
                    type = "TP1b";

                }
                else if (doc.FirstChild.NextSibling.Name.ToLower().Contains("RohdatenTP3Netz".ToLower()) || doc.FirstChild.NextSibling.Name.ToLower().Contains("RohdatenTP3Geo".ToLower()))
                {
                    type = "TP3";
                }
            }           

            return type;
        }


        /*
         *  if nodeElement is null than header is getting from all body of one document
         *  else 
         *  header is getting from nodeElement
         */

        private XmlNodeList getHeader(XmlDocument doc, XmlElement nodeElement){

            XmlNodeList header = null;

            if (nodeElement != null && doc !=null)
            {
                header = nodeElement.GetElementsByTagName("ZEBHeader"); 
            }
            else
            {
                header = doc.GetElementsByTagName("ZEBHeader"); 
            }


            return header;
        }


        private void saveFile(XmlDocument doc, String path, String fileName, String type)
        {
            
            if(path.EndsWith("\\"))
            {
                if (!Directory.Exists(path + "PP-Nx"))
                {
                    Directory.CreateDirectory(path + "PP-Nx");
                }

                if (!Directory.Exists(path + "PP-Ny"))
                {
                    Directory.CreateDirectory(path + "PP-Ny");
                }

                if (!Directory.Exists(path + "PP-I"))
                {
                    Directory.CreateDirectory(path + "PP-I");
                }

               

                switch(type)
                {
                    case "TP1a":
                        {
                            int i=1;
                            while(File.Exists(path + "PP-Nx\\" + fileName)){
                                if (fileName.Substring(fileName.Length - 6, 2).Contains("Nx"))
                                {
                                    fileName = fileName.Insert(fileName.Length - 4, "_" + i);
                                }
                                else
                                {
                                    fileName = fileName.Substring(0, fileName.Length - 6) + "_" + i + ".xml";

                                }
                                i++;
                            }

                            doc.Save(path + "PP-Nx\\" + fileName);
                            break;
                        }
                    case "TP1b":
                        {
                            int i = 1;
                            while (File.Exists(path + "PP-Ny\\" + fileName))
                            {
                                if (fileName.Substring(fileName.Length - 6, 2).Contains("Ny"))
                                {
                                    fileName = fileName.Insert(fileName.Length - 4, "_" + i);
                                }
                                else
                                {
                                    fileName = fileName.Substring(0, fileName.Length - 6) + "_" + i + ".xml";

                                }
                                i++;
                            }

                            doc.Save(path + "PP-Ny\\" + fileName);
                            break;
                        }
                    case "TP3":
                        {
                            int i = 1;
                            while (File.Exists(path + "PP-I\\" + fileName))
                            {
                                if (fileName.Substring(fileName.Length - 6, 2).Contains("I_"))
                                {
                                    fileName = fileName.Insert(fileName.Length - 4, "_" + i);
                                }
                                else
                                {
                                    fileName = fileName.Substring(0, fileName.Length - 6) + "_" + i + ".xml";
                                }
                                i++;
                            }

                            doc.Save(path + "PP-I\\" + fileName);
                            break;
                        }
                }

                
            }
            else
            {
                if (!Directory.Exists(path + "\\" + "PP-Nx"))
                {
                    Directory.CreateDirectory(path + "\\" + "PP-Nx");
                }

                if (!Directory.Exists(path + "\\" + "PP-Ny"))
                {
                    Directory.CreateDirectory(path + "\\" + "PP-Ny");
                }

                if (!Directory.Exists(path + "\\" + "PP-I"))
                {
                    Directory.CreateDirectory(path + "\\" + "PP-I");
                }

                switch (type)
                {
                    case "TP1a":
                        {
                            int i = 1;

                            while (File.Exists(path + "\\" + "PP-Nx\\" + fileName))
                            {

                                if (fileName.Substring(fileName.Length - 6, 2).Contains("Nx"))
                                {
                                    fileName = fileName.Insert(fileName.Length - 4, "_" + i);
                                }
                                else
                                {
                                    fileName = fileName.Substring(0, fileName.Length - 6) + "_" + i + ".xml";

                                }
                                
                                i++;
                            }

                            doc.Save(path + "\\" + "PP-Nx\\" + fileName);
                            break;
                        }
                    case "TP1b":
                        {
                            int i = 1;
                            while (File.Exists(path + "\\" + "PP-Ny\\" + fileName))
                            {
                                if (fileName.Substring(fileName.Length - 6, 2).Contains("Ny"))
                                {
                                    fileName = fileName.Insert(fileName.Length - 4, "_" + i);
                                }
                                else
                                {
                                    fileName = fileName.Substring(0, fileName.Length - 6) + "_" + i + ".xml";

                                }
                                i++;
                            }

                            doc.Save(path + "\\" + "PP-Ny\\" + fileName);
                            break;
                        }
                    case "TP3":
                        {
                            int i = 1;
                            while (File.Exists(path + "\\" + "PP-I\\" + fileName))
                            {
                               
                                if (fileName.Substring(fileName.Length - 6, 2).Contains("I_"))
                                {
                                    fileName = fileName.Insert(fileName.Length - 4, "_" + i);
                                }
                                else
                                {
                                    fileName = fileName.Substring(0, fileName.Length - 6)+"_"+i+".xml";
                                    
                                }
                                i++;
                            }

                            doc.Save(path + "\\" + "PP-I\\" + fileName);
                            break;
                        }
                }
            }
            
        }


        private String createFileName(String streetClass, String streetNr, String wojewodztwo, String type)
        {
            //  create file name with specification DSN
            String extension = null;
            String finalStreet = null;
            if (!String.IsNullOrEmpty(streetClass) && !String.IsNullOrEmpty(streetNr))
            {
                if (streetClass.ToUpper().Equals("L"))
                {
                    streetClass = kategoriaDrogi;
                }

                if (streetNr.Length == 1)
                {
                    finalStreet = "____" + streetClass + streetNr + "__";
                }
                else if (streetNr.Length == 2)
                {
                    finalStreet = "____" + streetClass + streetNr + "_";
                }
                else if (streetNr.Length == 3)
                {
                    finalStreet = "____" + streetClass + streetNr;
                }

            }
            else if (String.IsNullOrEmpty(streetClass) && !String.IsNullOrEmpty(streetNr))
            {
                if (streetNr.Length == 1)
                {
                    finalStreet = "_____" + streetNr + "__";
                }
                else if (streetNr.Length == 2)
                {
                    finalStreet = "_____" + streetNr + "_";
                }
                else if (streetNr.Length == 3)
                {
                    finalStreet = "_____" + streetNr;
                }
            }

            String kier = null;
            if (kierunek.ToLower().Equals("zgodnie"))
            {
                kier = "z";
            }
            else
            {
                kier = "p";
            }

            switch(type)
            {
                case "TP1a":
                    {
                        extension = "_PP-Nx.xml";
                        break;
                    }
                case "TP1b":
                    {
                        extension = "_PP-Ny.xml";
                        break;
                    }
                case "TP3":
                    {
                        extension = "_PP-I_.xml";
                        break;
                    }
            }

            String fileNameFinal = "S" + wojewodztwo + "_" + finalStreet + "_" + numerJezdni + pasRuchu + kier + extension;

            return fileNameFinal;
        }


        private String RenamePictureFile(String path, String fileName)
        {
            String newName = null;
            String folder = null;

            if (!String.IsNullOrEmpty(path) && !String.IsNullOrEmpty(fileName))
            {
                String[] splitedName = fileName.Split('\\');
                folder = splitedName[0];
                String fNameOld = splitedName[2];

                String[] splitedOldName = fNameOld.Split('_');
                String prefixName = splitedOldName[0].Replace("B", "P");
                newName = prefixName + "_" + splitedOldName[1] + "_" + splitedOldName[2];

                if (Directory.Exists(path+"\\"+folder+"\\"))
                {
                    DirectoryInfo d = new DirectoryInfo(@path + "\\" + folder + "\\");
                    
                    FileInfo[] infos = d.GetFiles(fNameOld);
                    if (infos != null && infos.Length !=0)
                    {
                        FileInfo file = infos[0];
                        File.Move(file.FullName, file.FullName.ToString().Replace(fNameOld, newName));
                    }

                }
            }

            return folder+"\\"+ newName;
        }


        private Boolean ValidateDsnXML(XmlDocument doc)
        {
            validDSN = true;

            DisplayData("Begin validate xml... ");

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.Schemas = schemaDSN;

            settings.ValidationEventHandler += dsnValidationEventHandler;

            XmlReader reader = XmlReader.Create(new StringReader(doc.OuterXml), settings);
            //XmlReader reader = XmlReader.Create(doc.OuterXml, settings);

            while ( reader.Read() );

            //if ( validDSN )
            //{

            //    Console.WriteLine("Document is valid");
                
            //}  // end if

                //valid = true;
                reader.Close();


            return validDSN;
        }

        private void dsnValidationEventHandler(object sender, ValidationEventArgs e)
        {
            

            if (e.Severity == XmlSeverityType.Warning)
            {
                //Console.Write("WARNING: ");
                //Console.WriteLine(e.Message);
                DisplayData("WARNING: " + e.Message);
                validDSN = false;
            }
            else if (e.Severity == XmlSeverityType.Error)
            {
                //Console.Write("ERROR: ");
                //Console.WriteLine(e.Message);
                DisplayData("ERROR: " + e.Message+"   line nr = "+e.Exception.LineNumber+"    line pos = "+e.Exception.LinePosition);
                validDSN = false;
            }
          
        }


        private void convertXmlFile(XmlDocument doc, String type, String filePath, String gmlID)
        {

            DisplayData("Start process file: "+ filePath);
            String[] filePathSplited = filePath.Split('\\');
            String fileName = filePathSplited[filePathSplited.Length - 1];
            String path = filePath.Replace(fileName, "");

            String streetClass = null;
            String streetNr = null;
            String wojewodztwo = null;


            XmlDocument newDoc = new XmlDocument();
            XmlDeclaration xmlDeclaration = newDoc.CreateXmlDeclaration("1.0", "UTF-8", null);

            switch(type){
                case "TP1a":
                    {
                            XmlElement rootOld = doc.DocumentElement;

                            XmlElement rootNew = newDoc.CreateElement("dsn", "daneElementarnePPNxSiec", "http://www.gddkia.gov.pl/dsn/1.0.0");
                            rootNew.SetAttribute("xmlns:dsn", "http://www.gddkia.gov.pl/dsn/1.0.0");
                            rootNew.SetAttribute("xmlns:gml", "http://www.opengis.net/gml");
                            rootNew.SetAttribute("xmlns:sch", "http://www.ascc.net/xml/schematron");
                            rootNew.SetAttribute("xmlns:xlink", "http://www.w3.org/1999/xlink");
                            rootNew.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
                            rootNew.SetAttribute("schemaLocation", "http://www.w3.org/2001/XMLSchema-instance", "http://www.gddkia.gov.pl/dsn/1.0.0/dane_elementarne.xsd");
                            rootNew.SetAttribute("uwaga", rootOld.GetAttribute("Bemerkung") == null ? "" : rootOld.GetAttribute("Bemerkung"));
                            rootNew.SetAttribute("rodzaj", "sieciowe");
                            // rootNew.SetAttribute("cecha", rootOld.GetAttribute("Merkmal") == null ? "" : rootOld.GetAttribute("Merkmal"));
                            rootNew.SetAttribute("cecha", "rownosc podluzna");
                            rootNew.SetAttribute("dataUtworzenia", rootOld.GetAttribute("Erstelldatum") == null ? "" : rootOld.GetAttribute("Erstelldatum"));
                            newDoc.AppendChild(xmlDeclaration);
                            newDoc.AppendChild(rootNew);

                            XmlNodeList pomiaryList = doc.GetElementsByTagName("ZEBMessstrecke");
                            int przejazdCount = 0;
                            int headerCount = 0;

                            foreach (XmlElement zebMessstrecke in pomiaryList)
                            {

                                XmlElement pomiar = newDoc.CreateElement(rootNew.Prefix, "przejazdPomiarowy", rootNew.NamespaceURI );
                                if(przejazdCount == 0){
                                    XmlAttribute t = newDoc.CreateAttribute("gml", "id", "http://www.opengis.net/gml");
                                    t.Value = gmlID;
                                    pomiar.SetAttributeNode(t);
                                    przejazdCount++;
                                }
                               

                                rootNew.AppendChild(pomiar);

                            XmlElement headerNode =null;

                            if (tp1aHeader != null)
                            {
                                XmlNodeList hList = tp1aHeader.GetElementsByTagName("dsn:naglowekPrzejazduPomiarowego");
                                if(hList !=null && hList.Count !=0){
                                    headerNode = (XmlElement)hList[0];
                                    XmlAttribute testA = headerNode.Attributes["gml:id"];
                                    if (headerCount == 0 && headerNode.Attributes["gml:id"] != null)
                                    {
                                         gmlID = increaseGmlID(gmlID);
                                         headerNode.Attributes["gml:id"].Value = gmlID;
                                         headerCount++;
                                    }
                                    else
                                    {
                                        if (headerNode.Attributes["gml:id"] != null)
                                        {
                                            headerNode.RemoveAttribute("id", "http://www.opengis.net/gml");
                                        }                      
                                    }
                                   

                                   XmlNodeList liMetadane = headerNode.GetElementsByTagName("dsn:metadane");
                                    if(liMetadane !=null){
                                        XmlElement metadane = (XmlElement)liMetadane[0];
                                        wojewodztwo = metadane.GetAttribute("wojewodztwo");
                                    }

                                   // pomiar.AppendChild(headerNode);
                                  XmlNode hNode = newDoc.ImportNode(headerNode, true);
                                  pomiar.AppendChild(hNode);
                                }
                                
                            }
                            else
                            {

                                // get header and paste to new XML
                                XmlNodeList header = getHeader(doc, null);
                                if (header != null)
                                {
                                    headerNode = newDoc.CreateElement("dsn", "naglowekPrzejazduPomiarowego", "dsn");
                                    pomiar.AppendChild(headerNode);


                                    // copy bildparameter
                                    XmlNodeList list = zebMessstrecke.GetElementsByTagName("Bildparameter");
                                    foreach (XmlElement node in list)
                                    {
                                        XmlElement bp = newDoc.CreateElement("dsn", "parametryZdjecia", "dsn");
                                        bp.SetAttribute("nosnikDanych", node.GetAttribute("Datentraeger"));
                                        headerNode.AppendChild(bp);

                                        if (node.HasChildNodes)
                                        {
                                            XmlNodeList listKamera = node.GetElementsByTagName("Kamera");
                                            foreach (XmlElement el in listKamera)
                                            {
                                                XmlElement kameraElement = newDoc.CreateElement("dsn", "kamera", "dsn");
                                                kameraElement.SetAttribute("formatZdjecia", el.GetAttribute("Bildformat"));
                                                kameraElement.SetAttribute("rozdzielczoscX", el.GetAttribute("PixelH"));
                                                kameraElement.SetAttribute("rozdzielczoscY", el.GetAttribute("PixelV"));
                                                kameraElement.SetAttribute("nr", el.GetAttribute("Nr"));
                                                kameraElement.SetAttribute("katalogGlowny", el.GetAttribute("RootVerzeichnis"));
                                                kameraElement.SetAttribute("nazwa", el.GetAttribute("Name"));

                                                bp.AppendChild(kameraElement);


                                                //TODO
                                                XmlElement kamPos = newDoc.CreateElement("dsn", "pozycja", "dsn");
                                                kamPos.SetAttribute("X0", "878");
                                                kamPos.SetAttribute("Y0", "179");
                                                kamPos.SetAttribute("Z0", "2729");
                                                kamPos.SetAttribute("alpha", "- 0.0000");
                                                kamPos.SetAttribute("ny", "1.4504");
                                                kamPos.SetAttribute("kappa", "0.0070");
                                                kamPos.SetAttribute("fx", "360");
                                                kamPos.SetAttribute("fy", "288");
                                                kamPos.SetAttribute("F", "4.440000000");
                                                kamPos.SetAttribute("Px", "0.000005150");
                                                kamPos.SetAttribute("Py", "0.000004650");

                                                kameraElement.AppendChild(kamPos);

                                            }
                                        }
                                    }


                                    // dodatkowe parametry ( Zusatzparameter )
                                    list = zebMessstrecke.GetElementsByTagName("Zusatzparameter");
                                    foreach (XmlElement el in list)
                                    {
                                        XmlElement parametryDodatkowe = newDoc.CreateElement("dsn", "parametryNiestandardowe", "dsn");
                                        headerNode.AppendChild(parametryDodatkowe);
                                    }

                                    XmlElement danePodstawowe = newDoc.CreateElement("dsn", "danePodstawowe", "dsn");
                                    danePodstawowe.SetAttribute("sumaKontrolnaMD5", "XXXXXXX");
                                    danePodstawowe.SetAttribute("nazwa", "XXXXXXXXX");
                                    headerNode.AppendChild(danePodstawowe);

                                    list = zebMessstrecke.GetElementsByTagName("ZEBAdministration");
                                    foreach (XmlElement el in list)
                                    {
                                        XmlElement metadane = newDoc.CreateElement("dns", "metadane", "dsn");
                                        metadane.SetAttribute("wojewodztwo", "XXXXXXX");
                                        metadane.SetAttribute("rok", el.GetAttribute("Jahr"));
                                        metadane.SetAttribute("powod", el.GetAttribute("Anlass"));

                                        headerNode.AppendChild(metadane);
                                    }


                                    list = zebMessstrecke.GetElementsByTagName("Messparameter");
                                    foreach (XmlElement el in list)
                                    {
                                        XmlElement parametryPomiaru = newDoc.CreateElement("dsn", "parametryPomiaru", "dsn");
                                        parametryPomiaru.SetAttribute("numerRejestracyjny", el.GetAttribute("KfzKennz"));
                                        parametryPomiaru.SetAttribute("systemPomiarowy", el.GetAttribute("Messsystem"));
                                        parametryPomiaru.SetAttribute("producentUrzadzeniaPomiarowego", el.GetAttribute("Messgeraetebauer"));
                                        parametryPomiaru.SetAttribute("zasadaPomiaru", el.GetAttribute("Messprinzip"));
                                        parametryPomiaru.SetAttribute("kierowca", el.GetAttribute("Fahrer"));
                                        parametryPomiaru.SetAttribute("uzytkownikSystemuPomiarowego", el.GetAttribute("Messsystembetreiber"));
                                        parametryPomiaru.SetAttribute("operator", el.GetAttribute("Messsystembetreiber"));
                                        parametryPomiaru.SetAttribute("metodaOkreslaniaPolozeniaGeograficznego", el.GetAttribute("Positionsbestimmungsverfahren"));

                                        headerNode.AppendChild(parametryPomiaru);

                                        if (el.HasChildNodes)
                                        {
                                            //TODO
                                            XmlElement odlegloscPunktowPomiarowychProfilPodluzny = newDoc.CreateElement("dsn", "odlegloscPunktowPomiarowychProfilPodluzny", "dsn");
                                            odlegloscPunktowPomiarowychProfilPodluzny.InnerText = "XXXXXXXX";
                                            parametryPomiaru.AppendChild(odlegloscPunktowPomiarowychProfilPodluzny);
                                            // XmlElement Messpunktabstand_Messgeschwindigkeit

                                            XmlElement odlegloscPunktowPomiarowychPredkoscPomiaru = newDoc.CreateElement("dsn", "odlegloscPunktowPomiarowychPredkoscPomiaru", "dsn");
                                            odlegloscPunktowPomiarowychPredkoscPomiaru.InnerText = "XXXXXXXX";
                                            parametryPomiaru.AppendChild(odlegloscPunktowPomiarowychPredkoscPomiaru);

                                            XmlElement odlegloscPunktowPomiarowychKrzywizna = newDoc.CreateElement("dsn", "odlegloscPunktowPomiarowychKrzywizna", "dsn");
                                            odlegloscPunktowPomiarowychKrzywizna.InnerText = "XXXXXXXXXXXXXX";
                                            parametryPomiaru.AppendChild(odlegloscPunktowPomiarowychKrzywizna);

                                            XmlElement odlegloscPunktowPochyleniePodluzne = newDoc.CreateElement("dsn", "odlegloscPunktowPochyleniePodluzne", "dsn");
                                            odlegloscPunktowPochyleniePodluzne.InnerText = "XXXXXXXXXXXX";
                                            parametryPomiaru.AppendChild(odlegloscPunktowPochyleniePodluzne);

                                            XmlElement odlegloscPunktowOdlegloscOdKrawedzi = newDoc.CreateElement("dsn", "odlegloscPunktowOdlegloscOdKrawedzi", "dsn");
                                            odlegloscPunktowOdlegloscOdKrawedzi.InnerText = "XXXXXXXXXXXXXX";
                                            parametryPomiaru.AppendChild(odlegloscPunktowOdlegloscOdKrawedzi);

                                            XmlElement liniaPomiarowa = newDoc.CreateElement("dsn", "liniaPomiarowa", "dsn");
                                            liniaPomiarowa.InnerText = "XXXXXXXXXXXXXXXXXXXXXX";
                                            parametryPomiaru.AppendChild(liniaPomiarowa);
                                        }

                                    }

                                }

                                // end header

                            }                



                            //begin vorlauf
                            XmlNodeList vorlaufList = zebMessstrecke.GetElementsByTagName("Vorlauf");
                            XmlElement vorlauf = newDoc.CreateElement("dsn", "daneRozbiegowe", rootNew.NamespaceURI);
                            pomiar.AppendChild(vorlauf);
                            foreach (XmlElement el in vorlaufList)
                            {

                                if (el.HasChildNodes)
                                {
                                    foreach (XmlElement element in el.ChildNodes)
                                    {
                                        XmlElement vElement = newDoc.CreateElement("dsn", "R", rootNew.NamespaceURI);
                                        vElement.SetAttribute("A", element.GetAttribute("A"));
                                        vElement.SetAttribute("K", element.GetAttribute("K"));
                                        vElement.SetAttribute("L", element.GetAttribute("L"));
                                        vElement.InnerText = element.InnerText;
                                        vorlauf.AppendChild(vElement);
                                    }
                                }
                            }


                            //begin datenstrom  (strumien danych)

                            XmlNodeList datenstromList = zebMessstrecke.GetElementsByTagName("Datenstrom");
                            foreach (XmlElement dataElement in datenstromList)
                            {
                                XmlElement el = newDoc.CreateElement("dsn", "strumenDanych", rootNew.NamespaceURI);
                                el.SetAttribute("data", dataElement.GetAttribute("Datum"));
                                el.SetAttribute("metrBiezacyPoczatkuStrumienia", dataElement.GetAttribute("LfdM"));
                                el.SetAttribute("uwaga", dataElement.GetAttribute("Bemerkung"));
                                el.SetAttribute("G", dataElement.GetAttribute("G"));
                                el.SetAttribute("V", dataElement.GetAttribute("V"));
                                el.SetAttribute("godzina", dataElement.GetAttribute("Uhr"));
                                pomiar.AppendChild(el);

                                if (dataElement.HasChildNodes)
                                {

                                    XmlElement wgs = newDoc.CreateElement(rootNew.Prefix, "seriaLokalizacjiGeo", rootNew.NamespaceURI);
                                    el.AppendChild(wgs);

                                    foreach (XmlElement wgsElement in dataElement.FirstChild.ChildNodes)
                                    {
                                        XmlElement lokalizacja = newDoc.CreateElement("dsn", "lokalizacja", rootNew.NamespaceURI);
                                        lokalizacja.SetAttribute("data", wgsElement.GetAttribute("Datum"));
                                        lokalizacja.SetAttribute("uwaga", wgsElement.GetAttribute("Bemerkung"));
                                        lokalizacja.SetAttribute("godzina", wgsElement.GetAttribute("Uhr"));
                                        lokalizacja.SetAttribute("mb", wgsElement.GetAttribute("LfdM"));
                                        lokalizacja.SetAttribute("odleglosc", wgsElement.GetAttribute("LfdM"));
                                        wgs.AppendChild(lokalizacja);

                                        XmlElement wspolrzedneGeo = newDoc.CreateElement("dsn", "wspolrzedneGeo", rootNew.NamespaceURI);
                                        lokalizacja.AppendChild(wspolrzedneGeo);

                                        XmlElement pos = newDoc.CreateElement("gml", "pos", "http://www.opengis.net/gml");
                                        pos.SetAttribute("srsName", "WGS84");
                                        pos.InnerText = wgsElement.GetAttribute("L") + " " + wgsElement.GetAttribute("B");
                                        wspolrzedneGeo.AppendChild(pos);

                                    }

                                    /*
                                     * //TODO 
                                     */


                                    XmlElement asbElement = (XmlElement)dataElement.FirstChild.NextSibling;
                                    XmlElement bkmElement = (XmlElement)asbElement.NextSibling;
                                    XmlElement zuordnungElement = (XmlElement)bkmElement.NextSibling;

                                    streetClass = zuordnungElement.GetAttribute("Klasse");
                                    streetNr = zuordnungElement.GetAttribute("Nummer");

                                    int vst = Int32.Parse(asbElement.GetAttribute("VST"));
                                    int bst = Int32.Parse(asbElement.GetAttribute("BST"));
                                    int odleglosc = bst - vst;


                                    string sql = "select * from dane where pref = '"+asbElement.GetAttribute("VNK")+"' and nref = '"+asbElement.GetAttribute("NNK")+"' and odleglosc = "+vst+";";
                                    loadedData = LoadData(sqlCon, sql);

                                    if (loadedData != null && loadedData.Rows.Count == 0)
                                    {
                                         sql = "select * from dane where pref = '" + asbElement.GetAttribute("VNK") + "' and nref = '" + asbElement.GetAttribute("NNK") + "';";
                                         loadedData = LoadData(sqlCon, sql);
                                    }


                                    XmlElement lokalizacjaSiec = newDoc.CreateElement("dsn", "lokalizacjaSiec", rootNew.NamespaceURI);
                                   // lokalizacjaSiec.SetAttribute("dlugosc", asbElement.GetAttribute("VST"));
                                    lokalizacjaSiec.SetAttribute("dlugosc", Convert.ToString(odleglosc));

                                    lokalizacjaSiec.SetAttribute("kodPRef", asbElement.GetAttribute("VNK"));
                                    lokalizacjaSiec.SetAttribute("numerDrogi", zuordnungElement.GetAttribute("Nummer"));
                                    lokalizacjaSiec.SetAttribute("numerJezdni", numerJezdni);
                                    lokalizacjaSiec.SetAttribute("kierunek", kierunek);
                                    lokalizacjaSiec.SetAttribute("kodNRef", asbElement.GetAttribute("NNK"));
                                    lokalizacjaSiec.SetAttribute("pasRuchu", pasRuchu);
                                    lokalizacjaSiec.SetAttribute("odleglosc", asbElement.GetAttribute("VST"));
                                    el.AppendChild(lokalizacjaSiec);

                                    if (loadedData != null && loadedData.Rows.Count !=0)
                                    {
                                        pasRuchu = loadedData.Rows[0]["pas_ruchu"].ToString();
                                        numerJezdni = loadedData.Rows[0]["numer_jezdni"].ToString();
                                        kierunek = loadedData.Rows[0]["kierunek"].ToString();
                                        powiat = loadedData.Rows[0]["powiat"].ToString();
                                        if (String.IsNullOrEmpty(wojewodztwo))
                                        {
                                            wojewodztwo = loadedData.Rows[0]["wojewodztwo"].ToString();
                                        }

                                        XmlElement informacjeSieciowe = newDoc.CreateElement("dsn", "informacjeSieciowe", rootNew.NamespaceURI);
                                        informacjeSieciowe.SetAttribute("wojewodztwo", loadedData.Rows[0]["wojewodztwo"].ToString());
                                        informacjeSieciowe.SetAttribute("powiat", powiat);
                                        informacjeSieciowe.SetAttribute("gmina", gmina);
                                        informacjeSieciowe.SetAttribute("oddzial", loadedData.Rows[0]["oddzial"].ToString());
                                        informacjeSieciowe.SetAttribute("rejon", loadedData.Rows[0]["rejon"].ToString());
                                        informacjeSieciowe.SetAttribute("rodzajObszaru", loadedData.Rows[0]["rodzaj_obszaru"].ToString());
                                        el.AppendChild(informacjeSieciowe);
                                    }
                                    else
                                    {
                                        XmlElement informacjeSieciowe = newDoc.CreateElement("dsn", "informacjeSieciowe", rootNew.NamespaceURI);
                                        informacjeSieciowe.SetAttribute("wojewodztwo", "X");
                                        informacjeSieciowe.SetAttribute("powiat", powiat);
                                        informacjeSieciowe.SetAttribute("gmina", gmina);
                                        informacjeSieciowe.SetAttribute("oddzial", "X");
                                        informacjeSieciowe.SetAttribute("rejon", "X");
                                        informacjeSieciowe.SetAttribute("rodzajObszaru", "X");
                                        el.AppendChild(informacjeSieciowe);
                                    }


                                    XmlElement zdjecia = newDoc.CreateElement("dsn", "zdjecia", rootNew.NamespaceURI);
                                    el.AppendChild(zdjecia);

                                    XmlElement bilder = (XmlElement)zuordnungElement.NextSibling;
                                    if (bilder.HasChildNodes)
                                    {
                                        foreach (XmlElement zdjecie in bilder.ChildNodes)
                                        {
                                            String picFileName = null;
                                            String zdj = zdjecie.GetAttribute("D");

                                            if(File.Exists(picturesPath+"\\"+zdj)){
                                                //Console.WriteLine("EXIST");
                                                picFileName = RenamePictureFile(picturesPath, zdj);
                                            }
                                            else
                                            {
                                                String[] splitedZDJ = zdj.Split('\\');
                                                String folder = splitedZDJ[0];
                                                String fNameOld = splitedZDJ[2];
                                                String[] splitedNameOld = fNameOld.Split('_');
                                                String prefixName = splitedNameOld[0].Replace("B", "P");
                                                String newName = prefixName + "_" + splitedNameOld[1] + "_" + splitedNameOld[2];

                                                if(File.Exists(picturesPath+"\\"+folder +"\\"+newName)){
                                                    picFileName = folder + "\\" + newName;
                                                }
                                                else
                                                {
                                                    picFileName = zdj;
                                                }
                                                
                                            }


                                            XmlElement zd = newDoc.CreateElement("dsn", "zdjecie", rootNew.NamespaceURI);
                                            //zd.SetAttribute("plik", zdjecie.GetAttribute("D"));
                                            zd.SetAttribute("plik", picFileName);
                                            zd.SetAttribute("nrKamery", zdjecie.GetAttribute("Nr"));
                                            zd.SetAttribute("A", "X");
                                            zd.SetAttribute("odleglosc", zdjecie.GetAttribute("Station"));
                                            zdjecia.AppendChild(zd);
                                        }
                                    }

                                    XmlNodeList rList = dataElement.GetElementsByTagName("R");
                                    foreach (XmlElement rElement in rList)
                                    {
                                        XmlElement r = newDoc.CreateElement("dsn", "R", rootNew.NamespaceURI);
                                        r.SetAttribute("A", rElement.GetAttribute("A"));
                                        r.SetAttribute("K", rElement.GetAttribute("K"));
                                        r.SetAttribute("L", rElement.GetAttribute("L"));
                                        r.InnerText = rElement.InnerText;
                                        el.AppendChild(r);
                                    }

                                }
                            }

                            //begin nachlauf  (dane pobiegowe)
                            XmlNodeList nachlaufList = zebMessstrecke.GetElementsByTagName("Nachlauf");
                            XmlElement nachlauf = newDoc.CreateElement("dsn", "danePobiegowe", rootNew.NamespaceURI);
                            pomiar.AppendChild(nachlauf);
                            foreach (XmlElement el in nachlaufList)
                            {

                                if (el.HasChildNodes)
                                {
                                    foreach (XmlElement element in el.ChildNodes)
                                    {
                                        XmlElement vElement = newDoc.CreateElement("dsn", "R", rootNew.NamespaceURI);
                                        vElement.SetAttribute("A", element.GetAttribute("A"));
                                        vElement.SetAttribute("K", element.GetAttribute("K"));
                                        vElement.SetAttribute("L", element.GetAttribute("L"));
                                        vElement.InnerText = element.InnerText;
                                        nachlauf.AppendChild(vElement);
                                    }
                                }
                            }

                        }

                           bool val = ValidateDsnXML(newDoc);
                           if (val)
                           {
                               DisplayData("Document is valid.");
                           }
                           else
                           {
                               DisplayData("Document is not valid.");
                           }
                    
                        // Create file name compatible with DSN specyfication
                            String fileNameFinal = createFileName(streetClass, streetNr, wojewodztwo, type);

                            if (outputFolder != null && Directory.Exists(outputFolder))
                            {
                                saveFile(newDoc, outputFolder, fileNameFinal, type);
                            }
                            else
                            {
                                saveFile(newDoc, path, fileNameFinal, type);
                            }

                        
                        break;
                    }
                case "TP1b":
                    {
                        XmlElement rootOld = doc.DocumentElement;

                        XmlElement rootNew = newDoc.CreateElement("dsn", "daneElementarnePPNySiec", "http://www.gddkia.gov.pl/dsn/1.0.0");
                        rootNew.SetAttribute("xmlns:dsn", "http://www.gddkia.gov.pl/dsn/1.0.0");
                        rootNew.SetAttribute("xmlns:gml", "http://www.opengis.net/gml");
                        rootNew.SetAttribute("xmlns:sch", "http://www.ascc.net/xml/schematron");
                        rootNew.SetAttribute("xmlns:xlink", "http://www.w3.org/1999/xlink");
                        rootNew.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
                        rootNew.SetAttribute("schemaLocation", "http://www.w3.org/2001/XMLSchema-instance", "http://www.gddkia.gov.pl/dsn/1.0.0/dane_elementarne.xsd");
                        rootNew.SetAttribute("uwaga", rootOld.GetAttribute("Bemerkung") == null ? "" : rootOld.GetAttribute("Bemerkung"));
                        rootNew.SetAttribute("rodzaj", "sieciowe");
                        // rootNew.SetAttribute("cecha", rootOld.GetAttribute("Merkmal") == null ? "" : rootOld.GetAttribute("Merkmal"));
                        rootNew.SetAttribute("cecha", "rownosc poprzeczna");
                        rootNew.SetAttribute("dataUtworzenia", rootOld.GetAttribute("Erstelldatum") == null ? "" : rootOld.GetAttribute("Erstelldatum"));
                        newDoc.AppendChild(xmlDeclaration);
                        newDoc.AppendChild(rootNew);

                         XmlNodeList pomiaryList = doc.GetElementsByTagName("ZEBMessstrecke");
                         int przejazdCount = 0;
                         int headerCount = 0;

                         foreach (XmlElement zebMessstrecke in pomiaryList)
                         {

                             XmlElement pomiar = newDoc.CreateElement("dsn", "przejazdPomiarowy", rootNew.NamespaceURI);
                             if (przejazdCount == 0)
                             {
                                 XmlAttribute t = newDoc.CreateAttribute("gml", "id", "http://www.opengis.net/gml");
                                 t.Value = gmlID;
                                 pomiar.SetAttributeNode(t);
                                 przejazdCount++;
                             }

                             rootNew.AppendChild(pomiar);

                             XmlElement headerNode = null;

                             if (tp1bHeader != null)
                             {
                                 XmlNodeList hList = tp1bHeader.GetElementsByTagName("dsn:naglowekPrzejazduPomiarowego");
                                 if (hList != null && hList.Count != 0)
                                 {
                                     headerNode = (XmlElement)hList[0];

                                     XmlAttribute testA = headerNode.Attributes["gml:id"];
                                     if (headerCount == 0 && headerNode.Attributes["gml:id"] != null)
                                     {
                                         gmlID = increaseGmlID(gmlID);
                                         headerNode.Attributes["gml:id"].Value = gmlID;
                                         headerCount++;
                                     }
                                     else
                                     {
                                         if (headerNode.Attributes["gml:id"] != null)
                                         {
                                             headerNode.RemoveAttribute("id", "http://www.opengis.net/gml");
                                         }
                                     }


                                     XmlNodeList liMetadane = headerNode.GetElementsByTagName("dsn:metadane");
                                     if (liMetadane != null)
                                     {
                                         XmlElement metadane = (XmlElement)liMetadane[0];
                                         wojewodztwo = metadane.GetAttribute("wojewodztwo");
                                     }

                                     // pomiar.AppendChild(headerNode);
                                     XmlNode hNode = newDoc.ImportNode(headerNode, true);
                                     pomiar.AppendChild(hNode);
                                 }

                             }
                             else
                             {
                                 // get header and paste to new XML
                                 XmlNodeList header = getHeader(doc, null);
                                 if (header != null)
                                 {
                                     headerNode = newDoc.CreateElement("dsn", "naglowekPrzejazduPomiarowego", "dsn");
                                     pomiar.AppendChild(headerNode);

                                     // copy bildparameter
                                     XmlNodeList list = zebMessstrecke.GetElementsByTagName("Bildparameter");
                                     foreach (XmlElement node in list)
                                     {
                                         XmlElement bp = newDoc.CreateElement("dsn", "parametryZdjecia", "dsn");
                                         bp.SetAttribute("nosnikDanych", node.GetAttribute("Datentraeger"));
                                         headerNode.AppendChild(bp);

                                         if (node.HasChildNodes)
                                         {
                                             XmlNodeList listKamera = node.GetElementsByTagName("Kamera");
                                             foreach (XmlElement el in listKamera)
                                             {
                                                 XmlElement kameraElement = newDoc.CreateElement("dsn", "kamera", "dsn");
                                                 kameraElement.SetAttribute("formatZdjecia", el.GetAttribute("Bildformat"));
                                                 kameraElement.SetAttribute("rozdzielczoscX", el.GetAttribute("PixelH"));
                                                 kameraElement.SetAttribute("rozdzielczoscY", el.GetAttribute("PixelV"));
                                                 kameraElement.SetAttribute("nr", el.GetAttribute("Nr"));
                                                 kameraElement.SetAttribute("katalogGlowny", el.GetAttribute("RootVerzeichnis"));
                                                 kameraElement.SetAttribute("nazwa", el.GetAttribute("Name"));

                                                 bp.AppendChild(kameraElement);

                                                 XmlElement kamPos = newDoc.CreateElement("dsn", "pozycja", "dsn");
                                                 kamPos.SetAttribute("X0", "878");
                                                 kamPos.SetAttribute("Y0", "179");
                                                 kamPos.SetAttribute("Z0", "2729");
                                                 kamPos.SetAttribute("alpha", "-0.0000");
                                                 kamPos.SetAttribute("ny", "1.4504");
                                                 kamPos.SetAttribute("kappa", "0.0070");
                                                 kamPos.SetAttribute("fx", "360");
                                                 kamPos.SetAttribute("fy", "288");
                                                 kamPos.SetAttribute("F", "4.440000000");
                                                 kamPos.SetAttribute("Px", "0.000005150");
                                                 kamPos.SetAttribute("Py", "0.000004650");

                                                 kameraElement.AppendChild(kamPos);

                                             }

                                             // dodatkowe parametry ( Zusatzparameter )
                                             list = zebMessstrecke.GetElementsByTagName("Zusatzparameter");
                                             foreach (XmlElement el in list)
                                             {
                                                 XmlElement parametryDodatkowe = newDoc.CreateElement("dsn", "parametryNiestandardowe", "dsn");
                                                 headerNode.AppendChild(parametryDodatkowe);
                                             }

                                             XmlElement danePodstawowe = newDoc.CreateElement("dsn", "danePodstawowe", "dsn");
                                             danePodstawowe.SetAttribute("sumaKontrolnaMD5", "XXXXXXX");
                                             danePodstawowe.SetAttribute("nazwa", "XXXXXXXXX");
                                             headerNode.AppendChild(danePodstawowe);

                                             list = zebMessstrecke.GetElementsByTagName("ZEBAdministration");
                                             foreach (XmlElement el in list)
                                             {
                                                 XmlElement metadane = newDoc.CreateElement("dns", "metadane", "dsn");
                                                 metadane.SetAttribute("wojewodztwo", "XXXXXXX");
                                                 metadane.SetAttribute("rok", el.GetAttribute("Jahr"));
                                                 metadane.SetAttribute("powod", el.GetAttribute("Anlass"));

                                                 headerNode.AppendChild(metadane);
                                             }

                                             list = zebMessstrecke.GetElementsByTagName("Messparameter");
                                             foreach (XmlElement el in list)
                                             {
                                                 XmlElement parametryPomiaru = newDoc.CreateElement("dsn", "parametryPomiaru", "dsn");
                                                 parametryPomiaru.SetAttribute("numerRejestracyjny", el.GetAttribute("KfzKennz"));
                                                 parametryPomiaru.SetAttribute("systemPomiarowy", el.GetAttribute("Messsystem"));
                                                 parametryPomiaru.SetAttribute("producentUrzadzeniaPomiarowego", el.GetAttribute("Messgeraetebauer"));
                                                 parametryPomiaru.SetAttribute("zasadaPomiaru", el.GetAttribute("Messprinzip"));
                                                 parametryPomiaru.SetAttribute("kierowca", el.GetAttribute("Fahrer"));
                                                 parametryPomiaru.SetAttribute("uzytkownikSystemuPomiarowego", el.GetAttribute("Messsystembetreiber"));
                                                 parametryPomiaru.SetAttribute("operator", el.GetAttribute("Messsystembetreiber"));
                                                 parametryPomiaru.SetAttribute("metodaOkreslaniaPolozeniaGeograficznego", el.GetAttribute("Positionsbestimmungsverfahren"));

                                                 headerNode.AppendChild(parametryPomiaru);

                                                 if (el.HasChildNodes)
                                                 {
                                                     //TODO
                                                     XmlElement odlegloscPunktowPomiarowychPredkoscPomiaru = newDoc.CreateElement("dsn", "odlegloscPunktowPomiarowychPredkoscPomiaru", "dsn");
                                                     XmlElement Messpunktabstand_Messgeschwindigkeit = (XmlElement)el.FirstChild;
                                                     odlegloscPunktowPomiarowychPredkoscPomiaru.InnerText = Messpunktabstand_Messgeschwindigkeit.InnerText;
                                                     parametryPomiaru.AppendChild(odlegloscPunktowPomiarowychPredkoscPomiaru);


                                                     XmlElement odleglosPunktowPomiarowychProfilePoprzeczne = newDoc.CreateElement("dsn", "odleglosPunktowPomiarowychProfilePoprzeczne", "dsn");
                                                     XmlElement Abstand_Profile_In_Laengsrichtung = (XmlElement)Messpunktabstand_Messgeschwindigkeit.NextSibling;
                                                     odleglosPunktowPomiarowychProfilePoprzeczne.InnerText = Abstand_Profile_In_Laengsrichtung.InnerText;
                                                     parametryPomiaru.AppendChild(odleglosPunktowPomiarowychProfilePoprzeczne);

                                                     XmlElement liniaPomiarowa = newDoc.CreateElement("dsn", "liniaPomiarowa", "dsn");
                                                     XmlElement Messlinie = (XmlElement)Abstand_Profile_In_Laengsrichtung.NextSibling;
                                                     liniaPomiarowa.InnerText = Messlinie.InnerText;
                                                     parametryPomiaru.AppendChild(liniaPomiarowa);


                                                     XmlElement odlegloscPunktowOdlegloscOdKrawedzi = newDoc.CreateElement("dsn", "odlegloscPunktowOdlegloscOdKrawedzi", "dsn");
                                                     odlegloscPunktowOdlegloscOdKrawedzi.InnerText = "XXXXXXXXXXXX";
                                                     parametryPomiaru.AppendChild(odlegloscPunktowOdlegloscOdKrawedzi);

                                                     XmlNodeList listCzujnik = el.GetElementsByTagName("Sonde");
                                                     foreach (XmlElement czujnik in listCzujnik)
                                                     {
                                                         XmlElement czElement = newDoc.CreateElement("dsn", "czujnik", "dsn");
                                                         czElement.SetAttribute("odstepQ", czujnik.GetAttribute("Abst_Q"));
                                                         czElement.SetAttribute("nr", czujnik.GetAttribute("Nr"));
                                                         parametryPomiaru.AppendChild(czElement);
                                                     }

                                                 }

                                             }//endHeader


                                         }

                                     }
                                 }
                             }


                                        //begin datenstrom  (strumien danych)

                                        XmlNodeList datenstromList = zebMessstrecke.GetElementsByTagName("Datenstrom");
                                        foreach (XmlElement dataElement in datenstromList)
                                        {
                                            XmlElement el = newDoc.CreateElement("dsn", "strumenDanych", rootNew.NamespaceURI);
                                            el.SetAttribute("data", dataElement.GetAttribute("Datum"));
                                            el.SetAttribute("metrBiezacyPoczatkuStrumienia", dataElement.GetAttribute("LfdM"));
                                            el.SetAttribute("uwaga", dataElement.GetAttribute("Bemerkung"));
                                            el.SetAttribute("G", dataElement.GetAttribute("G"));
                                            el.SetAttribute("V", dataElement.GetAttribute("V"));
                                            el.SetAttribute("godzina", dataElement.GetAttribute("Uhr"));
                                            pomiar.AppendChild(el);

                                            if (dataElement.HasChildNodes)
                                            {

                                                XmlElement wgs = newDoc.CreateElement(rootNew.Prefix, "seriaLokalizacjiGeo", rootNew.NamespaceURI);
                                                el.AppendChild(wgs);

                                                foreach (XmlElement wgsElement in dataElement.FirstChild.ChildNodes)
                                                {
                                                    XmlElement lokalizacja = newDoc.CreateElement("dsn", "lokalizacja", rootNew.NamespaceURI);
                                                    lokalizacja.SetAttribute("data", wgsElement.GetAttribute("Datum"));
                                                    lokalizacja.SetAttribute("uwaga", wgsElement.GetAttribute("Bemerkung"));
                                                    lokalizacja.SetAttribute("godzina", wgsElement.GetAttribute("Uhr"));
                                                    lokalizacja.SetAttribute("mb", wgsElement.GetAttribute("LfdM"));
                                                    lokalizacja.SetAttribute("odleglosc", wgsElement.GetAttribute("LfdM"));
                                                    wgs.AppendChild(lokalizacja);

                                                    XmlElement wspolrzedneGeo = newDoc.CreateElement("dsn", "wspolrzedneGeo", rootNew.NamespaceURI);
                                                    lokalizacja.AppendChild(wspolrzedneGeo);

                                                    XmlElement pos = newDoc.CreateElement("gml", "pos", "http://www.opengis.net/gml");
                                                    pos.SetAttribute("srsName", "WGS84");
                                                    pos.InnerText = wgsElement.GetAttribute("L") + " " + wgsElement.GetAttribute("B");
                                                    wspolrzedneGeo.AppendChild(pos);

                                                }

                                                /*
                                                 * //TODO 
                                                 */


                                                XmlElement asbElement = (XmlElement)dataElement.FirstChild.NextSibling;
                                                XmlElement bkmElement = (XmlElement)asbElement.NextSibling;
                                                XmlElement zuordnungElement = (XmlElement)bkmElement.NextSibling;

                                                streetClass = zuordnungElement.GetAttribute("Klasse");
                                                streetNr = zuordnungElement.GetAttribute("Nummer");

                                                int vst = Int32.Parse(asbElement.GetAttribute("VST"));
                                                int bst = Int32.Parse(asbElement.GetAttribute("BST"));
                                                int odleglosc = bst - vst;


                                                string sql = "select * from dane where pref = '" + asbElement.GetAttribute("VNK") + "' and nref = '" + asbElement.GetAttribute("NNK") + "' and odleglosc = " + vst + ";";
                                                loadedData = LoadData(sqlCon, sql);

                                                if (loadedData != null && loadedData.Rows.Count == 0)
                                                {
                                                    sql = "select * from dane where pref = '" + asbElement.GetAttribute("VNK") + "' and nref = '" + asbElement.GetAttribute("NNK") + "';";
                                                    loadedData = LoadData(sqlCon, sql);
                                                }

                                                XmlElement lokalizacjaSiec = newDoc.CreateElement("dsn", "lokalizacjaSiec", rootNew.NamespaceURI);
                                               // lokalizacjaSiec.SetAttribute("dlugosc", asbElement.GetAttribute("VST"));
                                                lokalizacjaSiec.SetAttribute("dlugosc", Convert.ToString(odleglosc));
                                                lokalizacjaSiec.SetAttribute("kodPRef", asbElement.GetAttribute("VNK"));
                                                lokalizacjaSiec.SetAttribute("numerDrogi", zuordnungElement.GetAttribute("Nummer"));
                                                lokalizacjaSiec.SetAttribute("numerJezdni", numerJezdni);
                                                lokalizacjaSiec.SetAttribute("kierunek", kierunek);
                                                lokalizacjaSiec.SetAttribute("kodNRef", asbElement.GetAttribute("NNK"));
                                                lokalizacjaSiec.SetAttribute("pasRuchu", pasRuchu);
                                               // lokalizacjaSiec.SetAttribute("odleglosc", Convert.ToString(odleglosc));
                                                lokalizacjaSiec.SetAttribute("odleglosc", asbElement.GetAttribute("VST"));
                                                el.AppendChild(lokalizacjaSiec);


                                                if (loadedData != null && loadedData.Rows.Count !=0)
                                                {
                                                    pasRuchu = loadedData.Rows[0]["pas_ruchu"].ToString();
                                                    numerJezdni = loadedData.Rows[0]["numer_jezdni"].ToString();
                                                    kierunek = loadedData.Rows[0]["kierunek"].ToString();
                                                    powiat = loadedData.Rows[0]["powiat"].ToString();
                                                    if (String.IsNullOrEmpty(wojewodztwo))
                                                    {
                                                        wojewodztwo = loadedData.Rows[0]["wojewodztwo"].ToString();
                                                    }

                                                    XmlElement informacjeSieciowe = newDoc.CreateElement("dsn", "informacjeSieciowe", rootNew.NamespaceURI);
                                                    informacjeSieciowe.SetAttribute("wojewodztwo", loadedData.Rows[0]["wojewodztwo"].ToString());
                                                    informacjeSieciowe.SetAttribute("powiat", powiat);
                                                    informacjeSieciowe.SetAttribute("gmina", gmina);
                                                    informacjeSieciowe.SetAttribute("oddzial", loadedData.Rows[0]["oddzial"].ToString());
                                                    informacjeSieciowe.SetAttribute("rejon", loadedData.Rows[0]["rejon"].ToString());
                                                    informacjeSieciowe.SetAttribute("rodzajObszaru", loadedData.Rows[0]["rodzaj_obszaru"].ToString());
                                                    el.AppendChild(informacjeSieciowe);                                          

                                                }
                                                else
                                                {
                                                    XmlElement informacjeSieciowe = newDoc.CreateElement("dsn", "informacjeSieciowe", rootNew.NamespaceURI);
                                                    informacjeSieciowe.SetAttribute("wojewodztwo", "XX");
                                                    informacjeSieciowe.SetAttribute("powiat", "XX");
                                                    informacjeSieciowe.SetAttribute("gmina", "XX");
                                                    informacjeSieciowe.SetAttribute("oddzial", "XXXX");
                                                    informacjeSieciowe.SetAttribute("rejon", "XXXX");
                                                    informacjeSieciowe.SetAttribute("rodzajObszaru", "XXXXXX");
                                                    el.AppendChild(informacjeSieciowe);
                                                }


                                                XmlElement zdjecia = newDoc.CreateElement("dsn", "zdjecia", rootNew.NamespaceURI);
                                                el.AppendChild(zdjecia);

                                                XmlElement bilder = (XmlElement)zuordnungElement.NextSibling;
                                                if (bilder.HasChildNodes)
                                                {
                                                    foreach (XmlElement zdjecie in bilder.ChildNodes)
                                                    {
                                                        
                                                        String picFileName = null;
                                                        String zdj = zdjecie.GetAttribute("D");

                                                        if (File.Exists(picturesPath + "\\" + zdj))
                                                        {
                                                            //Console.WriteLine("EXIST");
                                                            picFileName = RenamePictureFile(picturesPath, zdj);
                                                        }
                                                        else
                                                        {
                                                            String[] splitedZDJ = zdj.Split('\\');
                                                            String folder = splitedZDJ[0];
                                                            String fNameOld = splitedZDJ[2];
                                                            String[] splitedNameOld = fNameOld.Split('_');
                                                            String prefixName = splitedNameOld[0].Replace("B", "P");
                                                            String newName = prefixName + "_" + splitedNameOld[1] + "_" + splitedNameOld[2];

                                                            if (File.Exists(picturesPath + "\\" + folder + "\\" + newName))
                                                            {
                                                                picFileName = folder + "\\" + newName;
                                                            }
                                                            else
                                                            {
                                                                picFileName = zdj;
                                                            }

                                                        }

                                                        XmlElement zd = newDoc.CreateElement("dsn", "zdjecie", rootNew.NamespaceURI);
                                                        zd.SetAttribute("plik", picFileName);
                                                        zd.SetAttribute("nrKamery", zdjecie.GetAttribute("Nr"));
                                                        zd.SetAttribute("A", "X");
                                                        zd.SetAttribute("odleglosc", zdjecie.GetAttribute("Station"));
                                                        zdjecia.AppendChild(zd);
                                                    }
                                                }

                                                XmlNodeList rList = dataElement.GetElementsByTagName("R");
                                                foreach (XmlElement rElement in rList)
                                                {
                                                    XmlElement r = newDoc.CreateElement("dsn", "R", rootNew.NamespaceURI);
                                                    r.SetAttribute("A", rElement.GetAttribute("A"));
                                                   // r.SetAttribute("K", rElement.GetAttribute("K") == null || rElement.GetAttribute("K") == "" ? "X" : rElement.GetAttribute("K"));
                                                  //  r.SetAttribute("L", rElement.GetAttribute("L") == null || rElement.GetAttribute("L") == "" ? "X" : rElement.GetAttribute("L"));
                                                    r.InnerText = rElement.InnerText;
                                                    el.AppendChild(r);
                                                }

                                            }
                                        }

                         }//foreach zebMesstrecke in pomiaryList


                         bool val = ValidateDsnXML(newDoc);
                         if (val)
                         {
                             DisplayData("Document is valid.");
                         }
                         else
                         {
                             DisplayData("Document is not valid.");
                         }


                         // Create file name compatible with DSN specyfication
                         String fileNameFinal = createFileName(streetClass, streetNr, wojewodztwo, type);

                         if (outputFolder != null)
                         {
                             saveFile(newDoc, outputFolder, fileNameFinal, type);
                         }
                         else
                         {
                             saveFile(newDoc, path, fileNameFinal, type);
                         }

                         break;
                    }
                case "TP3":
                    {

                        XmlElement rootOld = doc.DocumentElement;

                        XmlElement rootNew = newDoc.CreateElement("dsn", "daneElementarnePPIsiec", "http://www.gddkia.gov.pl/dsn/1.0.0");
                        rootNew.SetAttribute("xmlns:dsn", "http://www.gddkia.gov.pl/dsn/1.0.0");
                        rootNew.SetAttribute("xmlns:gml", "http://www.opengis.net/gml");
                        rootNew.SetAttribute("xmlns:sch", "http://www.ascc.net/xml/schematron");
                        rootNew.SetAttribute("xmlns:xlink", "http://www.w3.org/1999/xlink");
                        rootNew.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
                        rootNew.SetAttribute("schemaLocation", "http://www.w3.org/2001/XMLSchema-instance", "http://www.gddkia.gov.pl/dsn/1.0.0/dane_elementarne.xsd");
                        rootNew.SetAttribute("uwaga", rootOld.GetAttribute("Bemerkung") == null ? "" : rootOld.GetAttribute("Bemerkung"));
                        rootNew.SetAttribute("rodzaj", "sieciowe");
                        // rootNew.SetAttribute("cecha", rootOld.GetAttribute("Merkmal") == null ? "" : rootOld.GetAttribute("Merkmal"));
                        rootNew.SetAttribute("cecha", "cechy powierzchniowe");
                        rootNew.SetAttribute("dataUtworzenia", rootOld.GetAttribute("Erstelldatum") == null ? "" : rootOld.GetAttribute("Erstelldatum"));
                        newDoc.AppendChild(xmlDeclaration);
                        newDoc.AppendChild(rootNew);

                        XmlElement headerNode = null;

                        int przejazdCount = 0;
                        int headerCount = 0;

                        if (tp3Header != null)
                        {
                            XmlNodeList hList = tp3Header.GetElementsByTagName("dsn:naglowekPrzejazduPomiarowego");
                            if (hList != null && hList.Count != 0)
                            {
                               XmlElement hNode = (XmlElement)hList[0];
                               XmlAttribute testA = hNode.Attributes["gml:id"];
                               if (headerCount == 0 && hNode.Attributes["gml:id"] != null)
                               {
                                   String increasedGmlID = increaseGmlID(gmlID);
                                   hNode.Attributes["gml:id"].Value = increasedGmlID;
                                   headerCount++;
                               }
                               else
                               {
                                   if (headerNode.Attributes["gml:id"] != null)
                                   {
                                       headerNode.RemoveAttribute("id", "http://www.opengis.net/gml");
                                   }
                               }


                               XmlNodeList liMetadane = hNode.GetElementsByTagName("dsn:metadane");
                               if (liMetadane != null)
                               {
                                   XmlElement metadane = (XmlElement)liMetadane[0];
                                   wojewodztwo = metadane.GetAttribute("wojewodztwo");
                               }

                                // pomiar.AppendChild(headerNode);
                                headerNode = (XmlElement)newDoc.ImportNode(hNode, true);
                                //rootNew.AppendChild(headerNode);

                                
                            }

                        }
                        else
                        {
                             // get header and paste to new XML
                            XmlNodeList header = getHeader(doc, null);
                            if (header != null)
                            { 
                            
                                  XmlElement headerElement = (XmlElement)header[0];
                                  if (headerElement != null)
                                  {

                                      headerNode = newDoc.CreateElement("dsn", "naglowekPrzejazduPomiarowego", rootNew.NamespaceURI);
                                      XmlAttribute t = newDoc.CreateAttribute("gml", "id", "http://www.opengis.net/gml");
                                      t.Value = gmlID;
                                      headerNode.SetAttributeNode(t);
                                      rootNew.AppendChild(headerNode);

                                      XmlElement bildparameter = (XmlElement)headerElement.FirstChild;
                                      XmlElement bp = newDoc.CreateElement("dsn", "parametryZdjecia", rootNew.NamespaceURI);
                                      bp.SetAttribute("nosnikDanych", bildparameter.GetAttribute("Datentraeger"));
                                      headerNode.AppendChild(bp);

                                      if (bildparameter.HasChildNodes)
                                      {

                                          XmlElement Messpunktabstand_Bilder = (XmlElement)bildparameter.FirstChild;

                                          XmlNodeList kamList = bildparameter.GetElementsByTagName("Kamera");
                                          foreach (XmlElement kamElement in kamList)
                                          {

                                              XmlElement kameraElement = newDoc.CreateElement("dsn", "kamera", rootNew.NamespaceURI);
                                              kameraElement.SetAttribute("formatZdjecia", kamElement.GetAttribute("Bildformat"));
                                              kameraElement.SetAttribute("rozdzielczoscX", kamElement.GetAttribute("PixelH"));
                                              kameraElement.SetAttribute("rozdzielczoscY", kamElement.GetAttribute("PixelV"));
                                              kameraElement.SetAttribute("nr", kamElement.GetAttribute("Nr"));
                                              kameraElement.SetAttribute("katalogGlowny", kamElement.GetAttribute("RootVerzeichnis"));
                                              kameraElement.SetAttribute("nazwa", kamElement.GetAttribute("Name"));

                                              bp.AppendChild(kameraElement);

                                              if (kamElement.HasChildNodes)
                                              {
                                                  foreach (XmlElement posEl in kamElement.ChildNodes)
                                                  {

                                                      XmlElement kamPos = newDoc.CreateElement("dsn", "pozycja", rootNew.NamespaceURI);
                                                      kamPos.SetAttribute("X0", "878");
                                                      kamPos.SetAttribute("Y0", "179");
                                                      kamPos.SetAttribute("Z0", "2729");
                                                      kamPos.SetAttribute("alpha", "-0.0000");
                                                      kamPos.SetAttribute("ny", "1.4504");
                                                      kamPos.SetAttribute("kappa", "0.0070");
                                                      kamPos.SetAttribute("fx", "360");
                                                      kamPos.SetAttribute("fy", "288");
                                                      kamPos.SetAttribute("F", "4.440000000");
                                                      kamPos.SetAttribute("Px", "0.000005150");
                                                      kamPos.SetAttribute("Py", "0.000004650");

                                                      kameraElement.AppendChild(kamPos);
                                                  }
                                              }

                                          }

                                      }

                                      //end Bildparameter

                                      // dodatkowe parametry ( Zusatzparameter )
                                      XmlNodeList list = headerElement.GetElementsByTagName("Zusatzparameter");
                                      foreach (XmlElement el in list)
                                      {
                                          XmlElement parametryDodatkowe = newDoc.CreateElement("dsn", "parametryNiestandardowe", rootNew.NamespaceURI);
                                          headerNode.AppendChild(parametryDodatkowe);
                                      }


                                      XmlElement danePodstawowe = newDoc.CreateElement("dsn", "danePodstawowe", rootNew.NamespaceURI);
                                      danePodstawowe.SetAttribute("sumaKontrolnaMD5", "XXXXXXX");
                                      danePodstawowe.SetAttribute("nazwa", "XXXXXXXXX");
                                      headerNode.AppendChild(danePodstawowe);


                                      list = headerElement.GetElementsByTagName("ZEBAdministration");
                                      foreach (XmlElement el in list)
                                      {
                                          XmlElement metadane = newDoc.CreateElement("dns", "metadane", rootNew.NamespaceURI);
                                          metadane.SetAttribute("wojewodztwo", "XXXXXXX");
                                          metadane.SetAttribute("rok", el.GetAttribute("Jahr"));
                                          metadane.SetAttribute("powod", el.GetAttribute("Anlass"));

                                          headerNode.AppendChild(metadane);
                                      }



                                      list = headerElement.GetElementsByTagName("Messparameter");
                                      foreach (XmlElement el in list)
                                      {
                                          XmlElement parametryPomiaru = newDoc.CreateElement("dsn", "parametryPomiaru", rootNew.NamespaceURI);
                                          parametryPomiaru.SetAttribute("numerRejestracyjny", el.GetAttribute("KfzKennz"));
                                          parametryPomiaru.SetAttribute("systemPomiarowy", el.GetAttribute("Messsystem"));
                                          parametryPomiaru.SetAttribute("producentUrzadzeniaPomiarowego", el.GetAttribute("Messgeraetebauer"));
                                          parametryPomiaru.SetAttribute("zasadaPomiaru", el.GetAttribute("Messprinzip"));
                                          parametryPomiaru.SetAttribute("kierowca", el.GetAttribute("Fahrer"));
                                          parametryPomiaru.SetAttribute("uzytkownikSystemuPomiarowego", el.GetAttribute("Messsystembetreiber"));
                                          parametryPomiaru.SetAttribute("operator", el.GetAttribute("Messsystembetreiber"));
                                          parametryPomiaru.SetAttribute("metodaOkreslaniaPolozeniaGeograficznego", el.GetAttribute("Positionsbestimmungsverfahren"));

                                          headerNode.AppendChild(parametryPomiaru);

                                          if (el.HasChildNodes)
                                          {
                                              //TODO
                                              //XmlElement odlegloscPunktowPomiarowychPredkoscPomiaru = newDoc.CreateElement("dsn", "odlegloscPunktowPomiarowychPredkoscPomiaru", "dsn");
                                              //XmlElement Messpunktabstand_Messgeschwindigkeit = (XmlElement)el.FirstChild;
                                              //odlegloscPunktowPomiarowychPredkoscPomiaru.InnerText = Messpunktabstand_Messgeschwindigkeit.InnerText;
                                              //parametryPomiaru.AppendChild(odlegloscPunktowPomiarowychPredkoscPomiaru);


                                              //XmlElement odleglosPunktowPomiarowychProfilePoprzeczne = newDoc.CreateElement("dsn", "odleglosPunktowPomiarowychProfilePoprzeczne", "dsn");
                                              //XmlElement Abstand_Profile_In_Laengsrichtung = (XmlElement)Messpunktabstand_Messgeschwindigkeit.NextSibling;
                                              //odleglosPunktowPomiarowychProfilePoprzeczne.InnerText = Abstand_Profile_In_Laengsrichtung.InnerText;
                                              //parametryPomiaru.AppendChild(odleglosPunktowPomiarowychProfilePoprzeczne);

                                          }
                                      }
                          
                                  }  
                            }
                        }
                        //end header
                


                         XmlNodeList pomiaryList = doc.GetElementsByTagName("ZEBMessstrecke");

                         foreach (XmlElement zebMessstrecke in pomiaryList)
                         {

                             XmlElement pomiar = newDoc.CreateElement("dsn", "przejazdPomiarowy", rootNew.NamespaceURI);
                             if (przejazdCount == 0)
                             {
                                 XmlAttribute t = newDoc.CreateAttribute("gml", "id", "http://www.opengis.net/gml");
                                 t.Value = gmlID;
                                 pomiar.SetAttributeNode(t);
                                 przejazdCount++;
                             }

                             rootNew.AppendChild(pomiar);
                             pomiar.AppendChild(headerNode);
                     
                             
                            XmlNodeList datenstromList = zebMessstrecke.GetElementsByTagName("Datenstrom");
                            foreach (XmlElement dataElement in datenstromList)
                            {

                                XmlElement el = newDoc.CreateElement("dsn", "strumenDanych", rootNew.NamespaceURI);

                                el.SetAttribute("data", dataElement.GetAttribute("Datum"));
                                el.SetAttribute("metrBiezacyPoczatkuStrumienia", dataElement.GetAttribute("LfdM"));                          
                                el.SetAttribute("uwaga", dataElement.GetAttribute("Bemerkung"));
                                el.SetAttribute("G", dataElement.GetAttribute("G"));                   
                                el.SetAttribute("V", dataElement.GetAttribute("V"));
                                el.SetAttribute("godzina", dataElement.GetAttribute("Uhr"));
                                pomiar.AppendChild(el);


                                if (dataElement.HasChildNodes)
                                {
                                    XmlNodeList tempWgsList = dataElement.GetElementsByTagName("WGS");
                                    XmlElement wgsSeries = newDoc.CreateElement(rootNew.Prefix, "seriaLokalizacjiGeo", rootNew.NamespaceURI);
                                    el.AppendChild(wgsSeries);

                                    foreach (XmlElement wgsElement in tempWgsList)
                                    {
                                        if (wgsElement.Attributes.Count != 0)
                                        {
                                            XmlElement lokalizacja = newDoc.CreateElement("dsn", "lokalizacja", rootNew.NamespaceURI);
                                            lokalizacja.SetAttribute("data", wgsElement.GetAttribute("Datum"));
                                            lokalizacja.SetAttribute("uwaga", wgsElement.GetAttribute("Bemerkung"));
                                            lokalizacja.SetAttribute("godzina", wgsElement.GetAttribute("Uhr"));
                                            lokalizacja.SetAttribute("mb", wgsElement.GetAttribute("LfdM"));
                                            lokalizacja.SetAttribute("odleglosc", wgsElement.GetAttribute("LfdM"));
                                            wgsSeries.AppendChild(lokalizacja);

                                            XmlElement wspolrzedneGeo = newDoc.CreateElement("dsn", "wspolrzedneGeo", rootNew.NamespaceURI);
                                            lokalizacja.AppendChild(wspolrzedneGeo);

                                            XmlElement pos = newDoc.CreateElement("gml", "pos", "http://www.opengis.net/gml");
                                            pos.SetAttribute("srsName", "WGS84");
                                            pos.InnerText = wgsElement.GetAttribute("L") + " " + wgsElement.GetAttribute("B");
                                            wspolrzedneGeo.AppendChild(pos);
                                        }                       

                                    }

                                 
                                    XmlElement asbElement = (XmlElement)dataElement.FirstChild.NextSibling;
                                    XmlElement bkmElement = (XmlElement)asbElement.NextSibling;
                                    XmlElement zuordnungElement = (XmlElement)bkmElement.NextSibling;

                                    streetClass = zuordnungElement.GetAttribute("Klasse");
                                    streetNr = zuordnungElement.GetAttribute("Nummer");

                                    if (asbElement !=null)
                                    {
                                        int vst = Int32.Parse(asbElement.GetAttribute("VST"));
                                        int bst = Int32.Parse(asbElement.GetAttribute("BST"));
                                        int odleglosc = bst - vst;

                                        string sql = "select * from dane where pref = '" + asbElement.GetAttribute("VNK") + "' and nref = '" + asbElement.GetAttribute("NNK") + "' and odleglosc = " + vst + ";";
                                        loadedData = LoadData(sqlCon, sql);

                                        if (loadedData != null && loadedData.Rows.Count == 0)
                                        {
                                            sql = "select * from dane where pref = '" + asbElement.GetAttribute("VNK") + "' and nref = '" + asbElement.GetAttribute("NNK") + "';";
                                            loadedData = LoadData(sqlCon, sql);
                                        }

                                        XmlElement lokalizacjaSiec = newDoc.CreateElement("dsn", "lokalizacjaSiec", rootNew.NamespaceURI);
                                        //lokalizacjaSiec.SetAttribute("dlugosc", asbElement.GetAttribute("VST"));
                                        lokalizacjaSiec.SetAttribute("dlugosc", Convert.ToString(odleglosc));
                                        lokalizacjaSiec.SetAttribute("kodPRef", asbElement.GetAttribute("VNK"));
                                        lokalizacjaSiec.SetAttribute("numerDrogi", zuordnungElement.GetAttribute("Nummer"));
                                        lokalizacjaSiec.SetAttribute("numerJezdni", numerJezdni);
                                        lokalizacjaSiec.SetAttribute("kierunek", kierunek);
                                        lokalizacjaSiec.SetAttribute("kodNRef", asbElement.GetAttribute("NNK"));
                                        lokalizacjaSiec.SetAttribute("pasRuchu", pasRuchu);
                                        //lokalizacjaSiec.SetAttribute("odleglosc", Convert.ToString(odleglosc));
                                        lokalizacjaSiec.SetAttribute("odleglosc", asbElement.GetAttribute("VST"));
                                        el.AppendChild(lokalizacjaSiec);

                                        if (loadedData != null && loadedData.Rows.Count != 0)
                                        {

                                            pasRuchu = loadedData.Rows[0]["pas_ruchu"].ToString();
                                            numerJezdni = loadedData.Rows[0]["numer_jezdni"].ToString();
                                            kierunek = loadedData.Rows[0]["kierunek"].ToString();
                                            powiat = loadedData.Rows[0]["powiat"].ToString();
                                            if (String.IsNullOrEmpty(wojewodztwo))
                                            {
                                                wojewodztwo = loadedData.Rows[0]["wojewodztwo"].ToString();
                                            }

                                            XmlElement informacjeSieciowe = newDoc.CreateElement("dsn", "informacjeSieciowe", rootNew.NamespaceURI);
                                            informacjeSieciowe.SetAttribute("wojewodztwo", loadedData.Rows[0]["wojewodztwo"].ToString());
                                            informacjeSieciowe.SetAttribute("powiat", powiat);
                                            informacjeSieciowe.SetAttribute("gmina", gmina);
                                            informacjeSieciowe.SetAttribute("oddzial", loadedData.Rows[0]["oddzial"].ToString());
                                            informacjeSieciowe.SetAttribute("rejon", loadedData.Rows[0]["rejon"].ToString());
                                            informacjeSieciowe.SetAttribute("rodzajObszaru", loadedData.Rows[0]["rodzaj_obszaru"].ToString());
                                            el.AppendChild(informacjeSieciowe);

                                        }
                                        else
                                        {
                                            XmlElement informacjeSieciowe = newDoc.CreateElement("dsn", "informacjeSieciowe", rootNew.NamespaceURI);
                                            informacjeSieciowe.SetAttribute("wojewodztwo", "XX");
                                            informacjeSieciowe.SetAttribute("powiat", "XX");
                                            informacjeSieciowe.SetAttribute("gmina", "XX");
                                            informacjeSieciowe.SetAttribute("oddzial", "XXXX");
                                            informacjeSieciowe.SetAttribute("rejon", "XXXX");
                                            informacjeSieciowe.SetAttribute("rodzajObszaru", "XXXXXX");
                                            el.AppendChild(informacjeSieciowe);
                                        }  

                                    }
                                    else
                                    {
                                        XmlElement lokalizacjaSiec = newDoc.CreateElement("dsn", "lokalizacjaSiec", rootNew.NamespaceURI);
                                        lokalizacjaSiec.SetAttribute("dlugosc", "X");
                                        lokalizacjaSiec.SetAttribute("kodPRef", "X");
                                        lokalizacjaSiec.SetAttribute("numerDrogi", "X");
                                        lokalizacjaSiec.SetAttribute("numerJezdni", numerJezdni);
                                        lokalizacjaSiec.SetAttribute("kierunek", kierunek);
                                        lokalizacjaSiec.SetAttribute("kodNRef", "X");
                                        lokalizacjaSiec.SetAttribute("pasRuchu", pasRuchu);
                                        lokalizacjaSiec.SetAttribute("odleglosc", "X");
                                        el.AppendChild(lokalizacjaSiec);


                                        XmlElement informacjeSieciowe = newDoc.CreateElement("dsn", "informacjeSieciowe", rootNew.NamespaceURI);
                                        informacjeSieciowe.SetAttribute("wojewodztwo", "X");
                                        informacjeSieciowe.SetAttribute("powiat", powiat);
                                        informacjeSieciowe.SetAttribute("gmina", gmina);
                                        informacjeSieciowe.SetAttribute("oddzial", "XXXX");
                                        informacjeSieciowe.SetAttribute("rejon", "XXXX");
                                        informacjeSieciowe.SetAttribute("rodzajObszaru", "XXXXXX");
                                        el.AppendChild(informacjeSieciowe);
                                    }


                                    XmlElement zdjecia = newDoc.CreateElement("dsn", "zdjecia", rootNew.NamespaceURI);
                                    el.AppendChild(zdjecia);

                                    XmlNodeList bilderList = dataElement.GetElementsByTagName("Bilder");
                                    if (bilderList != null)
                                    {
                                        XmlElement bilder = (XmlElement)bilderList[0];
                                        if (bilder.HasChildNodes)
                                        {
                                            foreach (XmlElement zdjecie in bilder.ChildNodes)
                                            {

                                                String picFileName = null;
                                                String zdj = zdjecie.GetAttribute("D");

                                                if (File.Exists(picturesPath + "\\" + zdj) )
                                                {
                                                    String[] splitedZDJ = zdj.Split('\\');
                                                    String folder = splitedZDJ[0];
                                                    String fNameOld = splitedZDJ[2];
                                                    String[] splitedNameOld = fNameOld.Split('_');

                                                    if (splitedNameOld[0].Contains("B"))
                                                    {
                                                        picFileName = RenamePictureFile(picturesPath, zdj);
                                                    }
                                                    else
                                                    {
                                                        picFileName = zdj;
                                                    }
                                                    
                                                 
                                                }
                                                else if (File.Exists(makroPath + "\\" + zdj))
                                                {                                   
                                                        picFileName = zdj;

                                                }
                                                else
                                                {
                                                    String[] splitedZDJ = zdj.Split('\\');

                                                    if (splitedZDJ.Length == 3)
                                                    {
                                                        String folder = splitedZDJ[0];
                                                        String fNameOld = splitedZDJ[2];
                                                        String[] splitedNameOld = fNameOld.Split('_');

                                                        if (splitedNameOld[0].Contains("B"))
                                                        {
                                                            String prefixName = splitedNameOld[0].Replace("B", "P");
                                                            String newName = prefixName + "_" + splitedNameOld[1] + "_" + splitedNameOld[2];
                                                            if (File.Exists(picturesPath + "\\" + folder + "\\" + newName))
                                                            {
                                                                picFileName = folder + "\\" + newName;
                                                            }
                                                            else
                                                            {
                                                                picFileName = zdj;
                                                            }
                                                        }
                                                        else if (splitedNameOld[0].Contains("M"))
                                                        {
                                                            if (File.Exists(makroPath + "\\" + zdj))
                                                            {
                                                                picFileName = zdj;
                                                            }
                                                        }

                                                    }
                                                    else if (splitedZDJ.Length == 2)
                                                    {
                                                        String folder = splitedZDJ[0];
                                                        String fNameOld = splitedZDJ[1];
                                                        String[] splitedNameOld = fNameOld.Split('_');

                                                        if (splitedNameOld[0].Contains("B"))
                                                        {
                                                            String prefixName = splitedNameOld[0].Replace("B", "P");
                                                            String newName = prefixName + "_" + splitedNameOld[1] + "_" + splitedNameOld[2];
                                                            if (File.Exists(picturesPath + "\\" + folder + "\\" + newName))
                                                            {
                                                                picFileName = folder + "\\" + newName;
                                                            }
                                                            else
                                                            {
                                                                picFileName = zdj;
                                                            }
                                                        }
                                                        else if (splitedNameOld[0].Contains("M"))
                                                        {
                                                            if (File.Exists(makroPath + "\\" + zdj))
                                                            {
                                                                picFileName = zdj;
                                                            }
                                                        }

                                                    }


                                                }

                                                XmlElement zd = newDoc.CreateElement("dsn", "zdjecie", rootNew.NamespaceURI);
                                                //zd.SetAttribute("plik", zdjecie.GetAttribute("D"));
                                                zd.SetAttribute("plik", picFileName);
                                                zd.SetAttribute("nrKamery", zdjecie.GetAttribute("Nr"));
                                                if (zdjecie.GetAttribute("A") != null && !String.IsNullOrEmpty(zdjecie.GetAttribute("A")))
                                                {
                                                    zd.SetAttribute("A", zdjecie.GetAttribute("A"));
                                                }
                                                zd.SetAttribute("odleglosc", zdjecie.GetAttribute("Station"));
                                                zdjecia.AppendChild(zd);
                                            }
                                        }
                                    }


                                    XmlNodeList asphaltList = dataElement.GetElementsByTagName("Asphalt");
                                    if (asphaltList != null && asphaltList.Count != 0)
                                    {
                                        foreach(XmlElement asElement in asphaltList){
                                            foreach(XmlElement rElement in asElement.ChildNodes ){

                                                XmlElement asfalt = newDoc.CreateElement("dsn", "asfalt", rootNew.NamespaceURI);
                                                el.AppendChild(asfalt);
                                                XmlElement rAsfalt = newDoc.CreateElement("dsn", "R", rootNew.NamespaceURI);
                                                rAsfalt.SetAttribute("A", rElement.GetAttribute("A"));
                                                rAsfalt.SetAttribute("SSP", rElement.GetAttribute("RISS"));
                                                rAsfalt.SetAttribute("LA_W", rElement.GetAttribute("EFLI"));
                                                rAsfalt.SetAttribute("LA_N", rElement.GetAttribute("AFLI"));
                                                rAsfalt.SetAttribute("LA", rElement.GetAttribute("FLI"));
                                                rAsfalt.SetAttribute("WYB", rElement.GetAttribute("AUS"));
                                                rAsfalt.SetAttribute("NST", rElement.GetAttribute("ONA"));
                                                rAsfalt.SetAttribute("NL", rElement.GetAttribute("BIN"));
                                                asfalt.AppendChild(rAsfalt);
                                            }
                                        }
                                    }



                                }//dataElement.HasChildNodes
                            }

                         }

                         

                         bool val = ValidateDsnXML(newDoc);
                         if (val)
                         {
                             DisplayData("Document is valid.");
                         }
                         else
                         {
                             DisplayData("Document is not valid.");
                         }


                        //newDoc.Save("C:\\test\\generated_" + fileName);
                         String fileNameFinal = createFileName(streetClass, streetNr, wojewodztwo, type);

                         if (outputFolder != null)
                         {
                             saveFile(newDoc, outputFolder, fileNameFinal, type);
                         }
                         else
                         {
                             saveFile(newDoc, path, fileNameFinal, type);
                         }

                        break;
                    }
            }
           
        }

        public static XmlDocument RemoveXmlns(XmlDocument doc)
        {
            XDocument d;
            using (var nodeReader = new XmlNodeReader(doc))
                d = XDocument.Load(nodeReader);

            d.Root.Descendants().Attributes().Where(x => x.Equals("xmlns: gml = \"gml\"")).Remove();
            d.Root.Descendants().Attributes().Where(x => x.Equals("xmlns:dsn=\"dsn\"")).Remove();

            foreach (var elem in d.Descendants())
                elem.Name = elem.Name.LocalName;

            var xmlDocument = new XmlDocument();
            using (var xmlReader = d.CreateReader())
                xmlDocument.Load(xmlReader);

            return xmlDocument;
        }

        public static XmlDocument RemoveXmlns(String xml)
        {
            XDocument d = XDocument.Parse(xml);
            //d.Root.Descendants().Attributes().Where(x => x.ToString().Contains("xmlns:dsn=\"dsn\"")).ToString().Replace("xmlns:dsn=\"dsn\"", "");

            foreach (XElement n in d.Root.Descendants())
            {
                foreach(XAttribute att in n.Attributes()){
                    if (att.ToString().Equals("xmlns:dsn=\"dsn\""))
                    {
                        n.ReplaceAttributes(att, "");
                    }
                }
            }

            //foreach (XAttribute att in d.Root.Descendants().Attributes())
            //{
            //    if (att.ToString().Equals("xmlns:dsn=\"dsn\""))
            //    {
            //        Console.WriteLine("Equals");
            //        att.Remove();
            //    }
            //}

            //foreach (var elem in d.Descendants())
            //    elem.Name = elem.Name.LocalName;

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(d.CreateReader());

            return xmlDocument;
        }


        private XmlDocument removeXMLNS(XmlDocument doc)
        {
            XmlDocument newDoc = null;
            if (doc != null)
            {
                String docString = doc.OuterXml;
                docString.Replace("xmlns: gml = \"gml\"", "");
                docString.Replace("xmlns:dsn=\"dsn\"", "");

                Console.WriteLine(docString);

                newDoc = new XmlDocument();
                newDoc.Load(docString);

                
            }

            return newDoc;
        }
       

        private void toolStripMenuItemClose_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void toolStripMenuItemOpenFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1 = new FolderBrowserDialog();
            toolStripProgressBar1.Value = 0;
            filesList = new Dictionary<string, string>();

            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                String directory = folderBrowserDialog1.SelectedPath;
                DisplayData("Opened directory: " + directory);
                // toolStripStatusLabel1.Text = "Opened directory: " + directory;

                String[] filesXML = Directory.GetFiles(directory, "*.xml");
                if (filesXML != null && filesXML.Length != 0)
                {
                    int maxValue = filesXML.Length;
                    toolStripProgressBar1.Maximum = maxValue;

                    foreach (String filePath in filesXML)
                    {
                        String fileName = filePath;
                        String[] splitedName = fileName.Split('\\');
                        String fileNameWithoutPath = splitedName[splitedName.Length - 1];
                        filesList.Add(fileName, fileNameWithoutPath);
                    }

                    testStartButton();
                }
                else
                {
                    String[] directories = Directory.GetDirectories(directory);

                    foreach (String dir in directories)
                    {
                        String[] filesInDir = Directory.GetFiles(dir, "*.xml");

                        foreach (String filePath in filesInDir)
                        {
                            String fileName = filePath;
                            String[] splitedName = fileName.Split('\\');
                            String fileNameWithoutPath = splitedName[splitedName.Length - 1];
                            filesList.Add(fileName, fileNameWithoutPath);
                        }

                    }

                    testStartButton();

                }

                // Thread backgroundThread = new Thread(
                //      new ThreadStart(() =>
                //      {

                //foreach (String filePath in filesXML)
                //{

                //    openXmlFile(filePath);
                //    count++;

                //    UpdateProgressBar(count);
                //   // toolStripProgressBar1.Value = count;
                //}

                //toolStripStatusLabel1.Text = "Finished";

                //if (sqlCon != null && sqlCon.State == ConnectionState.Open)
                //{
                //    sqlCon.Close();
                //}

                //      }
                //          ));

                // backgroundThread.Start();

            }
        }

        #region DisplayData
        /// <summary>
        /// method to display the data to & from the port
        /// on the screen
        /// </summary>
        /// <param name="type">MessageType of the message</param>
        /// <param name="msg">Message to display</param>
        [STAThread]
        private void DisplayData(string msg)
        {
            try
            {
                richTextBox1.Invoke(new EventHandler(delegate
                {
                    richTextBox1.SelectedText = string.Empty;
                    richTextBox1.SelectionFont = new Font(richTextBox1.SelectionFont, FontStyle.Bold);

                    richTextBox1.AppendText(msg + Environment.NewLine);
                    richTextBox1.ScrollToCaret();
                }));

            }
            catch (InvalidOperationException ie)
            {
                Console.WriteLine(ie);
            }

        }
        #endregion



        public void UpdateProgressBar(int value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<int>(UpdateProgressBar), new object[] { value });
                return;
            }
            toolStripProgressBar1.Value = value;
        }

        public void SetProgresMaximum(int value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<int>(SetProgresMaximum), new object[] { value });
                return;
            }
            toolStripProgressBar1.Maximum = value;
        }

        public void EnableButtonStartPhoto(bool value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<bool>(EnableButtonStartPhoto), new object[] { value });
                return;
            }
            toolStripButtonProcessPhoto.Enabled = value;
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sqlCon != null && sqlCon.State == ConnectionState.Open)
            {
                sqlCon.Close();
            }
        }

        private void toolStripButtonOpenFile_Click(object sender, EventArgs e)
        {
            openFileDialog1 = new OpenFileDialog();
            toolStripProgressBar1.Value = 0;
            filesList = new Dictionary<string, string>();

            DialogResult result = openFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                String fileName = openFileDialog1.FileName;
                String fileNameWithoutPath = openFileDialog1.SafeFileName;
                toolStripStatusLabel1.Text = "Prepare to open file: " + fileNameWithoutPath;
                DisplayData("Prepare to open file: " + fileNameWithoutPath);
                filesList.Add(fileName, fileNameWithoutPath);
                testStartButton();
                //Thread backgroundThread = new Thread(
                //         new ThreadStart(() =>
                //         {

                //             openXmlFile(fileName);
                //             UpdateProgressBar(100);
                //             toolStripStatusLabel1.Text = "Finished";

                //             if (sqlCon != null && sqlCon.State == ConnectionState.Open)
                //             {
                //                 sqlCon.Close();
                //             }

                //         }
                //             ));

                //backgroundThread.Start();


            }
        }


        private void testStartButton()
        {
            this.Invoke(new EventHandler(delegate
                {
                    if (filesList != null && filesList.Count != 0)
                    {
                        toolStripButtonStart.Enabled = true;
                    }
                    else
                    {
                        toolStripButtonStart.Enabled = false;
                    }
                }));   
        }

        private void testStartPhotoButton()
        {
            this.Invoke(new EventHandler(delegate
            {
                if (roadList != null && roadList.Count != 0)
                {
                    toolStripButtonProcessPhoto.Enabled = true;
                }
                else
                {
                    toolStripButtonProcessPhoto.Enabled = false;
                }
            }));
        }

        private void toolStripButtonOpenFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1 = new FolderBrowserDialog();
            toolStripProgressBar1.Value = 0;
            filesList = new Dictionary<string, string>();

            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                String directory = folderBrowserDialog1.SelectedPath;
                DisplayData("Opened directory: " + directory);
               // toolStripStatusLabel1.Text = "Opened directory: " + directory;

                String[] filesXML = Directory.GetFiles(directory, "*.xml");
                if (filesXML != null && filesXML.Length != 0)
                {
                    int maxValue = filesXML.Length;
                    toolStripProgressBar1.Maximum = maxValue;

                    foreach (String filePath in filesXML)
                    {
                        String fileName = filePath;
                        String[] splitedName = fileName.Split('\\');
                        String fileNameWithoutPath = splitedName[splitedName.Length - 1];
                        filesList.Add(fileName, fileNameWithoutPath);
                    }

                    testStartButton();
                }
                else
                {
                    String[] directories = Directory.GetDirectories(directory);

                    foreach(String dir in directories){
                        String[] filesInDir = Directory.GetFiles(dir, "*.xml");

                        foreach (String filePath in filesInDir)
                        {
                            String fileName = filePath;
                            String[] splitedName = fileName.Split('\\');
                            String fileNameWithoutPath = splitedName[splitedName.Length - 1];
                            filesList.Add(fileName, fileNameWithoutPath);
                        }

                    }

                    int maxValue = filesList.Count;
                    toolStripProgressBar1.Maximum = maxValue;

                    testStartButton();

                }
               // int count = 0;
               

                // Thread backgroundThread = new Thread(
                //      new ThreadStart(() =>
                //      {

                //foreach (String filePath in filesXML)
                //{

                //    openXmlFile(filePath);
                //    count++;

                //    UpdateProgressBar(count);
                //   // toolStripProgressBar1.Value = count;
                //}

                //toolStripStatusLabel1.Text = "Finished";

                //if (sqlCon != null && sqlCon.State == ConnectionState.Open)
                //{
                //    sqlCon.Close();
                //}

                //      }
                //          ));

                // backgroundThread.Start();

            }
        }

        private void toolStripButtonOutputFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialogOutput = new FolderBrowserDialog();
            DialogResult result = folderBrowserDialogOutput.ShowDialog();
           // toolStripTextBoxOutputFolder.Text = "";

            if (result == DialogResult.OK)
            {
                outputFolder = folderBrowserDialogOutput.SelectedPath;
                toolStripTextBoxOutputFolder.Text = outputFolder;
                DisplayData("Selected output folder: "+outputFolder);
            }
        }

        private void toolStripTextBoxOutputFolder_TextChanged(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(((ToolStripTextBox)sender).Text))
            {
                outputFolder = ((ToolStripTextBox)sender).Text;
            }
            else
            {
                outputFolder = null;
            }
            
        }

        private void toolStripButtonStart_Click(object sender, EventArgs e)
        {

            toolStripProgressBar1.Maximum = filesList.Count;
            int count = 0;
            Thread backgroundThread = new Thread(
                  new ThreadStart(() =>
                  {
                      
                      foreach(KeyValuePair<String, String> filePair in filesList){
                          String fileName = filePair.Key;
                          String fileNameWithoutPath = filePair.Value;

                          openXmlFile(fileName);
                          //UpdateProgressBar(count);
                          count++;
                          UpdateProgressBar(count);
                      }

                     // openXmlFile(fileName);
                     // UpdateProgressBar(100);
                      toolStripStatusLabel1.Text = "Finished";

                      if (sqlCon != null && sqlCon.State == ConnectionState.Open)
                      {
                          sqlCon.Close();
                      }

                      filesList = null;
                      testStartButton();

                  }
                      ));

            backgroundThread.Start();
        }

        private void toolStripMenuItemGmlId_Click(object sender, EventArgs e)
        {
            DatabaseConnector dc = new DatabaseConnector();
            dc.Show();
        }


        private void savePhotoXml(XmlDocument newDoc, String outputFolder, String fileName)
        {
            if (!Directory.Exists(outputFolder + "\\PP-F"))
            {
                Directory.CreateDirectory(outputFolder + "\\PP-F");
            }

            newDoc.Save(outputFolder + "\\PP-F\\"+fileName);

        }

        private void createMessfahrtFolder(String outputFolder, String messfahrtNr)
        {
            if (outputFolder != null && Directory.Exists(outputFolder))
            {
                
                if (!Directory.Exists(outputFolder + "\\" + messfahrtNr))
                {
                    Directory.CreateDirectory(outputFolder + "\\" + messfahrtNr);
                }
                
            }
        }

        private void ProcessPhoto2(Dictionary<String, String> roadDictionary, String outputPath)
        {
            if (roadDictionary != null && roadDictionary.Count != 0)
            {
                List<String> rList = roadDictionary.Values.Distinct().ToList();
                List<String> pathListForOneRoad;

                int count = 0;

                SetProgresMaximum(rList.Count);
               // toolStripProgressBar1.Maximum = rList.Count;

              foreach(string road in rList)
              {
                  odleglosci = new List<int>();
                  pathListForOneRoad = new List<string>();
                  List<String> messfahrtForRoad = new List<string>();
                  Console.WriteLine(road);

                  foreach (KeyValuePair<String, String> pair in roadDictionary)
                  {
                      if (pair.Value.Equals(road))
                      {
                          String messf = pair.Key.Split('?')[1];
                          if (!messfahrtForRoad.Contains(messf))
                          {
                              messfahrtForRoad.Add(messf);
                          }
                          pathListForOneRoad.Add(pair.Key);
                      }
                  }



                  String wojewodztwo = null;
                  XmlDocument newDoc = new XmlDocument();
                  XmlDeclaration xmlDeclaration = newDoc.CreateXmlDeclaration("1.0", "UTF-8", null);

                  XmlElement rootOld = fotoHeader.DocumentElement;

                  XmlElement rootNew = newDoc.CreateElement("dsn", "daneElementarnePPFsiec", "http://www.gddkia.gov.pl/dsn/1.0.0");
                  rootNew.SetAttribute("xmlns:dsn", "http://www.gddkia.gov.pl/dsn/1.0.0");
                  rootNew.SetAttribute("xmlns:gml", "http://www.opengis.net/gml");
                  rootNew.SetAttribute("xmlns:sch", "http://www.ascc.net/xml/schematron");
                  rootNew.SetAttribute("xmlns:xlink", "http://www.w3.org/1999/xlink");
                  rootNew.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
                  rootNew.SetAttribute("schemaLocation", "http://www.w3.org/2001/XMLSchema-instance", "http://www.gddkia.gov.pl/dsn/1.0.0/dane_elementarne.xsd");
                  rootNew.SetAttribute("uwaga", rootOld.GetAttribute("uwaga") == null ? "" : rootOld.GetAttribute("uwaga"));
                  rootNew.SetAttribute("rodzaj", "sieciowe");
                  // rootNew.SetAttribute("cecha", rootOld.GetAttribute("Merkmal") == null ? "" : rootOld.GetAttribute("Merkmal"));
                  rootNew.SetAttribute("dataUtworzenia", rootOld.GetAttribute("dataUtworzenia") == null ? "" : rootOld.GetAttribute("dataUtworzenia"));
                  newDoc.AppendChild(xmlDeclaration);
                  newDoc.AppendChild(rootNew);

                  XmlElement pomiar = newDoc.CreateElement("dsn", "przejazdPomiarowy", rootNew.NamespaceURI);
                  //XmlAttribute t = newDoc.CreateAttribute("gml", "id", "http://www.opengis.net/gml");
                  //String gmlID = GenerateGmlId(messfahrt);
                  //t.Value = gmlID;
                 // pomiar.SetAttributeNode(t);

                  rootNew.AppendChild(pomiar);



                  XmlNodeList hList = fotoHeader.GetElementsByTagName("dsn:naglowekPrzejazduPomiarowego");
                  if (hList != null && hList.Count != 0)
                  {
                      XmlElement headerNode = (XmlElement)hList[0];

                      //headerNode.Attributes["gml:id"].Value = increaseGmlID(gmlID);

                      XmlNodeList liMetadane = headerNode.GetElementsByTagName("dsn:metadane");
                      if (liMetadane != null)
                      {
                          XmlElement metadane = (XmlElement)liMetadane[0];
                          wojewodztwo = metadane.GetAttribute("wojewodztwo");
                      }

                      // pomiar.AppendChild(headerNode);
                      XmlNode hNode = newDoc.ImportNode(headerNode, true);
                      pomiar.AppendChild(hNode);
                  }


                  XmlDocument saDoc =null;
                  foreach(string path in pathListForOneRoad){

                      String dirPath = path.Split('?')[0];

                      if (Directory.Exists(dirPath))
                      {
                          String picSA = dirPath + "pic_sa.xml";

                          if (File.Exists(picSA))
                          {
                              saDoc = new XmlDocument();
                              saDoc.Load(picSA);

                              newDoc = AddGeoLokalisation(newDoc, saDoc);

                          }
                      }

                  }

                  newDoc = AddNetInfo(newDoc, saDoc, wojewodztwo, road);

                  foreach (string path in pathListForOneRoad)
                  {

                      String dirPath = path.Split('?')[0];
                      String messfahrt = path.Split('?')[1];

                      if (Directory.Exists(dirPath))
                      {
                          String picSA = dirPath + "pic_sa.xml";

                          if (File.Exists(picSA))
                          {
                              saDoc = new XmlDocument();
                              saDoc.Load(picSA);

                              newDoc = AddPhoto(newDoc, saDoc, outputPath, messfahrt);

                          }
                      }

                  }

                  String finalName = CreatePhotoXmlName(road, wojewodztwo, kierunek);
                  savePhotoXml(newDoc, outputFolder, finalName);
                  //newDoc.Save("C:\\test\\fotoTest.xml");

                  count++;
                  UpdateProgressBar(count);

              }//foreach road
               
            }
        }

        private String CreatePhotoXmlName(String road, String wojewodztwo, String kierunek)
        {
            String newName = null;
            String extension = "_PP-F_.xml";
            String kier = null;

            if (kierunek.ToLower().Equals("zgodnie"))
            {
                kier = "z";
            }
            else
            {
                kier = "p";
            }

            String[] splitedRoad = road.Split(' ');
            String streetClass = splitedRoad[0];
            String streetNr = splitedRoad[1];
            String finalStreet = null;

            if (streetNr.Length == 1)
            {
                finalStreet = "____" + streetClass + streetNr + "__";
            }
            else if (streetNr.Length == 2)
            {
                finalStreet = "____" + streetClass + streetNr + "_";
            }
            else if (streetNr.Length == 3)
            {
                finalStreet = "____" + streetClass + streetNr;
            }

           // +"_" + numerJezdni + pasRuchu + kier + extension;
            newName = "S" + wojewodztwo + finalStreet + "_" + numerJezdni + pasRuchu + kier + extension; 

            return newName;
        }

        private XmlDocument AddPhoto(XmlDocument newDoc, XmlDocument picSa, String outputFolder, String messfahrt)
        {
            XmlElement rootNew = newDoc.DocumentElement;
            XmlElement rootSa = picSa.DocumentElement;
            XmlElement pomiar = (XmlElement)newDoc.GetElementsByTagName("dsn:przejazdPomiarowy")[0];
            XmlElement strumienDanych = (XmlElement)newDoc.GetElementsByTagName("dsn:strumenDanych")[0];

            if(strumienDanych == null)
            {
                XmlElement firstIndexEl = (XmlElement)rootSa.FirstChild;
                DateTime data = DateTime.Parse(firstIndexEl.GetElementsByTagName("Datum")[0].InnerText);

                strumienDanych = newDoc.CreateElement("dsn", "strumenDanych", rootNew.NamespaceURI);
                strumienDanych.SetAttribute("data", data.ToShortDateString());
                strumienDanych.SetAttribute("metrBiezacyPoczatkuStrumienia", "0");
                strumienDanych.SetAttribute("uwaga", "");
                strumienDanych.SetAttribute("G", "0");
                strumienDanych.SetAttribute("V", "0");
                strumienDanych.SetAttribute("godzina", data.ToLongTimeString());
                pomiar.AppendChild(strumienDanych);
            }

            XmlElement dsnZdjecia = (XmlElement)newDoc.GetElementsByTagName("dsn:zdjecia")[0];
            if (dsnZdjecia == null)
            {
                dsnZdjecia = newDoc.CreateElement("dsn", "zdjecia", rootNew.NamespaceURI);
                strumienDanych.AppendChild(dsnZdjecia);
            }

            List<XmlElement> kamFront = new List<XmlElement>();
            List<XmlElement> kamRetro = new List<XmlElement>();
            List<XmlElement> kamLeft = new List<XmlElement>();
            List<XmlElement> kamRight = new List<XmlElement>();

            if (rootSa.HasChildNodes)
            {

                XmlNodeList picIndexList = rootSa.GetElementsByTagName("PicIndex");

                foreach (XmlElement element in picIndexList)
                {
                    XmlElement kameraOznaczenie = (XmlElement)element.GetElementsByTagName("Buchst")[0];
                   // XmlElement picName = (XmlElement)element.GetElementsByTagName("Filename")[0];
                   // XmlElement picPath = (XmlElement)element.GetElementsByTagName("PicPath")[0];

                    if (kameraOznaczenie.InnerText.Equals("G"))
                    {
                        kamRetro.Add(element);
                    }
                    else if (kameraOznaczenie.InnerText.Equals("S"))
                    {
                        kamLeft.Add(element);
                    }
                    else if (kameraOznaczenie.InnerText.Equals("V"))
                    {
                        kamRight.Add(element);
                    }
                    else if (kameraOznaczenie.InnerText.Equals("X"))
                    {
                        kamFront.Add(element);
                    }

                }

                for (int i = 0; i < kamFront.Count; i++)
                {

                    //wpis dla kamery przod
                    try 
                    {
                          
                          XmlElement zdF = kamFront[i];
                          if (zdF != null)
                          {
                              string fileName = zdF.GetElementsByTagName("Filename")[0].InnerText;
                              string actDirPic = zdF.GetElementsByTagName("PicPath")[0].InnerText;
                              string station = zdF.GetElementsByTagName("Station")[0].InnerText;

                              String finalOldFile = directoryPhoto+"\\" + actDirPic + fileName;
                              String newFile = null;
                              String plik = null;

                              if (File.Exists(finalOldFile))
                              {
                                  newFile = RenamePhoto(fileName, messfahrt, "Front");
                                  plik = MovePhoto("Front", messfahrt, directoryPhoto, finalOldFile, newFile);
                              }

                              XmlElement zdKamFront = newDoc.CreateElement("dsn", "zdjecie", rootNew.NamespaceURI);
                                         zdKamFront.SetAttribute("plik", plik);
                                         zdKamFront.SetAttribute("nrKamery", "1");
                                         zdKamFront.SetAttribute("A", "X");
                                         zdKamFront.SetAttribute("odleglosc", station);
                                         dsnZdjecia.AppendChild(zdKamFront);
                                
                          }


                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e);
                    }  

        
                    // wpis dla kamery lewej
                     try 
                    {
                          XmlElement zdL = kamLeft[i];
                          if (zdL != null)
                          {
                              string fileName = zdL.GetElementsByTagName("Filename")[0].InnerText;
                              string actDirPic = zdL.GetElementsByTagName("PicPath")[0].InnerText;
                              string station = zdL.GetElementsByTagName("Station")[0].InnerText;

                              String finalOldFile = directoryPhoto + "\\" + actDirPic + fileName;
                              String newFile = null;
                              String plik = null;

                              if (File.Exists(finalOldFile))
                              {
                                  newFile = RenamePhoto(fileName, messfahrt, "Lewa");
                                  plik = MovePhoto("Lewa", messfahrt, directoryPhoto, finalOldFile, newFile);
                              }

                              XmlElement zdKamLeft = newDoc.CreateElement("dsn", "zdjecie", rootNew.NamespaceURI);
                              zdKamLeft.SetAttribute("plik", plik);
                              zdKamLeft.SetAttribute("nrKamery", "2");
                              zdKamLeft.SetAttribute("A", "X");
                              zdKamLeft.SetAttribute("odleglosc", station);
                              dsnZdjecia.AppendChild(zdKamLeft);
                          }

                    }
                     catch (Exception e)
                     {
                         Console.WriteLine(e);
                     }  


                    //wpis dla kamery prawej
                     try 
                     {
                          XmlElement zdR = kamRight[i];
                          if (zdR != null)
                          {
                              string fileName = zdR.GetElementsByTagName("Filename")[0].InnerText;
                              string actDirPic = zdR.GetElementsByTagName("PicPath")[0].InnerText;
                              string station = zdR.GetElementsByTagName("Station")[0].InnerText;

                              String finalOldFile = directoryPhoto + "\\" + actDirPic + fileName;
                              String newFile = null;
                              String plik = null;

                              if (File.Exists(finalOldFile))
                              {
                                  newFile = RenamePhoto(fileName, messfahrt, "Prawa");
                                  plik = MovePhoto("Prawa", messfahrt, directoryPhoto, finalOldFile, newFile);
                              }

                              XmlElement zdKamRight = newDoc.CreateElement("dsn", "zdjecie", rootNew.NamespaceURI);
                              zdKamRight.SetAttribute("plik", plik);
                              zdKamRight.SetAttribute("nrKamery", "3");
                              zdKamRight.SetAttribute("A", "X");
                              zdKamRight.SetAttribute("odleglosc", station);
                              dsnZdjecia.AppendChild(zdKamRight);
                          }
                     }
                     catch (Exception e)
                     {
                         Console.WriteLine(e);
                     }  


                    //wpis dla kamert tylnej
                     try
                     {
                         XmlElement zdT = kamRetro[i];
                         if(zdT !=null){

                             string fileName = zdT.GetElementsByTagName("Filename")[0].InnerText;
                             string actDirPic = zdT.GetElementsByTagName("PicPath")[0].InnerText;
                             string station = zdT.GetElementsByTagName("Station")[0].InnerText;

                             String finalOldFile = directoryPhoto + "\\" + actDirPic + fileName;
                             String newFile = null;
                             String plik = null;

                             if (File.Exists(finalOldFile))
                             {
                                 newFile = RenamePhoto(fileName, messfahrt, "Tylna");
                                 plik = MovePhoto("Tylna", messfahrt, directoryPhoto, finalOldFile, newFile);
                             }

                             XmlElement zdKamRetro = newDoc.CreateElement("dsn", "zdjecie", rootNew.NamespaceURI);
                             zdKamRetro.SetAttribute("plik", plik);
                             zdKamRetro.SetAttribute("nrKamery", "4");
                             zdKamRetro.SetAttribute("A", "X");
                             zdKamRetro.SetAttribute("odleglosc", station);
                             dsnZdjecia.AppendChild(zdKamRetro);
                         }
                     
                     }catch(IndexOutOfRangeException ex)
                     {
                         Console.WriteLine(ex);
                         continue;

                     }catch(Exception e){
                         Console.WriteLine(e);
                     }


                }//for kamFront

            }

            return newDoc;
        }

        private XmlDocument AddNetInfo(XmlDocument newDoc, XmlDocument picSa, String wojewodztwo, String road)
        {
            XmlElement rootNew = newDoc.DocumentElement;
            XmlElement rootSa = picSa.DocumentElement;
            XmlElement pomiar = (XmlElement)newDoc.GetElementsByTagName("dsn:przejazdPomiarowy")[0];

            XmlElement strumienDanych = (XmlElement)newDoc.GetElementsByTagName("dsn:strumenDanych")[0];
            
                if (strumienDanych == null)
                {
                    XmlElement firstIndexEl = (XmlElement)rootSa.FirstChild;
                    DateTime data = DateTime.Parse(firstIndexEl.GetElementsByTagName("Datum")[0].InnerText);

                    strumienDanych = newDoc.CreateElement("dsn", "strumenDanych", rootNew.NamespaceURI);
                    strumienDanych.SetAttribute("data", data.ToShortDateString());
                    strumienDanych.SetAttribute("metrBiezacyPoczatkuStrumienia", "0");
                    strumienDanych.SetAttribute("uwaga", "");
                    strumienDanych.SetAttribute("G", "0");
                    strumienDanych.SetAttribute("V", "0");
                    strumienDanych.SetAttribute("godzina", data.ToLongTimeString());
                    pomiar.AppendChild(strumienDanych);
                }

                if (odleglosci != null && odleglosci.Count != 0)
                {
                    odleglosci = odleglosci.Distinct().OrderBy(v => v).ToList();
                }

                XmlNodeList picIndexList = rootSa.GetElementsByTagName("PicIndex");
                String vnk = null;
                XmlNodeList vnkList = rootSa.GetElementsByTagName("VNK");
                vnk = vnkList[0].InnerText;

                String nnk = null;
                XmlNodeList nnkList = rootSa.GetElementsByTagName("NNK");
                nnk = nnkList[0].InnerText;


                string sql = "select * from dane where pref = '" + vnk + "' and nref = '" + nnk + "' ;";
                loadedData = LoadData(sqlCon ,sql);

                 if (loadedData != null && loadedData.Rows.Count != 0)
                 {
                      pasRuchu = loadedData.Rows[0]["pas_ruchu"].ToString();
                      numerJezdni = loadedData.Rows[0]["numer_jezdni"].ToString();
                      kierunek = loadedData.Rows[0]["kierunek"].ToString();
                      powiat = loadedData.Rows[0]["powiat"].ToString();
                         if (String.IsNullOrEmpty(wojewodztwo))
                         {
                                wojewodztwo = loadedData.Rows[0]["wojewodztwo"].ToString();
                         }

                         
                         int dlugosc = odleglosci[odleglosci.Count -1] - odleglosci[0];
                         string[] splitedRoad = road.Split(' ');
                         string roadNrWithoutClass = splitedRoad[1];

                         XmlElement lokalizacjaSiec = newDoc.CreateElement("dsn", "lokalizacjaSiec", rootNew.NamespaceURI);
                         // lokalizacjaSiec.SetAttribute("dlugosc", asbElement.GetAttribute("VST"));
                         lokalizacjaSiec.SetAttribute("dlugosc", Convert.ToString(dlugosc));

                         lokalizacjaSiec.SetAttribute("kodPRef", vnk);
                         lokalizacjaSiec.SetAttribute("numerDrogi", roadNrWithoutClass);
                         lokalizacjaSiec.SetAttribute("numerJezdni", numerJezdni);
                         lokalizacjaSiec.SetAttribute("kierunek", kierunek);
                         lokalizacjaSiec.SetAttribute("kodNRef", nnk);
                         lokalizacjaSiec.SetAttribute("pasRuchu", pasRuchu);
                         lokalizacjaSiec.SetAttribute("odleglosc", Convert.ToString(odleglosci[odleglosci.Count - 1]));
                         strumienDanych.AppendChild(lokalizacjaSiec);

                         XmlElement informacjeSieciowe = newDoc.CreateElement("dsn", "informacjeSieciowe", rootNew.NamespaceURI);
                         informacjeSieciowe.SetAttribute("wojewodztwo", loadedData.Rows[0]["wojewodztwo"].ToString());
                         informacjeSieciowe.SetAttribute("powiat", powiat);
                         informacjeSieciowe.SetAttribute("gmina", gmina);
                         informacjeSieciowe.SetAttribute("oddzial", loadedData.Rows[0]["oddzial"].ToString());
                         informacjeSieciowe.SetAttribute("rejon", loadedData.Rows[0]["rejon"].ToString());
                         informacjeSieciowe.SetAttribute("rodzajObszaru", loadedData.Rows[0]["rodzaj_obszaru"].ToString());
                         strumienDanych.AppendChild(informacjeSieciowe);

                }

                 return newDoc;
            
        }
     

        private XmlDocument AddGeoLokalisation(XmlDocument newDoc, XmlDocument picSa)
        {
            XmlElement rootNew = newDoc.DocumentElement;
            XmlElement rootSa = picSa.DocumentElement;
            XmlElement pomiar = (XmlElement)newDoc.GetElementsByTagName("dsn:przejazdPomiarowy")[0];
            XmlElement firstIndexEl = (XmlElement)rootSa.FirstChild;
            DateTime data = DateTime.Parse(firstIndexEl.GetElementsByTagName("Datum")[0].InnerText);

            XmlElement strumienDanych = (XmlElement)newDoc.GetElementsByTagName("dsn:strumenDanych")[0];
            if(strumienDanych == null)
            {
                strumienDanych = newDoc.CreateElement("dsn", "strumenDanych", rootNew.NamespaceURI);
                strumienDanych.SetAttribute("data", data.ToShortDateString());
                strumienDanych.SetAttribute("metrBiezacyPoczatkuStrumienia", "0");
                strumienDanych.SetAttribute("uwaga", "");
                strumienDanych.SetAttribute("G", "0");
                strumienDanych.SetAttribute("V", "0");
                strumienDanych.SetAttribute("godzina", data.ToLongTimeString());
                pomiar.AppendChild(strumienDanych);
            }

            XmlElement seriaLokalizacjiGeo;
            XmlNodeList geoLoc = newDoc.GetElementsByTagName("dsn:seriaLokalizacjiGeo");

            if(geoLoc != null && geoLoc.Count !=0){
                seriaLokalizacjiGeo = (XmlElement)geoLoc[0];

            }else{
                
                seriaLokalizacjiGeo = newDoc.CreateElement("dsn", "seriaLokalizacjiGeo", rootNew.NamespaceURI);
                strumienDanych.AppendChild(seriaLokalizacjiGeo);
            }

            

            if (rootSa.HasChildNodes)
            {

                XmlNodeList picIndexList = rootSa.GetElementsByTagName("PicIndex");
                String vnk = null;
                XmlNodeList vnkList = rootSa.GetElementsByTagName("VNK");
                vnk = vnkList[0].InnerText;

                String nnk = null;
                XmlNodeList nnkList = rootSa.GetElementsByTagName("NNK");
                nnk = nnkList[0].InnerText;


                foreach (XmlElement element in picIndexList)
                {


                    XmlElement lat = null;
                    XmlNodeList latList = element.GetElementsByTagName("LAT");
                    if (latList != null && latList.Count != 0)
                    {
                        lat = (XmlElement)latList[0];
                    }

                    XmlElement lon = null;
                    XmlNodeList lonList = element.GetElementsByTagName("LON");
                    if (lonList != null && lonList.Count != 0)
                    {
                        lon = (XmlElement)lonList[0];
                    }

                    XmlElement datum = null;
                    XmlNodeList datumList = element.GetElementsByTagName("Datum");
                    if (datumList != null && datumList.Count != 0)
                    {
                        datum = (XmlElement)datumList[0];
                    }

                    XmlElement station = null;
                    XmlNodeList stationList = element.GetElementsByTagName("Station");
                    if (stationList != null && stationList.Count != 0)
                    {
                        station = (XmlElement)stationList[0];
                        odleglosci.Add(Int32.Parse(station.InnerText));
                    }

                    String test = datum.InnerText;
                    DateTime dt = DateTime.Parse(test);

                    XmlElement lokalizacja = newDoc.CreateElement("dsn", "lokalizacja", rootNew.NamespaceURI);
                    lokalizacja.SetAttribute("data", dt.ToShortDateString());
                    lokalizacja.SetAttribute("uwaga", "");
                    lokalizacja.SetAttribute("godzina", dt.ToLongTimeString());
                    lokalizacja.SetAttribute("mb", station == null || station.InnerText == null ? "" : station.InnerText);
                    lokalizacja.SetAttribute("odleglosc", station == null || station.InnerText == null ? "" : station.InnerText);
                    seriaLokalizacjiGeo.AppendChild(lokalizacja);

                    XmlElement wspolrzedneGeo = newDoc.CreateElement("dsn", "wspolrzedneGeo", rootNew.NamespaceURI);
                    lokalizacja.AppendChild(wspolrzedneGeo);

                    XmlElement pos = newDoc.CreateElement("gml", "pos", "http://www.opengis.net/gml");
                    pos.SetAttribute("srsName", "WGS84");
                    pos.InnerText = lon.InnerText + " " + lat.InnerText;
                    wspolrzedneGeo.AppendChild(pos);

                }

            }//if rootSa has child nodes


                return newDoc;
        }

        //private void ProcessPhoto(List<String> roadList, String outputPath)
        //{

          
        //    XmlDocument newDoc =null;
        //    XmlDeclaration xmlDeclaration = null;
        //    XmlElement rootNew = null;
        //    XmlElement pomiar = null;
        //    XmlElement headerNode = null;
        //    String wojewodztwo = null;

        //    if (roadList != null && roadList.Count != 0)
        //    {

        //        String streetTemp = "";

        //        foreach(string pair in roadList)
        //        {
        //            String[] splitedKey = pair.Split('?');
        //            String street = splitedKey[0];
        //            String messfahrt = splitedKey[1];
        //            String path = splitedKey[2];

        //            if (!streetTemp.Equals(street))
        //            {
        //                //save this xml and create new document
        //                newDoc = new XmlDocument();
        //                xmlDeclaration = newDoc.CreateXmlDeclaration("1.0", "UTF-8", null);

        //                XmlElement rootOld = fotoHeader.DocumentElement;

        //                rootNew = newDoc.CreateElement("dsn", "daneElementarnePPFsiec", "dsn");
        //                rootNew.SetAttribute("xmlns:dsn", "http://www.gddkia.gov.pl/dsn/1.0.0");
        //                rootNew.SetAttribute("xmlns:gml", "http://www.opengis.net/gml");
        //                rootNew.SetAttribute("xmlns:sch", "http://www.ascc.net/xml/schematron");
        //                rootNew.SetAttribute("xmlns:xlink", "http://www.w3.org/1999/xlink");
        //                rootNew.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
        //                rootNew.SetAttribute("xsi:schemaLocation", "http://www.gddkia.gov.pl/dsn/1.0.0./dane_elementarne.xsd");
        //                rootNew.SetAttribute("uwaga", rootOld.GetAttribute("Bemerkung") == null ? "" : rootOld.GetAttribute("Bemerkung"));
        //                rootNew.SetAttribute("rodzaj", "sieciowe");
        //                // rootNew.SetAttribute("cecha", rootOld.GetAttribute("Merkmal") == null ? "" : rootOld.GetAttribute("Merkmal"));
        //                rootNew.SetAttribute("dataUtworzenia", rootOld.GetAttribute("Erstelldatum") == null ? "" : rootOld.GetAttribute("Erstelldatum"));
        //                newDoc.AppendChild(xmlDeclaration);
        //                newDoc.AppendChild(rootNew);

        //                pomiar = newDoc.CreateElement("dsn", "przejazdPomiarowy", rootNew.NamespaceURI);
        //                XmlAttribute t = newDoc.CreateAttribute("gml", "id", "http://www.opengis.net/gml");
        //                String gmlID = GenerateGmlId(messfahrt);
        //                t.Value = gmlID;
        //                pomiar.SetAttributeNode(t);

        //                rootNew.AppendChild(pomiar);

                        

        //                XmlNodeList hList = fotoHeader.GetElementsByTagName("dsn:naglowekPrzejazduPomiarowego");
        //                if (hList != null && hList.Count != 0)
        //                {
        //                    headerNode = (XmlElement)hList[0];
        //                    headerNode.Attributes["gml:id"].Value = increaseGmlID(gmlID);

        //                    XmlNodeList liMetadane = headerNode.GetElementsByTagName("dsn:metadane");
        //                    if (liMetadane != null)
        //                    {
        //                        XmlElement metadane = (XmlElement)liMetadane[0];
        //                        wojewodztwo = metadane.GetAttribute("wojewodztwo");
        //                    }

        //                    // pomiar.AppendChild(headerNode);
        //                    XmlNode hNode = newDoc.ImportNode(headerNode, true);
        //                    pomiar.AppendChild(hNode);
        //                }

        //                if (!String.IsNullOrEmpty(outputFolder))
        //                {
        //                    createMessfahrtFolder(outputFolder, messfahrt);
        //                }
        //                else
        //                {
        //                    //TODO
        //                }
                        
        //                streetTemp = street;
        //            }

        //           // newDoc = new XmlDocument();
        //           // XmlDeclaration xmlDeclaration = newDoc.CreateXmlDeclaration("1.0", "UTF-8", null);

                    

        //            if (Directory.Exists(path))
        //            {
        //                String picSA = path + "pic_sa.xml";

        //                if (File.Exists(picSA))
        //                {
        //                    XmlDocument saDoc = new XmlDocument();
        //                    saDoc.Load(picSA);
                            

        //                    //XmlElement rootOld = fotoHeader.DocumentElement;

        //                    //XmlElement rootNew = newDoc.CreateElement("dsn", "daneElementarnePPFsiec", "dsn");
        //                    //rootNew.SetAttribute("xmlns:dsn", "http://www.gddkia.gov.pl/dsn/1.0.0");
        //                    //rootNew.SetAttribute("xmlns:gml", "http://www.opengis.net/gml");
        //                    //rootNew.SetAttribute("xmlns:sch", "http://www.ascc.net/xml/schematron");
        //                    //rootNew.SetAttribute("xmlns:xlink", "http://www.w3.org/1999/xlink");
        //                    //rootNew.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
        //                    //rootNew.SetAttribute("xsi:schemaLocation", "http://www.gddkia.gov.pl/dsn/1.0.0./dane_elementarne.xsd");
        //                    //rootNew.SetAttribute("uwaga", rootOld.GetAttribute("Bemerkung") == null ? "" : rootOld.GetAttribute("Bemerkung"));
        //                    //rootNew.SetAttribute("rodzaj", "sieciowe");
        //                    //// rootNew.SetAttribute("cecha", rootOld.GetAttribute("Merkmal") == null ? "" : rootOld.GetAttribute("Merkmal"));
        //                    //rootNew.SetAttribute("dataUtworzenia", rootOld.GetAttribute("Erstelldatum") == null ? "" : rootOld.GetAttribute("Erstelldatum"));
        //                    //newDoc.AppendChild(xmlDeclaration);
        //                    //newDoc.AppendChild(rootNew);

        //                    //XmlElement pomiar = newDoc.CreateElement("dsn", "przejazdPomiarowy", rootNew.NamespaceURI);
        //                    //XmlAttribute t = newDoc.CreateAttribute("gml", "id", "http://www.opengis.net/gml");
        //                    //String gmlID = GenerateGmlId(messfahrt);
        //                    //t.Value = gmlID;
        //                    //pomiar.SetAttributeNode(t);

        //                    //rootNew.AppendChild(pomiar);

        //                    //XmlElement headerNode = null;
        //                    //String wojewodztwo = null;

        //                    //XmlNodeList hList = fotoHeader.GetElementsByTagName("dsn:naglowekPrzejazduPomiarowego");
        //                    //if (hList != null && hList.Count != 0)
        //                    //{
        //                    //    headerNode = (XmlElement)hList[0];
        //                    //    headerNode.Attributes["gml:id"].Value = increaseGmlID(gmlID);

        //                    //    XmlNodeList liMetadane = headerNode.GetElementsByTagName("dsn:metadane");
        //                    //    if (liMetadane != null)
        //                    //    {
        //                    //        XmlElement metadane = (XmlElement)liMetadane[0];
        //                    //        wojewodztwo = metadane.GetAttribute("wojewodztwo");
        //                    //    }

        //                    //    // pomiar.AppendChild(headerNode);
        //                    //    XmlNode hNode = newDoc.ImportNode(headerNode, true);
        //                    //    pomiar.AppendChild(hNode);
        //                    //}

        //                    XmlElement wgs = newDoc.CreateElement("dsn", "seriaLokalizacjiGeo", rootNew.NamespaceURI);
        //                    pomiar.AppendChild(wgs);           


        //                    XmlElement rootSa = saDoc.DocumentElement;

        //                    if (rootSa.HasChildNodes)
        //                    {

        //                        XmlNodeList picIndexList = rootSa.GetElementsByTagName("PicIndex");
        //                        String vnk = null;
        //                        XmlNodeList vnkList = rootSa.GetElementsByTagName("VNK");
        //                        vnk = vnkList[0].InnerText;

        //                        String nnk = null;
        //                        XmlNodeList nnkList = rootSa.GetElementsByTagName("NNK");
        //                        nnk = nnkList[0].InnerText;


        //                        foreach(XmlElement element in picIndexList)
        //                        {
                                 

        //                            XmlElement lat = null;
        //                            XmlNodeList latList = element.GetElementsByTagName("LAT");
        //                            if (latList != null && latList.Count != 0)
        //                            {
        //                                lat = (XmlElement)latList[0];
        //                            }
                                    
        //                            XmlElement lon = null;
        //                            XmlNodeList lonList = element.GetElementsByTagName("LON");
        //                            if (lonList != null && lonList.Count != 0)
        //                            {
        //                                lon = (XmlElement)lonList[0];
        //                            }

        //                            XmlElement datum = null;
        //                            XmlNodeList datumList = element.GetElementsByTagName("Datum");
        //                            if (datumList != null && datumList.Count !=0)
        //                            {
        //                                datum = (XmlElement)datumList[0];
        //                            }

        //                            XmlElement station = null;
        //                            XmlNodeList stationList = element.GetElementsByTagName("Station");
        //                            if (stationList != null && stationList.Count != 0)
        //                            {
        //                                station = (XmlElement)stationList[0];
        //                            }

        //                            String test = datum.InnerText;
        //                            DateTime dt = DateTime.Parse(test);

        //                            XmlElement lokalizacja = newDoc.CreateElement("dsn", "lokalizacja", rootNew.NamespaceURI);
        //                            lokalizacja.SetAttribute("data", dt.ToShortDateString());
        //                            lokalizacja.SetAttribute("uwaga", "");
        //                            lokalizacja.SetAttribute("godzina", dt.ToLongTimeString());
        //                            lokalizacja.SetAttribute("mb", station ==null || station.InnerText ==null ?"": station.InnerText);
        //                            lokalizacja.SetAttribute("odleglosc", station == null || station.InnerText == null ? "" : station.InnerText);
        //                            wgs.AppendChild(lokalizacja);

        //                            XmlElement wspolrzedneGeo = newDoc.CreateElement("dsn", "wspolrzedneGeo", rootNew.NamespaceURI);
        //                            lokalizacja.AppendChild(wspolrzedneGeo);

        //                            XmlElement pos = newDoc.CreateElement("gml", "pos", "http://www.opengis.net/gml");
        //                            pos.SetAttribute("srsName", "WGS84");
        //                            pos.InnerText = lon.InnerText + " " + lat.InnerText;
        //                            wspolrzedneGeo.AppendChild(pos);

        //                        }

        //                           // string sql = "select * from dane where pref = '" + vnk + "' and nref = '" + nnk + "' and odleglosc = " + station + ";";
        //                            string sql = "select * from dane where pref = '" + vnk + "' and nref = '" + nnk + "' ;";
        //                                loadedData = LoadData(sqlCon ,sql);

        //                          if (loadedData != null && loadedData.Rows.Count !=0)
        //                          {
        //                              pasRuchu = loadedData.Rows[0]["pas_ruchu"].ToString();
        //                              numerJezdni = loadedData.Rows[0]["numer_jezdni"].ToString();
        //                              kierunek = loadedData.Rows[0]["kierunek"].ToString();
        //                              powiat = loadedData.Rows[0]["powiat"].ToString();
        //                              if (String.IsNullOrEmpty(wojewodztwo))
        //                              {
        //                                  wojewodztwo = loadedData.Rows[0]["wojewodztwo"].ToString();
        //                              }


        //                              XmlElement firstIndex =(XmlElement) picIndexList[0];
        //                              XmlNodeList firstStationList = firstIndex.GetElementsByTagName("Station");
        //                              XmlElement firstStation = (XmlElement)firstStationList[0];

        //                              XmlElement nrDrogi = (XmlElement)firstIndex.GetElementsByTagName("IDDROGI")[0];

        //                              XmlElement lastIndex = (XmlElement)picIndexList[picIndexList.Count -1];
        //                              XmlNodeList lastStationList = lastIndex.GetElementsByTagName("Station");
        //                              XmlElement lastStation = (XmlElement)lastStationList[0];

        //                              int dlugosc = Int32.Parse(lastStation.InnerText) - Int32.Parse(firstStation.InnerText);

        //                              XmlElement lokalizacjaSiec = newDoc.CreateElement("dsn", "lokalizacjaSiec", rootNew.NamespaceURI);
        //                           // lokalizacjaSiec.SetAttribute("dlugosc", asbElement.GetAttribute("VST"));
        //                            lokalizacjaSiec.SetAttribute("dlugosc", Convert.ToString(dlugosc));

        //                            lokalizacjaSiec.SetAttribute("kodPRef", vnk);
        //                            lokalizacjaSiec.SetAttribute("numerDrogi", nrDrogi.InnerText);
        //                            lokalizacjaSiec.SetAttribute("numerJezdni", numerJezdni);
        //                            lokalizacjaSiec.SetAttribute("kierunek", kierunek);
        //                            lokalizacjaSiec.SetAttribute("kodNRef", nnk);
        //                            lokalizacjaSiec.SetAttribute("pasRuchu", pasRuchu);

        //                              //nie wiem 
        //                            lokalizacjaSiec.SetAttribute("odleglosc", lastStation.InnerText);
        //                            pomiar.AppendChild(lokalizacjaSiec);


        //                            XmlElement informacjeSieciowe = newDoc.CreateElement("dsn", "informacjeSieciowe", rootNew.NamespaceURI);
        //                                        informacjeSieciowe.SetAttribute("wojewodztwo", loadedData.Rows[0]["wojewodztwo"].ToString());
        //                                        informacjeSieciowe.SetAttribute("powiat", powiat);
        //                                        informacjeSieciowe.SetAttribute("gmina", gmina);
        //                                        informacjeSieciowe.SetAttribute("oddzial", loadedData.Rows[0]["oddzial"].ToString());
        //                                        informacjeSieciowe.SetAttribute("rejon", loadedData.Rows[0]["rejon"].ToString());
        //                                        informacjeSieciowe.SetAttribute("rodzajObszaru", loadedData.Rows[0]["rodzaj_obszaru"].ToString());
        //                                        pomiar.AppendChild(informacjeSieciowe);


        //                        }

        //                          XmlElement zdjecia = newDoc.CreateElement("dsn", "zdjecia", rootNew.NamespaceURI);
        //                          pomiar.AppendChild(zdjecia);

        //                          List<XmlElement> kamPrzod = new List<XmlElement>();
        //                          List<XmlElement> kamTyl = new List<XmlElement>();
        //                          List<XmlElement> kamLewa = new List<XmlElement>();
        //                          List<XmlElement> kamPrawa = new List<XmlElement>();
                                  
        //                        foreach (XmlElement element in picIndexList)
        //                        {
        //                            XmlElement kameraOznaczenie = (XmlElement)element.GetElementsByTagName("Buchst")[0];
        //                            XmlElement picName = (XmlElement)element.GetElementsByTagName("Filename")[0];
        //                            XmlElement picPath = (XmlElement)element.GetElementsByTagName("PicPath")[0];


        //                            String picFileName = null;
        //                            String zdj = picName.InnerText;

        //                            //if (File.Exists(path + "\\" + zdj))
        //                            //{
        //                            //    //Console.WriteLine("EXIST");
        //                            //   // picFileName = RenamePictureFile(pair.Value, zdj);
        //                            //    picFileName = RenamePhoto(path, zdj, messfahrt);
        //                            //}
        //                            //else
        //                            //{
        //                            //    String[] splitedZDJ = zdj.Split('\\');
        //                            //    String folder = splitedZDJ[0];
        //                            //    String fNameOld = splitedZDJ[2];
        //                            //    String[] splitedNameOld = fNameOld.Split('_');
        //                            //    String prefixName = splitedNameOld[0].Replace("B", "P");
        //                            //    String newName = prefixName + "_" + splitedNameOld[1] + "_" + splitedNameOld[2];

        //                            //    if (File.Exists(picturesPath + "\\" + folder + "\\" + newName))
        //                            //    {
        //                            //        picFileName = folder + "\\" + newName;
        //                            //    }
        //                            //    else
        //                            //    {
        //                            //        picFileName = zdj;
        //                            //    }

        //                            //}


        //                            //XmlElement zd = newDoc.CreateElement("dsn", "zdjecie", "dsn");
        //                            ////zd.SetAttribute("plik", zdjecie.GetAttribute("D"));
        //                            //zd.SetAttribute("plik", picFileName);
        //                            //zd.SetAttribute("nrKamery", zdjecie.GetAttribute("Nr"));
        //                            //zd.SetAttribute("A", "X");
        //                            //zd.SetAttribute("odleglosc", zdjecie.GetAttribute("Station"));
        //                            ////zdjecia.AppendChild(zd);

                                   

        //                            if (kameraOznaczenie.InnerText.Equals("G"))
        //                            {
        //                                //kamTyl.Add();
        //                            }
        //                            else if (kameraOznaczenie.InnerText.Equals("S"))
        //                            {

        //                            }
        //                            else if (kameraOznaczenie.InnerText.Equals("V"))
        //                            {

        //                            }
        //                            else if (kameraOznaczenie.InnerText.Equals("X"))
        //                            {

        //                            }
        //                        }

        //                    }

        //                }

        //            }


        //            newDoc.Save("C:\\test\\fotoTest.xml");

        //            //TODO save file and rename and save pictures

        //        }
        //    }

        //}

        private String MovePhoto(String camera, String messfahrt, String photoPath, String oldFileName, String newFileName)
        {
            String pathWithName = null;

            if (!Directory.Exists(outputFolder + "\\" + camera))
            {
                Directory.CreateDirectory(outputFolder + "\\" + camera);
            }

            if (!Directory.Exists(outputFolder + "\\" + camera + "\\" + messfahrt))
            {
                createMessfahrtFolder(outputFolder + "\\" + camera, messfahrt);
            }

            String dirToOldFile = oldFileName.Substring(0, oldFileName.LastIndexOf("\\"));
            String oldFileN = oldFileName.Replace(dirToOldFile + "\\", "");
           // DirectoryInfo d = new DirectoryInfo(dirToOldFile + "\\" + folder + "\\");
            DirectoryInfo d = new DirectoryInfo(dirToOldFile + "\\");

            FileInfo[] infos = d.GetFiles(oldFileN);
            if (infos != null && infos.Length != 0)
            {
                FileInfo file = infos[0];
               // File.C
                File.Copy(file.FullName, outputFolder + "\\" + camera + "\\" + messfahrt + "\\" + newFileName, true);
                //File.Move(file.FullName, outputFolder + "\\" + camera + "\\" + messfahrt+"\\"+newFileName);
                pathWithName = "\\" + camera + "\\" + messfahrt + "\\" + newFileName;
            }


            return pathWithName;

        }

        private String RenamePhoto(String fileName, String messfahrt, String camera )
        {
            String newName = null;
            String camNr = null;
            String prefix = "P";
            

            try
            {
               
                String tempName = null;

                switch(camera){
                    case "Front":
                        {
                            camNr = "01_";
                            tempName = fileName.Replace("SXA", "");
                            break;
                        }
                    case "Lewa":
                        {
                            camNr = "02_";
                            tempName = fileName.Replace("SSA", "");
                            break;
                        }
                    case "Prawa":
                        {
                            camNr = "03_";
                            tempName = fileName.Replace("SVA", "");
                            break;
                        }
                    case "Tylna":
                        {
                            camNr = "04_";
                            tempName = fileName.Replace("SGA", "");
                            break;
                        }

                }
             

                int picNr = Int32.Parse(tempName.Replace(".jpg", ""));

                switch(picNr.ToString().Length){
                    case 1:
                        {
                            tempName = "0000000" + picNr + ".jpg";
                            break;
                        }
                    case 2:
                        {
                            tempName = "000000" + picNr + ".jpg";
                            break;
                        }
                    case 3:
                        {
                            tempName = "00000" + picNr + ".jpg";
                            break;
                        }
                    case 4:
                        {
                            tempName = "0000" + picNr + ".jpg";
                            break;
                        }
                    case 5:
                        {
                            tempName = "000" + picNr + ".jpg";
                            break;
                        }
                    case 6:
                        {
                            tempName = "00" + picNr + ".jpg";
                            break;
                        }
                    case 7:
                        {
                            tempName = "0" + picNr + ".jpg";
                            break;
                        }
                }           

                newName = prefix + camNr + messfahrt+"_"+tempName;


            }catch(Exception e){
                Console.WriteLine(e);
            }
           

            return newName;
        }

        private void toolStripMenuItemOpenPhotoFolder_Click(object sender, EventArgs e)
        {
           // FotoRejestrationProcess proc = new FotoRejestrationProcess();
            folderBrowserDialogPhoto = new FolderBrowserDialog();
            DialogResult result = folderBrowserDialogPhoto.ShowDialog();

             if (result == DialogResult.OK)
             {
                 directoryPhoto = folderBrowserDialogPhoto.SelectedPath;
                 String[] files = Directory.GetFiles(directoryPhoto, "*.xml");
                 //roadList = new Dictionary<string, string>();
                 roadList = new List<String>();
                 roadDictionary = new Dictionary<string, string>();

                 foreach(string file in files){
                     if (file.ToLower().Contains("index.xml"))
                     {
                         XmlDocument doc = new XmlDocument();
                         doc.Load(file);

                         XmlNodeList indexList = doc.GetElementsByTagName("INDEX");
                         foreach(XmlElement index in indexList)
                         {
                             XmlElement picPath = null;
                             XmlElement vol = null;
                             XmlElement str = null;
                             XmlElement messfahrt = null;

                             XmlNodeList picPathL = index.GetElementsByTagName("PICPATH");
                             if (picPathL != null && picPathL.Count != 0)
                             {
                                 picPath = (XmlElement)picPathL[0];
                             }
                             
                             XmlNodeList volL = index.GetElementsByTagName("VOLUME");
                             if (volL != null && volL.Count != 0)
                             {
                                 vol = (XmlElement)volL[0];
                             }

                             XmlNodeList strL = index.GetElementsByTagName("StrBez");
                             if (strL != null && strL.Count != 0)
                             {
                                 str = (XmlElement)strL[0];
                             }

                             XmlNodeList messfList = index.GetElementsByTagName("BEMERKUNG");
                             if (messfList != null && messfList.Count != 0)
                             {
                                 messfahrt = (XmlElement)messfList[0];
                             }

                             String pathToPictures = directoryPhoto + "\\" + picPath.InnerText;
                             String street = str.InnerText;


                             if (!roadList.Contains(street + "?" + messfahrt.InnerText + "?" + pathToPictures))
                             {
                                 roadList.Add(street + "?" + messfahrt.InnerText + "?" + pathToPictures);
                             }

                             if (!roadDictionary.ContainsKey(pathToPictures + "?" + messfahrt.InnerText))
                             {
                                 roadDictionary.Add(pathToPictures + "?" + messfahrt.InnerText, street);
                             }
                            
                             
                         }

                         //Console.WriteLine(roadList.Count);
                         //ProcessPhoto2(roadDictionary, outputFolder);
                        // ProcessPhoto(roadList, outputFolder);
                     }
                 }

             }

             testStartPhotoButton();

        }

        private void toolStripMenuItemXmlFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1 = new FolderBrowserDialog();
            DialogResult result = folderBrowserDialog1.ShowDialog();
            List<String> xmlToValidate = new List<string>();
            toolStripProgressBar1.Value = 0;

            if(result == DialogResult.OK){

                String[] filesXml = Directory.GetFiles(folderBrowserDialog1.SelectedPath , "*.xml");
                toolStripProgressBar1.Maximum = filesXml.Length;
                int count = 0;

                foreach(string file in filesXml)
                {
                    string[] splitedName = file.Split('\\');
                    String fileName = splitedName[splitedName.Length - 1];
                    XmlDocument doc = new XmlDocument();
                    doc.Load(file);

                    bool val = ValidateDsnXML(doc);
                    if (val)
                    {
                        DisplayData("Document " + fileName + " is valid.");
                    }
                    else
                    {
                        DisplayData("Document " + fileName + " is not valid.");
                    }

                    count++;
                    toolStripProgressBar1.Value = count;

                }

            }
        }

        private void toolStripButtonProcessPhoto_Click(object sender, EventArgs e)
        {

            Thread backgroundThread = new Thread(
                 new ThreadStart(() =>
                 {

                     if (String.IsNullOrEmpty(outputFolder))
                     {
                         MessageBox.Show("Output path is empty.");
                         return;
                     }
                     else
                     {
                         EnableButtonStartPhoto(false);
                         ProcessPhoto2(roadDictionary, outputFolder);
                         roadList = null;
                         testStartPhotoButton();
                     }
                    
                 }));

            backgroundThread.Start();

        }

     

     
    }
}
