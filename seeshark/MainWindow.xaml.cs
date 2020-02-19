//############################################################################################################################
//########?Was?######SQL Foto Datenbank#######################################################################################
//########?Name?#####See Shark################################################################################################
//########?Author?###David D'Orazio###########################################################################################
//############################################################################################################################
//########In diesem Programm ist es möglich Fotos in einer SQL DatenBank zu speichern#########################################
//########Fotos werden in Binärstring-Arrays gespeichert######################################################################
//########Fotos werden aus der SQL Datenbank Binär ausgelesen und wieder konvertiert##########################################
//############################################################################################################################

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace seeshark
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            fillLists();
        }

        public OpenFileDialog ofd = new OpenFileDialog();
        public List<Image> miniBox = new List<Image>();
        public List<Image> delBoxes = new List<Image>();
        public List<Image> editBoxes = new List<Image>();
        public List<string> fileName = new List<string>();
        public List<int> lstChanged = new List<int>();
        public string connectionString = null;
        public bool wasLoaded { get; set; }                                 //bool das ein datensatz geladen wurde, um zwei funktionen auf einen knopf egen zu können


        //##########################################################################
        //########################FUNKTIONEN########################################
        //##########################################################################
        public void fillLists()                 //alle listen befüllen
        {
            delBoxes.Add(picDel0);
            delBoxes.Add(picDel1);
            delBoxes.Add(picDel2);
            delBoxes.Add(picDel3);
            delBoxes.Add(picDel4);
            delBoxes.Add(picDel5);
            editBoxes.Add(picEdit0);
            editBoxes.Add(picEdit1);
            editBoxes.Add(picEdit2);
            editBoxes.Add(picEdit3);
            editBoxes.Add(picEdit4);
            editBoxes.Add(picEdit5);
            miniBox.Add(picMini0);
            miniBox.Add(picMini1);
            miniBox.Add(picMini2);
            miniBox.Add(picMini3);
            miniBox.Add(picMini4);
            miniBox.Add(picMini5);
        }
        private void openFile()                 //filedialog aufmachen und die vorschau boxen befüllen
        {
            ofd.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            if (ofd.ShowDialog() == true)                                                  //wenn ofd erfolgreich ausgeführt wurde und ausgewählt
            {
                for (int i = 0; i <= 5; i++)                                                 //loop um alle 6 bilderboxen befüllen zu können
                {
                    if (miniBox[i].Source == null)                                          //nach leerer Minibox suchen
                    {
                        miniBox[i].Source = new BitmapImage(new Uri(ofd.FileName));         //liste befülllen
                        delBoxes[i].Visibility = Visibility.Visible;                        //löschen und neu laden symbol einbleden
                        editBoxes[i].Visibility = Visibility.Visible;
                        fileName.Add(ofd.FileName);                                         //filename in Liste 
                        break;                                                              //break um verschiedene Bilder zu laden     AUSKOMMENTIEREN um schneller Testen zu können (HURRAAAA!!! Zeitsparen)
                    }
                }
            }
        }
        private void loadAll()                  //Funktion um alle Daten aus SQL in die Liste zu laden
        {
            SqlConnection myConnection = new SqlConnection(connectionString);
            SqlCommand myCommand = new SqlCommand();
            myConnection.Open();
            SqlDataReader myReader = null;
            try
            {
                myCommand.Connection = myConnection;
                myCommand.CommandType = CommandType.StoredProcedure;
                myCommand.CommandText = "dbo.getAllData";
                myReader = myCommand.ExecuteReader();
                while (myReader.Read())                                                         //solange gelesen wird
                {
                    ListViewItem item = new ListViewItem();                                     //werden werte in die Liste
                    item.Content = myReader[0].ToString();
                    this.lstAllData.Items.Add(item);                                            //hinzugefügt
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                myConnection.Close();                                                           //verbindung und reader werden geschlossen
                myReader.Close();
            }
        }
        private void deleteAll()                //ALLES Löschen und Bilder verstecken
        {
            lblID.Content = "00";
            txtName.Clear();
            picZoom.Source = null;
            wasLoaded = false;
            foreach (var image in miniBox)
                image.Source = null;


            foreach (var delbox in delBoxes)
                delbox.Visibility = Visibility.Collapsed;

            foreach (var editbox in editBoxes)
                editbox.Visibility = Visibility.Collapsed;

            fileName.Clear();
        }
        private void uploadFile()               //Datensatz auf Server Laden
        {
            SqlConnection myConnection = new SqlConnection(connectionString);
            SqlCommand myCommand = new SqlCommand();
            myConnection.Open();
            bool error = false;                                                                 //bool um nur uplaoden wenn kein fehler
            List<SqlParameter> lstPrmtr = new List<SqlParameter>();                             //Liste für Parameter
            try
            {
                myCommand.Connection = myConnection;
                myCommand.CommandType = CommandType.StoredProcedure;
                myCommand.CommandText = "dbo.addData";
                for (int i = 0; i <= 5; i++)
                {
                    if (miniBox[i].Source != null)
                    {                                                                                            //Bild konvertieren
                        byte[] img = null;                                                                       //Bild konvertieren
                        FileStream fs = new FileStream(fileName[i], FileMode.Open, FileAccess.Read);             //Bild konvertieren
                        BinaryReader br = new BinaryReader(fs);                                                  //Bild konvertieren
                        img = br.ReadBytes((int)fs.Length);                                                      //Bild konvertieren
                        SqlParameter toAdd = new SqlParameter("@picture" + (i), SqlDbType.Image);                //Parameter für Bild an Position 1 geladen dann 2 dann 3..usw.
                        toAdd.Value = img;                                                                       //Parameter bekommt byte wert des Bildes
                        lstPrmtr.Add(toAdd);                                                                     //Paramterer wird in Liste gespeichert
                    }
                }


                if (txtName.Text.Length > 0)                                                                     //Titel muss angegeben werden
                {
                    SqlParameter toAdd = new SqlParameter("@Text1", SqlDbType.NVarChar);                            //Parameter für Titel
                    toAdd.Value = txtName.Text;
                    lstPrmtr.Add(toAdd);                                                                            //wird zu liste hinzugefügt
                }


                myCommand.Parameters.AddRange(lstPrmtr.ToArray());                                                  //parameterliste wird SQL command hinzugefügt                myCommand.ExecuteNonQuery();                                                                        //Execute Proc
                myCommand.ExecuteNonQuery();                                                                        //SQL command execute
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                error = true;                                                                                       //falls error wird bool gesetzt
            }
            finally
            {
                myConnection.Close();                                                                               //connection wird geschlossen
                if (error == false)                                                                                 //Wenn man es hier vorbei schafft wird ausgegeben das geuploaded wurde
                {
                    MessageBox.Show("Daten wurden hochgeladen");                                                    //HURRRAAAAAA!!! hier freuen wir uns
                }
                deleteAll();                                                                                        //alles löschen und aufräumen das UI wieder leer ist
                lstPrmtr.Clear();                                                                                   //Parameterliste löschen
            }

        }
        private void editFile()                                         //funktion um Daten die wasLoaded=true sind zu bearbeiten
        {
            SqlConnection myConnection = new SqlConnection(connectionString);
            SqlCommand myCommand = new SqlCommand();
            myConnection.Open();
            bool error = false;
            List<SqlParameter> lstPrmtr = new List<SqlParameter>();  //Liste für Parameter
            int pos = 0;
            try
            {
                myCommand.Connection = myConnection;
                myCommand.CommandType = CommandType.StoredProcedure;
                myCommand.CommandText = "dbo.editData";
                SqlParameter IDAdd = new SqlParameter("@ID", SqlDbType.Int);
                IDAdd.Value = lblID.Content;
                lstPrmtr.Add(IDAdd);
                foreach (int changed in lstChanged)
                {
                    byte[] img = null;
                    FileStream fs = new FileStream(fileName[pos], FileMode.Open, FileAccess.Read);
                    BinaryReader br = new BinaryReader(fs);
                    img = br.ReadBytes((int)fs.Length);
                    SqlParameter toAdd = new SqlParameter("@picture" + (changed), SqlDbType.Image);
                    toAdd.Value = img;
                    lstPrmtr.Add(toAdd);
                    pos++;
                }

                if (txtName.Text != "")
                {
                    SqlParameter toAdd = new SqlParameter("@Text1", SqlDbType.NVarChar);
                    toAdd.Value = txtName.Text;
                    lstPrmtr.Add(toAdd);
                }

                myCommand.Parameters.AddRange(lstPrmtr.ToArray());              //parameter hinzufügen
                myCommand.ExecuteNonQuery();                                    //Execute Proc
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                error = true;
            }
            finally
            {
                myConnection.Close();
                if (error == false)
                {
                    MessageBox.Show("Eintrag wurde bearbeitet");
                }
                lstAllData.Items.Clear();
                lstPrmtr.Clear();
                loadAll();
                deleteAll();
                lstChanged.Clear();
                wasLoaded = false;
            }
        }
        private BitmapImage ConvertBinaryToImage(byte[] data)           //funktion um binary aus SQL zu bild datein zu konvertieren
        {
            using (var ms = new MemoryStream(data))                //memorystream liest byte und returned Image
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                return bitmap;
            }
        }
        private void editPicture(int number)                            //Einzelne Bilder die bearbeitet werden, zuerst gelöscht und buttons verstecken und wert in der Liste speichern
        {
            miniBox[number].Source = null;
            delBoxes[number].Visibility = Visibility.Collapsed;
            editBoxes[number].Visibility = Visibility.Collapsed;
            lstChanged.Add(number);                                     //nummer der Box wird gespeichert das die richtige editiert werden kann
        }
        private void deletePic(int number)                              //löschen eines einzelnen Bildes
        {
            SqlConnection myConnection = new SqlConnection(connectionString);
            SqlCommand myCommand = new SqlCommand();
            myConnection.Open();
            if (lstChanged.Contains(number))                                                        //Liste wird abgearbeitet und die Zahl des Bildes gelöscht
            {
                lstChanged.Remove(number);
            }
            try
            {
                myCommand.Connection = myConnection;
                myCommand.CommandType = CommandType.StoredProcedure;
                myCommand.CommandText = "dbo.deletePic";
                SqlParameter addID = new SqlParameter("@ID", SqlDbType.Int);                        //Bild mit ID identifizieren
                addID.Value = lblID.Content;
                SqlParameter deletePic = new SqlParameter("@Number", SqlDbType.Int);                //Bild über Nummer ansprechen
                deletePic.Value = number;                                                           //value ist die Nummer
                myCommand.Parameters.Add(addID);
                myCommand.Parameters.Add(deletePic);
                myCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                myConnection.Close();
            }
        }
        private void shiftPics(int number)                              //Funktion zum nachschieben wenn Bilder gelöscht werden
        {
            lstChanged.Add(number);
            for (int i = 5; i >= number; i--)
            {
                if (miniBox[i].Source != null)
                {
                    miniBox[i - 1].Source = miniBox[i].Source;
                    miniBox[i].Source = null;
                    break;
                }
            }
            for (int i = 5; i >= number; i--)
            {
                if (miniBox[i].Source == null)
                {
                    delBoxes[i].Visibility = Visibility.Collapsed;
                    editBoxes[i].Visibility = Visibility.Collapsed;
                }
            }
        }
        private void clickList(string selected)                         //funktion um in listview clicken zu können und daten in vorschau zu bekommen
        {
            if (lstAllData.SelectedItems.Count > 0)                     //nur wenn was ausgewählt wurde
            {
                deleteAll();                                            //alles leeren
                SqlConnection myConnection = new SqlConnection(connectionString);       //SQL connection mit Reader
                SqlCommand myCommand = new SqlCommand();
                myConnection.Open();
                SqlDataReader myReader = null;
                try
                {


                    myCommand.Connection = myConnection;
                    myCommand.CommandType = CommandType.StoredProcedure;
                    myCommand.CommandText = "dbo.getData";                                          //Proc name
                    SqlParameter search = new SqlParameter("@text", SqlDbType.NVarChar);
                    search.Value = selected;                                                        //value wird übergeben
                    myCommand.Parameters.Add(search);
                    myReader = myCommand.ExecuteReader();
                    myReader.Read();
                    lblID.Content = myReader[0].ToString();                                         //ID wird ins Label geschrieben
                    for (int i = 1; i < 7; i++)                                                     //Bilder werden gesetzt
                    {
                        if (myReader[i] != System.DBNull.Value)
                        {
                            byte[] myBytes = (byte[])myReader[i];
                            Image bitToAdd = new Image();
                            bitToAdd.Source = (ConvertBinaryToImage((byte[])myReader[i]));
                            miniBox[i - 1].Source = bitToAdd.Source;
                            delBoxes[i - 1].Visibility = Visibility.Visible;
                            editBoxes[i - 1].Visibility = Visibility.Visible;
                            miniBox[i - 1].Visibility = Visibility.Visible;
                        }
                    }
                    txtName.Text = myReader[7].ToString();                                          //Text wird gesetzt
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    myConnection.Close();
                    myReader.Close();
                    wasLoaded = true;                                                               //was loaded true setzen
                }
            }
        }
        private void deleteFile()
        {
            SqlConnection myConnection = new SqlConnection(connectionString);
            SqlCommand myCommand = new SqlCommand();
            myConnection.Open();
            bool error = false;
            if (lstAllData.SelectedItems.Count > 0)
            {
                try
                {
                    myCommand.Connection = myConnection;
                    myCommand.CommandType = CommandType.StoredProcedure;
                    myCommand.CommandText = "dbo.deleteData";
                    SqlParameter search = new SqlParameter("@ID", SqlDbType.Int);              //paremeter ID wird 
                    search.Value = lblID.Content;                                              //vom lbl mit der ID genommen
                    myCommand.Parameters.Add(search);
                    myCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    error = true;
                }
                finally
                {
                    myConnection.Close();
                    lstAllData.Items.Clear();
                    loadAll();
                    deleteAll();
                    if (error == false)
                    {
                        MessageBox.Show("Eintrag wurde gelöscht");
                    }
                    wasLoaded = false;
                }
            }

        }
        private void openEdited()                                       //funktion das lstCHanged befüllt wird wenn man files öffnet
        {
            int count = 0;
            foreach (var img in miniBox)
            {
                if (img.Source != null)                                 //Zählen wenn bild vorhanden ist
                    count++;
            }
            lstChanged.Add(count);                                      //liste befüllen mit zähler das richtiges bild geladen wird

        }
        private void searchFile()                                               //funktion zum suchen über namen
        {
            deleteAll();
            SqlConnection myConnection = new SqlConnection(connectionString);                       //SQL connection mit Reader 
            SqlCommand myCommand = new SqlCommand();
            myConnection.Open();
            SqlDataReader myReader = null;
            try
            {
                myCommand.Connection = myConnection;
                myCommand.CommandType = CommandType.StoredProcedure;
                myCommand.CommandText = "dbo.getData";
                SqlParameter search = new SqlParameter("@text", SqlDbType.NVarChar);                //parameter text wird übernommen
                search.Value = txtSearch.Text;
                myCommand.Parameters.Add(search);
                myReader = myCommand.ExecuteReader();
                myReader.Read();
                lblID.Content = myReader[0].ToString();                                             //ID nummer pos[0] wird gesetzt
                for (int i = 1; i <= 6; i++)                                                         //pos[1] bis pos[7] wird mit Bilder gesetzt
                {
                    if (myReader[i] != System.DBNull.Value)                                         //wenn kein Wert geladen wird wird nicht befüllt
                    {
                        byte[] myBytes = (byte[])myReader[i];
                        Image bitToAdd = new Image();
                        bitToAdd.Source = (ConvertBinaryToImage((byte[])myReader[i]));              //bitmap wird erstellt und mit funktion konvertiert
                        miniBox[i - 1].Source = bitToAdd.Source;                                        //kleine vorschau boxen werden befüllt
                        delBoxes[i - 1].Visibility = Visibility.Visible;                                 //löschenbild und editieren bild wird eingeblendet
                        editBoxes[i - 1].Visibility = Visibility.Visible;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                myConnection.Close();                                                               //connection schließen
                myReader.Close();
                wasLoaded = true;                                                                   //wasloaded true setzen damit beim speichern editiert wird
            }
        }
        private void zoomPic(int number)
        {
            picZoom.Source = miniBox[number].Source;
        }

        //##########################################################################
        //#############################CLICKEVENTS##################################
        //##########################################################################
        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            if (wasLoaded == true)
            {
                openEdited();
            }
            openFile();
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            deleteAll();
        }
        private void picEdit5_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            editPicture(5);
            openFile();
        }

        private void picEdit4_mouseDown(object sender, MouseButtonEventArgs e)
        {
            editPicture(4);
            openFile();
        }

        private void picEdit3_mouseDown(object sender, MouseButtonEventArgs e)
        {
            editPicture(3);
            openFile();
        }

        private void picEdit2_mouseDown(object sender, MouseButtonEventArgs e)
        {
            editPicture(2);
            openFile();
        }

        private void picEdit1_mouseDown(object sender, MouseButtonEventArgs e)
        {
            editPicture(1);
            openFile();
        }

        private void picEdit0_mouseDown(object sender, MouseButtonEventArgs e)
        {
            editPicture(0);
            openFile();
        }

        private void picdel0_mouseDown(object sender, MouseButtonEventArgs e)
        {

            deletePic(0);
            shiftPics(0);
        }

        private void picdel1_mouseDown(object sender, MouseButtonEventArgs e)
        {

            deletePic(1);
            shiftPics(1);
        }

        private void picdel2_mouseDown(object sender, MouseButtonEventArgs e)
        {
            deletePic(2);
            shiftPics(2);
        }

        private void picdel3_mouseDown(object sender, MouseButtonEventArgs e)
        {
            deletePic(3);
            shiftPics(3);

        }

        private void picdel4_mouseDown(object sender, MouseButtonEventArgs e)
        {
            deletePic(4);
            shiftPics(4);
        }

        private void picdel5_mouseDown(object sender, MouseButtonEventArgs e)
        {
            deletePic(5);
            shiftPics(5);
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            lstAllData.Items.Clear();
            deleteAll();
            string datasource = txtConnect.Text;
            connectionString = $"Connection Timeout=5; Data Source= {datasource}; Integrated Security= true; Database= chashtag; ApplicationIntent= ReadWrite";
            loadAll();
        }

        private void btnSaveEdit_Click(object sender, RoutedEventArgs e)
        {
            if (wasLoaded == true)              //wenn Datensatz geladen wurde, wird EditFile funktion aufgerufen
            {
                editFile();
            }
            else                            //sonst Normal Upload und neuer Tupel angelegt
            {
                uploadFile();

            }
            lstAllData.Items.Clear();       //liste wird gelöscht und neu geladen mit neuem / editiertem beitrag
            loadAll();
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            deleteFile();
        }

        private void lstAllData_clicked(object sender, SelectionChangedEventArgs e)
        {
            if (lstAllData.SelectedIndex >= 0)
            {
                clickList(((ListViewItem)((ListView)sender).SelectedItem).Content.ToString());
            }

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            searchFile();
        }

        private void picMini0_MouseDown(object sender, MouseButtonEventArgs e)
        {
            zoomPic(0);
        }

        private void picMini1_mouseDown(object sender, MouseButtonEventArgs e)
        {
            zoomPic(1);
        }

        private void picMini2_mouseDown(object sender, MouseButtonEventArgs e)
        {
            zoomPic(2);
        }

        private void picMini3_mouseDown(object sender, MouseButtonEventArgs e)
        {
            zoomPic(3);
        }

        private void picMini4_mouseDown(object sender, MouseButtonEventArgs e)
        {
            zoomPic(4);
        }

        private void picMini5_mouseDown(object sender, MouseButtonEventArgs e)
        {
            zoomPic(5);
        }
    }
}