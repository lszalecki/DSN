using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace DiagnostykaStanuNawierzchni
{
    public partial class XMLMerge : Form
    {

        private List<String> filesList;
        private string filesType;
        private string pathToFiles;
        private XmlDocument tp1aHeader;
        private XmlDocument tp1bHeader;
        private XmlDocument tp3Header;

        public XMLMerge()
        {
            InitializeComponent();
        }

        public XMLMerge(List<String> filesList, string filesType)
        {
            this.filesList = filesList;
            this.filesType = filesType;

            InitializeComponent();

          

            if (filesList != null && filesList.Count != 0)
            {
                listView1.Columns.Add("Files to merge:");
                listView1.View = View.Details;
                listView1.ShowGroups = true;

                pathToFiles = filesList[0].Substring(0, filesList[0].LastIndexOf("\\")) +"\\";

                foreach(string file in filesList){
                    string onlyFileName = file.Substring(file.LastIndexOf("\\")+1);
                    //g1.Items.Add(onlyFileName);
                    listView1.Items.Add(onlyFileName);
                }

                 string nrDrogi = listView1.Items[0].Text.Substring(8, 4);
                ListViewGroup group;
              

                int count =0;
                foreach(ListViewItem item in listView1.Items){
                    if (count == 0)
                    {
                        group = new ListViewGroup();                                       
                        group.Header = nrDrogi;
                        group.Name = nrDrogi;
                        listView1.Groups.Add(group);
                    }

                    if(item.Text.Contains(nrDrogi)){
                        item.Group = listView1.Groups[nrDrogi];
                    }
                    else
                    {
                        nrDrogi = item.Text.Substring(8, 4);
                        group = new ListViewGroup();
                        group.Header = nrDrogi;
                        group.Name = nrDrogi;
                        listView1.Groups.Add(group);
                        item.Group = listView1.Groups[nrDrogi];
                    }

                    count++;
                }
               

               
            }
            
           

            if (listView1 != null || listView1.Items.Count != 0)
            {
                if (ColumnHeaderAutoResizeStyle.ColumnContent.ToString().Length > ColumnHeaderAutoResizeStyle.HeaderSize.ToString().Length)
                {
                    listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                }
                else
                {
                    listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                }

            }
            else
            {
                listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            }


        }

        public void UpdateProgressBar(int value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<int>(UpdateProgressBar), new object[] { value });
                return;
            }
            toolStripProgressBar1.Value = value;
        }

        private void buttonMergeSelected_Click(object sender, EventArgs e)
        {
            UpdateProgressBar(0);
            string output = pathToFiles + "merged_files\\";
            if (!Directory.Exists(output))
            {
                Directory.CreateDirectory(output);
            }

            
            XmlDocument mergedDoc = new XmlDocument();
            XmlDeclaration xmlDeclaration = mergedDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
           

           ListView.SelectedListViewItemCollection collectionItems = listView1.SelectedItems;
           toolStripProgressBar1.Maximum = collectionItems.Count;

           string nameForMergedDoc = collectionItems[0].Text;

           int count = 0;
            foreach(ListViewItem item in collectionItems){

                string path = pathToFiles + item.Text;

                if (path != null && File.Exists(path))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(path);        

                    if (count == 0)
                    {
                        XmlNode t = mergedDoc.ImportNode(doc.DocumentElement, true);
                        mergedDoc.AppendChild(t);

                    }
                    else
                    {                                     

                        XmlNodeList list = doc.GetElementsByTagName("dsn:przejazdPomiarowy");
                        if (list != null && list.Count != 0)
                        {
                            XmlElement rideEl = (XmlElement)list[0];
                            string rideGmlId = rideEl.GetAttribute("gml:id") == null ? null : rideEl.GetAttribute("gml:id"); 
                            XmlNodeList ridesFromMerged = mergedDoc.GetElementsByTagName("dsn:przejazdPomiarowy"); 

                            foreach(XmlElement node in ridesFromMerged)
                            {
                                string gmlID = node.GetAttribute("gml:id") == null ? null : node.GetAttribute("gml:id"); 

                                if(rideGmlId.Equals(gmlID)){
                                    rideEl.RemoveAttribute("id", "http://www.opengis.net/gml");
                                }
                            }

                           XmlNode rNode = mergedDoc.ImportNode(rideEl, true);
                           mergedDoc.DocumentElement.AppendChild(rNode);

                        }
                    }
                
                    //mergedDoc.Save(output+nameForMergedDoc);
                }  

                count++;
                UpdateProgressBar(count);
            }

            mergedDoc.Save(output + nameForMergedDoc);
            RemoveFromListView(collectionItems);

        }


        private void RemoveFromListView(ListView.SelectedListViewItemCollection collectionToRemove)
        {
            listView1.Invoke(new EventHandler(delegate {
            
                foreach(ListViewItem item in collectionToRemove){
                    listView1.Items.Remove(item);
                }
            
            }));

        }


    }         
}
