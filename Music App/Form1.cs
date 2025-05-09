using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WMPLib;
using System.Data.SQLite;
using System.Collections;
using System.IO;
using System.Net.Configuration;

namespace Music_App
{
    public partial class Form1 : Form
    {
        int currentFolder, numberOfFavourites = 10;
        bool play, mute, darkmode = true, settings = true;
        string connectionString = "Data source = songs.db; Version = 3", filepath;
        SQLiteConnection connection;
        List<Song> songList = new List<Song>();
        List<Song> favourites = new List<Song>();
        List<Artist> artistList = new List<Artist>();
        List<Button> buttonList = new List<Button>();
        List<Label> labelList = new List<Label>();
        List<PictureBox> pictureBoxList = new List<PictureBox>();
        List<Image> imageList = new List<Image>();
        public Form1()
        {
            InitializeComponent();
            axWindowsMediaPlayer1.settings.volume = 10 * trackBar1.Value;
            imageList.AddRange(new Image[] {(Image)Properties.Resources.background_music1, (Image)Properties.Resources.background_music2,
                                            (Image)Properties.Resources.background_music3, (Image)Properties.Resources.headphones});
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            connection = new SQLiteConnection(connectionString);
            connection.Open();
            string createTableSQL = "Create table if not exists Song(" +
                                    "Title Text primary key," +
                                    "Name Text," +
                                    "Artist Text," +
                                    "Frequency real," +
                                    "Photo Text," +
                                    "Year integer, " +
                                    "Genre Text)";
            SQLiteCommand command = new SQLiteCommand(createTableSQL, connection);
            command.ExecuteNonQuery();
            connection.Close();
            createButtons();
            AddSongsToPlaylist();
        }

        private void createButtons()
        {
            songList.Clear();
            artistList.Clear();
            flowLayoutPanel1.Visible = false;
            foreach (Button button in buttonList)
            {
                button.Dispose();
            }
            buttonList.Clear();
            connection.Open();

            string selectTitlesSQL = "Select * from Song";
            SQLiteCommand command2 = new SQLiteCommand(selectTitlesSQL, connection);
            SQLiteDataReader sQLiteDataReader = command2.ExecuteReader();

            string title, name, artist, photo, genre;
            float frequency;
            int year;
            Image image;
            while (sQLiteDataReader.Read())
            {
                title = sQLiteDataReader.GetString(0);
                name = sQLiteDataReader.GetString(1);
                artist = sQLiteDataReader.GetString(2);
                frequency = sQLiteDataReader.GetFloat(3);
                photo = sQLiteDataReader.GetString(4);
                if (photo == "" || Properties.Resources.ResourceManager.GetObject(photo) == null)
                {
                    image = (Image)Properties.Resources.music;
                    image.Tag = "music";
                }
                else
                {
                    image = (Image)Properties.Resources.ResourceManager.GetObject(photo);
                    image.Tag = photo;
                }
                year = sQLiteDataReader.GetInt32(5);
                genre = sQLiteDataReader.GetString(6);

                Song song = new Song();
                song.title = title;
                song.name = name;
                song.photo = image;
                song.year = year;
                song.genre = genre;
                song.frequency = frequency;

                bool foundArtist = false;
                foreach (Artist artist1 in artistList)
                {
                    if (artist1.name == artist)
                    {
                        foundArtist = true;
                        song.artist = artist1;
                        artist1.songs.Add(song);
                        break;
                    }
                }

                if (!foundArtist)
                {
                    Artist newArtist = new Artist();
                    newArtist.name = artist;
                    song.artist = newArtist;
                    newArtist.songs.Add(song);
                    artistList.Add(newArtist);
                }

                songList.Add(song);
            }
            connection.Close();

            foreach (Song song in songList)
            {
                Button button = new Button();

                button.FlatStyle = FlatStyle.Flat;
                button.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
                if (darkmode)
                {
                    button.ForeColor = Color.Silver;
                }
                else
                {
                    button.ForeColor = Color.FromArgb(64, 64, 64);
                }
                button.Location = list_of_songs.Location;
                button.Margin = new Padding(0);
                button.Size = list_of_songs.Size;
                button.UseVisualStyleBackColor = false;
                button.Text = song.title;
                button.TextAlign = ContentAlignment.MiddleLeft;

                Bitmap resizedImage = new Bitmap(song.photo, new Size(30, 30));
                button.Image = resizedImage;
                button.ImageAlign = ContentAlignment.MiddleLeft;
                button.Tag = song.name;

                this.Controls.Add(button);
                button.BringToFront();

                button.Parent = flowLayoutPanel1;
                button.BackColor = Color.Transparent;

                button.Click += new EventHandler(this.button_Click);
                buttonList.Add(button);
                list_of_songs.Top += list_of_songs.Height;
            }
            flowLayoutPanel1.Visible = true;
        }

        private void AddSongsToPlaylist()
        {
            if (currentFolder == 1)
            {
                foreach (Song song in favourites)
                {
                    axWindowsMediaPlayer1.currentPlaylist.appendItem(axWindowsMediaPlayer1.newMedia(song.name));
                }
            }
            else
            {
                foreach (Song song in songList)
                {
                    axWindowsMediaPlayer1.currentPlaylist.appendItem(axWindowsMediaPlayer1.newMedia(song.name));
                }
            }
        }
        private void button_Click(object sender, EventArgs e)
        {
            play = true;
            button7.Image = Properties.Resources.pause;
            axWindowsMediaPlayer1.Ctlcontrols.stop();
            Button button = sender as Button;
            Song currentSong = new Song();
            foreach (Song song in songList)
            {
                if (song.title == button.Text)
                {
                    currentSong = song;
                    filepath = song.name;
                    label2.Text = song.title;
                    label4.Text = "Artist: " + song.artist.name;
                    label5.Text = "Genre: " + song.genre;
                    label6.Text = "Year: " + song.year;
                    label2.Visible = label4.Visible = label5.Visible = label6.Visible = true;
                    pictureBox1.Visible = true;
                    pictureBox1.Image = song.photo;
                    song.frequency++;

                    connection.Open();
                    string query = "update Song set Frequency = @newFrequency where Title = @title";
                    SQLiteCommand command = new SQLiteCommand(query, connection);
                    command.Parameters.AddWithValue("@newFrequency", song.frequency);
                    command.Parameters.AddWithValue("@title", song.title);
                    command.ExecuteNonQuery();
                    connection.Close();
                    break;
                }
            }
            axWindowsMediaPlayer1.URL = filepath;
            List<Song> list = new List<Song>();
            if (currentFolder == 1)
            {
                list = favourites;
            }
            else
            {
                list = songList;
            }

            bool found = false;
            foreach(Song song in list)
            {
                if (found)
                {
                    axWindowsMediaPlayer1.currentPlaylist.appendItem(axWindowsMediaPlayer1.newMedia(song.name));
                }
                if (song == currentSong)
                {
                    found = true;
                }
            }

            found = false;
            foreach(Song song in list)
            {
                if (song == currentSong)
                {
                    found = true;
                }
                if (!found)
                {
                    axWindowsMediaPlayer1.currentPlaylist.appendItem(axWindowsMediaPlayer1.newMedia(song.name));
                }
            }
            axWindowsMediaPlayer1.Ctlcontrols.play();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.Ctlcontrols.next();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            play = !play;
            if (play)
            {
                button7.Image = Properties.Resources.pause;
                axWindowsMediaPlayer1.Ctlcontrols.play();
            }
            else
            {
                button7.Image = Properties.Resources.play;
                axWindowsMediaPlayer1.Ctlcontrols.pause();
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.Ctlcontrols.previous();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            play = !play;
            button7.Image = Properties.Resources.play;
            axWindowsMediaPlayer1.Ctlcontrols.stop();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            pictureBox3.Visible = false;
            label1.Text = "Songs";
            if (currentFolder != 0)
            {
                label3.Visible = label7.Visible = label8.Visible = label9.Visible = label10.Visible = label11.Visible = false;
                textBox1.Visible = textBox2.Visible = textBox3.Visible = textBox4.Visible = textBox6.Visible = textBox7.Visible = false;
                button1.Visible = button15.Visible = button16.Visible = false;
                flowLayoutPanel2.Visible = false;
                createButtons();
                if (axWindowsMediaPlayer1.playState == WMPPlayState.wmppsPlaying || axWindowsMediaPlayer1.playState == WMPPlayState.wmppsPaused)
                {
                    pictureBox1.Visible = true;
                    label2.Visible = label4.Visible = label5.Visible = label6.Visible =  true;
                }
            }
            
            currentFolder = 0;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (currentFolder != 2)
            {
                label3.Visible = label7.Visible = label8.Visible = label9.Visible = label10.Visible = label11.Visible = true;
                textBox1.Visible = textBox2.Visible = textBox3.Visible = textBox4.Visible = textBox6.Visible = textBox7.Visible = true;
                button15.Visible = button16.Visible = false;
                button1.Visible = true;
            }
            currentFolder = 2;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (currentFolder != 3)
            {
                label3.Visible = textBox1.Visible = true;
                label7.Visible = label8.Visible = label9.Visible = label10.Visible = label11.Visible = false;
                textBox2.Visible = textBox3.Visible = textBox4.Visible = textBox6.Visible = textBox7.Visible = false;
                button1.Visible = button15.Visible = false;
                button16.Visible = true;
            }
            currentFolder = 3;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (currentFolder != 4)
            {
                label3.Visible = label7.Visible = label8.Visible = label9.Visible = label10.Visible = label11.Visible = true;
                textBox1.Visible = textBox2.Visible = textBox3.Visible = textBox4.Visible = textBox6.Visible = textBox7.Visible = true;
                button1.Visible = button16.Visible = false;
                button15.Visible = true;
            }
            currentFolder = 4;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bool alreadyExists = false;
            flowLayoutPanel2.Visible = false;
            foreach(Song song in songList)
            {
                if (song.title == "          " + textBox1.Text)
                {
                    alreadyExists = true;
                    break;
                }
            }

            if (!alreadyExists && textBox1.Text != "")
            {
                if (textBox2.Text != "")
                {
                    if (File.Exists(textBox2.Text))
                    {
                        connection.Open();
                        string insertSQL = "insert into Song (Title, Name, Artist, Frequency, Photo, Year, Genre) " +
                                           "values (@title, @name, @artist, @frequency, @photo, @year, @genre)";
                        SQLiteCommand command = new SQLiteCommand(insertSQL, connection);
                        command.Parameters.AddWithValue("@title", "          " + textBox1.Text);
                        command.Parameters.AddWithValue("@name", textBox2.Text);
                        if (textBox3.Text == "")
                        {
                            command.Parameters.AddWithValue("@artist", "Unknown");
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@artist", textBox3.Text);
                        }
                        command.Parameters.AddWithValue("@frequency", 0);
                        command.Parameters.AddWithValue("@photo", textBox4.Text);
                        if (textBox6.Text == "")
                        {
                            command.Parameters.AddWithValue("@year", 2024);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@year", int.Parse(textBox6.Text));
                        }
                        if (textBox7.Text == "")
                        {
                            command.Parameters.AddWithValue("@genre", "None");
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@genre", textBox7.Text);
                        }
                        command.ExecuteNonQuery();
                        connection.Close();

                        textBox1.Text = textBox2.Text = textBox3.Text = textBox4.Text = textBox6.Text = textBox7.Text = "";
                        label3.Visible = label7.Visible = label8.Visible = label9.Visible = label10.Visible = label11.Visible = false;
                        textBox1.Visible = textBox2.Visible = textBox3.Visible = textBox4.Visible = textBox6.Visible = textBox7.Visible = false;
                        button1.Visible = button15.Visible = button16.Visible = false;

                        currentFolder = 10;
                        createButtons();
                        if (axWindowsMediaPlayer1.playState == WMPPlayState.wmppsPlaying || axWindowsMediaPlayer1.playState == WMPPlayState.wmppsPaused)
                        {
                            pictureBox1.Visible = true;
                            label2.Visible = label4.Visible = label5.Visible = label6.Visible = true;
                        }
                    }
                    else
                    {
                        MessageBox.Show("The file does not exist.");
                    }
                }
                else
                {
                    MessageBox.Show("The filepath is empty.");
                }
            }
            else
            {
                MessageBox.Show("Non valid title.");
            }
            
        }

        private void button16_Click(object sender, EventArgs e)
        {
            flowLayoutPanel2.Visible = false;
            bool found = false;
            foreach (Song song in songList)
            {
                if (song.title == "          " + textBox1.Text)
                {
                    found = true;
                }
            }
            if (found)
            {
                connection.Open();

                string deleteSQL = "delete from Song where Title = @title";
                SQLiteCommand command = new SQLiteCommand(deleteSQL, connection);
                command.Parameters.AddWithValue("@title", "          " + textBox1.Text);
                command.ExecuteNonQuery();

                connection.Close();

                songList.RemoveAll(song => song.name == "          " + textBox1.Text);
                favourites.RemoveAll(song => song.name == "          " + textBox1.Text);

                textBox1.Text = "";
                label3.Visible = label7.Visible = label8.Visible = label9.Visible = label10.Visible = label11.Visible = false;
                textBox1.Visible = textBox2.Visible = textBox3.Visible = textBox4.Visible = textBox6.Visible = textBox7.Visible = false;
                button1.Visible = button15.Visible = button16.Visible = false;

                currentFolder = 10;
                createButtons();
                if (axWindowsMediaPlayer1.playState == WMPPlayState.wmppsPlaying || axWindowsMediaPlayer1.playState == WMPPlayState.wmppsPaused)
                {
                    pictureBox1.Visible = true;
                    label2.Visible = label4.Visible = label5.Visible = label6.Visible = true;
                }
            }
            else
            {
                MessageBox.Show("This song does not exit.");
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            flowLayoutPanel2.Visible = false;
            bool found = false;
            foreach(Song song in songList)
            {
                if (song.title == "          " + textBox1.Text)
                {
                    found = true;
                    break;
                }
            }

            if (found)
            {
                if (textBox2.Text != "")
                {
                    if (File.Exists(textBox2.Text))
                    {
                        connection.Open();
                        string updateSQL = "update Song set " +
                                           "Name = @name," +
                                           "Artist = @artist," +
                                           "Photo = @photo," +
                                           "Year = @year," +
                                           "Genre = @genre " +
                                           "where Title = @title";

                        SQLiteCommand command = new SQLiteCommand(updateSQL, connection);
                        command.Parameters.AddWithValue("@title", "          " + textBox1.Text);
                        command.Parameters.AddWithValue("@name", textBox2.Text);
                        if (textBox3.Text == "")
                        {
                            command.Parameters.AddWithValue("@artist", "Unknown");
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@artist", textBox3.Text);
                        }
                        command.Parameters.AddWithValue("@photo", textBox4.Text);
                        if (textBox6.Text == "")
                        {
                            command.Parameters.AddWithValue("@year", 2024);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@year", int.Parse(textBox6.Text));
                        }
                        if (textBox7.Text == "")
                        {
                            command.Parameters.AddWithValue("@genre", "None");
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@genre", textBox7.Text);
                        }
                        command.ExecuteNonQuery();
                        connection.Close();

                        textBox1.Text = textBox2.Text = textBox3.Text = textBox4.Text = textBox6.Text = textBox7.Text = "";
                        label3.Visible = label7.Visible = label8.Visible = label9.Visible = label10.Visible = label11.Visible = false;
                        textBox1.Visible = textBox2.Visible = textBox3.Visible = textBox4.Visible = textBox6.Visible = textBox7.Visible = false;
                        button1.Visible = button15.Visible = button16.Visible = false;

                        currentFolder = 10;
                        createButtons();
                        if (axWindowsMediaPlayer1.playState == WMPPlayState.wmppsPlaying || axWindowsMediaPlayer1.playState == WMPPlayState.wmppsPaused)
                        {
                            pictureBox1.Visible = true;
                            label2.Visible = label4.Visible = label5.Visible = label6.Visible = true;
                        }
                    }
                    else
                    {
                        MessageBox.Show("The file does not exist.");
                    }
                }
                else
                {
                    MessageBox.Show("The filepath is empty.");
                }
            }
            else
            {
                MessageBox.Show("This song does not exist.");
            }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            if (darkmode)
            {
                darkmode = false;
                button17.Visible = false;
                button18.Visible = false;
                button20.Visible = false;
                settings = true;
                panel3.BackgroundImage = Properties.Resources.light_mode;
                panel2.BackColor = Color.RosyBrown;
                panel1.BackColor = Color.Beige;
                button17.BackColor = button18.BackColor = button20.BackColor = Color.Beige;
                button7.ForeColor = button8.ForeColor = button9.ForeColor = button10.ForeColor = button11.ForeColor = button12.ForeColor = button13.ForeColor = button14.ForeColor = Color.Beige;
                button2.ForeColor = button3.ForeColor = button4.ForeColor = button5.ForeColor = button6.ForeColor = button19.ForeColor = Color.FromArgb(64, 64, 64);
                label1.ForeColor = Color.Black;
                label2.ForeColor = label3.ForeColor = label4.ForeColor = label5.ForeColor = label6.ForeColor = label7.ForeColor = label8.ForeColor =
                label9.ForeColor = label10.ForeColor = label11.ForeColor = Color.FromArgb(64, 64, 64);
                foreach (Button button in buttonList)
                {
                    button.ForeColor = Color.FromArgb(64, 64, 64);
                }
                foreach (Label label in labelList)
                {
                    label.ForeColor = Color.FromArgb(64, 64, 64);
                }
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            if (!darkmode)
            {
                darkmode = true;
                button17.Visible = false;
                button18.Visible = false;
                button20.Visible = false;
                settings = true;
                panel3.BackgroundImage = Properties.Resources.headphones;
                panel2.BackColor = Color.FromArgb(64, 64, 64);
                panel1.BackColor = Color.Gray;
                button17.BackColor = button18.BackColor = button20.BackColor = Color.Gray;
                button7.ForeColor = button8.ForeColor = button9.ForeColor = button10.ForeColor = button11.ForeColor = button12.ForeColor = button13.ForeColor = button14.ForeColor = button19.ForeColor = Color.Gray;
                button2.ForeColor = button3.ForeColor = button4.ForeColor = button5.ForeColor = button6.ForeColor = button19.ForeColor = Color.Silver;
                label1.ForeColor = Color.WhiteSmoke;
                label2.ForeColor = label3.ForeColor = label4.ForeColor = label5.ForeColor = label6.ForeColor = label7.ForeColor = label8.ForeColor =
                label9.ForeColor = label10.ForeColor = label11.ForeColor = Color.Silver;
                foreach (Button button in buttonList)
                {
                    button.ForeColor = Color.Silver;
                }
                foreach (Label label in labelList)
                {
                    label.ForeColor = Color.Silver;
                }
            }
        }

        private void button19_Click(object sender, EventArgs e)
        {
            if (currentFolder != 5)
            {
                pictureBox3.Visible = false;
                label1.Text = "Artists";
                pictureBox1.Visible = false;
                label2.Visible = label4.Visible = label5.Visible = label6.Visible = false;
                label3.Visible = label7.Visible = label8.Visible = label9.Visible = label10.Visible = label11.Visible = false;
                textBox1.Visible = textBox2.Visible = textBox3.Visible = textBox4.Visible = textBox6.Visible = textBox7.Visible = false;
                button1.Visible = button15.Visible = button16.Visible = false;

                flowLayoutPanel1.Visible = false;
                foreach (Button button in buttonList)
                {
                    button.Dispose();
                }
                buttonList.Clear();

                flowLayoutPanel2.Visible = false;
                foreach (PictureBox pictureBox in pictureBoxList)
                {
                    pictureBox.Dispose();
                }
                pictureBoxList.Clear();

                foreach(Label label in labelList)
                {
                    label.Dispose();
                }
                labelList.Clear();  

                foreach (Artist artist in artistList)
                {
                    PictureBox pictureBox = new PictureBox();
                    pictureBox.Top = pictureBox2.Top;
                    pictureBox.Left = pictureBox2.Left;
                    pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox.Size = pictureBox2.Size;
                    pictureBox.BackColor = Color.Transparent;
                    pictureBox.Image = Properties.Resources.music;
                    pictureBox.Tag = artist.name;
                    if (Properties.Resources.ResourceManager.GetObject(artist.name) != null)
                    {
                        pictureBox.Image = (Image)Properties.Resources.ResourceManager.GetObject(artist.name);
                    }
                    else
                    {
                        pictureBox.Image = (Image)Properties.Resources.Unkown_artist;
                    }
                    this.Controls.Add(pictureBox);

                    pictureBox.Parent = flowLayoutPanel2;
                    pictureBox.BackColor = Color.Transparent;
                    pictureBox.Padding = new Padding(10, 10, 10, 10);
                    pictureBox.Click += new EventHandler(this.pictureBox_click);
                    pictureBox.MouseEnter += new EventHandler(this.pictureBox_mouseEnter);
                    pictureBox.MouseLeave += new EventHandler(this.pictureBox_mouseLeave);

                    Label artist_name = new Label();
                    artist_name.Text = artist.name;
                    artist_name.Top = pictureBox.Top + pictureBox.Height;
                    if (darkmode)
                    {
                        artist_name.ForeColor = Color.Silver;
                    }
                    else
                    {
                        artist_name.ForeColor = Color.FromArgb(64, 64, 64);
                    }
                    this.Controls.Add(artist_name);  
                    artist_name.Parent = flowLayoutPanel2;
                    artist_name.BackColor = Color.Transparent;
                    artist_name.Font = new Font(label1.Font.FontFamily, 12);
                    labelList.Add(artist_name);
                    pictureBoxList.Add(pictureBox);
                }
                flowLayoutPanel2.Visible = true;
            }
            currentFolder = 5;
        }

        private void pictureBox_mouseEnter(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        private void pictureBox_mouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
        }

        private void pictureBox_click(object sender, EventArgs e)
        {
            currentFolder = 10;
            flowLayoutPanel1.Visible = false;
            foreach (Button button in buttonList)
            {
                button.Dispose();
            }

            PictureBox pictureBox = (PictureBox)sender;

            foreach (Artist artist in artistList)
            {
                if (artist.name == (string)pictureBox.Tag)
                {
                    flowLayoutPanel2.Visible = false;
                    label1.Text = artist.name;
                    pictureBox3.Visible = true;
                    if (Properties.Resources.ResourceManager.GetObject(artist.name) != null)
                    {
                        pictureBox3.Image = (Image)Properties.Resources.ResourceManager.GetObject(artist.name);
                    }
                    else
                    {
                        pictureBox3.Image = (Image)Properties.Resources.Unkown_artist;
                    }
                    pictureBox3.Left = label1.Left + label1.Width + 5;

                    if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPlaying || axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPaused)
                    {
                        pictureBox1.Visible = true;
                        label2.Visible = label4.Visible = label5.Visible = label6.Visible = true;
                    }
                    
                    foreach (Song song in artist.songs)
                    {
                        Button button = new Button();

                        button.FlatStyle = FlatStyle.Flat;
                        button.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
                        if (darkmode)
                        {
                            button.ForeColor = Color.Silver;
                        }
                        else
                        {
                            button.ForeColor = Color.FromArgb(64, 64, 64);
                        }
                        button.Location = list_of_songs.Location;
                        button.Margin = new Padding(0);

                        button.Size = list_of_songs.Size;
                        button.UseVisualStyleBackColor = false;
                        button.Text = song.title;
                        button.ImageAlign = ContentAlignment.MiddleLeft;

                        Bitmap resizedImage = new Bitmap(song.photo, new Size(30, 30));
                        button.Image = resizedImage;

                        button.TextAlign = ContentAlignment.MiddleLeft;
                        button.Tag = song.name;

                        this.Controls.Add(button);

                        button.Parent = flowLayoutPanel1;
                        button.BackColor = Color.Transparent;

                        button.Click += new EventHandler(this.button_Click);
                        buttonList.Add(button);
                        list_of_songs.Top += list_of_songs.Height;
                    }
                    flowLayoutPanel1.Visible = true;
                    break;
                }
            }
        }

        private void button20_Click(object sender, EventArgs e)
        {
            currentFolder = 10;
            pictureBox1.Visible = false;
            label2.Visible = label4.Visible = label5.Visible = label6.Visible = false;
            button17.Visible = button18.Visible = button20.Visible = false;
            label1.Text = "Select Backround";
            pictureBox3.Visible = false;
            flowLayoutPanel1.Visible = false;
            flowLayoutPanel2.Visible = false;
            foreach(PictureBox pictureBox in pictureBoxList)
            {
                pictureBox.Dispose();
            }
            pictureBoxList.Clear();

            foreach (Label label in labelList)
            {
                label.Dispose();
            }
            labelList.Clear();

            foreach (Image image in imageList)
            {
                PictureBox pictureBox = new PictureBox();
                pictureBox.Top = pictureBox4.Top;
                pictureBox.Left = pictureBox4.Left;
                pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox.Size = pictureBox4.Size;
                pictureBox.BackColor = Color.Transparent;
                pictureBox.Image = image;
                this.Controls.Add(pictureBox);

                pictureBox.Parent = flowLayoutPanel2;
                pictureBox.BackColor = Color.Transparent;
                pictureBox.Padding = new Padding(10, 10, 10, 10);
                pictureBox.Click += new EventHandler(this.background_click);
                pictureBox.MouseEnter += new EventHandler(this.pictureBox_mouseEnter);
                pictureBox.MouseLeave += new EventHandler(this.pictureBox_mouseLeave);
                pictureBoxList.Add(pictureBox); 
            }
            flowLayoutPanel2.Visible = true;
        }

        private void background_click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
            settings = true;
            
            if (sender is PictureBox)
            {
                PictureBox pictureBox = (PictureBox)sender;
                panel3.BackgroundImage = pictureBox.Image;  
            }
            flowLayoutPanel2.Visible = false;
            foreach (PictureBox pictureBox in pictureBoxList)
            {
                pictureBox.Dispose();
            }

            pictureBoxList.Clear();
            
            createButtons();
            flowLayoutPanel1.Visible = true;
            label1.Text = "Songs";
            if (axWindowsMediaPlayer1.playState == WMPPlayState.wmppsPlaying || axWindowsMediaPlayer1.playState == WMPPlayState.wmppsPaused)
            {
                pictureBox1.Visible = true;
                label2.Visible = label4.Visible = label5.Visible = label6.Visible = true;
            }
            if (!darkmode)
            {
                darkmode = true;
                button17.Visible = false;
                button18.Visible = false;
                button20.Visible = false;
                settings = true;
                panel2.BackColor = Color.FromArgb(64, 64, 64);
                panel1.BackColor = Color.Gray;
                button17.BackColor = button18.BackColor = button20.BackColor = Color.Gray;
                button7.ForeColor = button8.ForeColor = button9.ForeColor = button10.ForeColor = button11.ForeColor = button12.ForeColor = button13.ForeColor = button14.ForeColor = button19.ForeColor = Color.Gray;
                button2.ForeColor = button3.ForeColor = button4.ForeColor = button5.ForeColor = button6.ForeColor = button19.ForeColor = Color.Silver;
                label1.ForeColor = Color.WhiteSmoke;
                label2.ForeColor = label3.ForeColor = label4.ForeColor = label5.ForeColor = label6.ForeColor = label7.ForeColor = label8.ForeColor =
                label9.ForeColor = label10.ForeColor = label11.ForeColor = Color.Silver;
                foreach (Button button in buttonList)
                {
                    button.ForeColor = Color.Silver;
                }
                foreach (Label label in labelList)
                {
                    label.ForeColor = Color.Silver;
                }
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            currentFolder = 10;
            label1.Text = "Songs";
            pictureBox3.Visible = false;
            play = true;
            flowLayoutPanel1.Visible = false;
            flowLayoutPanel2.Visible = false;
            foreach (Button button in buttonList)
            {
                button.Dispose();
            }
            buttonList.Clear();

            Random random = new Random();

            int n = songList.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);

                Song temp = (Song)songList[k];
                songList[k] = songList[n];
                songList[n] = temp;
            }
            Song currentSong = new Song();
            bool first = true;
            foreach (Song song in songList)
            {
                if (first)
                {
                    currentSong = song;
                    first = false;
                }
                Button button = new Button();

                button.FlatStyle = FlatStyle.Flat;
                button.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
                if (darkmode)
                {
                    button.ForeColor = Color.Silver;
                }
                else
                {
                    button.ForeColor = Color.FromArgb(64, 64, 64);
                }
                button.Location = list_of_songs.Location;
                button.Margin = new Padding(0);
                button.Size = list_of_songs.Size;
                button.UseVisualStyleBackColor = false;
                button.Text = song.title;
                button.TextAlign = ContentAlignment.MiddleLeft;

                Bitmap resizedImage = new Bitmap(song.photo, new Size(30, 30));
                button.Image = resizedImage;
                button.ImageAlign = ContentAlignment.MiddleLeft;
                button.Tag = song.name;

                this.Controls.Add(button);
                button.BringToFront();

                button.Parent = flowLayoutPanel1;
                button.BackColor = Color.Transparent;

                button.Click += new EventHandler(this.button_Click);
                buttonList.Add(button);
                list_of_songs.Top += list_of_songs.Height;
            }
            flowLayoutPanel1.Visible = true;
            filepath = songList[0].name;
            pictureBox1.Image = songList[0].photo;
            pictureBox1.Visible = true;

            button7.Image = Properties.Resources.pause;

            label2.Text = songList[0].title;
            label4.Text = "Artist: " + songList[0].artist.name;
            label5.Text = "Genre: " + songList[0].genre;
            label6.Text = "Year: " + songList[0].year;
            label2.Visible = label4.Visible = label5.Visible = label6.Visible = true;

            axWindowsMediaPlayer1.URL = filepath;

            bool found = false;
            foreach (Song song in songList)
            {
                if (found)
                {
                    axWindowsMediaPlayer1.currentPlaylist.appendItem(axWindowsMediaPlayer1.newMedia(song.name));
                }
                if (song == currentSong)
                {
                    found = true;
                }
            }

            found = false;
            foreach (Song song in songList)
            {
                if (song == currentSong)
                {
                    found = true;
                }
                if (!found)
                {
                    axWindowsMediaPlayer1.currentPlaylist.appendItem(axWindowsMediaPlayer1.newMedia(song.name));
                }
            }
            axWindowsMediaPlayer1.Ctlcontrols.play();
        }

        private void axWindowsMediaPlayer1_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            if ((WMPPlayState)e.newState == WMPPlayState.wmppsPlaying)
            {
                foreach (Song song in songList)
                {
                    if (axWindowsMediaPlayer1.currentMedia.sourceURL.Contains(song.name))
                    {
                        pictureBox1.Image = song.photo;
                        label2.Text = song.title;
                        label4.Text = "Artist: " + song.artist.name;
                        label5.Text = "Genre: " + song.genre;
                        label6.Text = "Year: " + song.year.ToString();
                        break;
                    }
                }
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (settings)
            {
                button17.Visible = button18.Visible = button20.Visible = true;
            }
            else
            {
                button17.Visible = button18.Visible = button20.Visible = false;
            }
            settings = !settings;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (currentFolder != 1)
            {
                pictureBox3.Visible = false;
                label1.Text = "Top 10 Songs";
                label3.Visible = label7.Visible = label8.Visible = label9.Visible = label10.Visible = label11.Visible = false;
                textBox1.Visible = textBox2.Visible = textBox3.Visible = textBox4.Visible = textBox6.Visible = textBox7.Visible = false;
                button1.Visible = button15.Visible = button16.Visible = false;
                flowLayoutPanel2.Visible = false;
                favourites.Clear();
                flowLayoutPanel1.Visible = false;
                foreach (Button button in buttonList)
                {
                    button.Dispose();
                }
                buttonList.Clear();

                connection.Open();
                string selectTitlesSQL = "Select * from Song";
                SQLiteCommand command2 = new SQLiteCommand(selectTitlesSQL, connection);
                SQLiteDataReader sQLiteDataReader = command2.ExecuteReader();

                string title, name, artist, photo, genre;
                float frequency;
                int year;
                Image image;
                Artist currentArtist = new Artist();
                while (sQLiteDataReader.Read())
                {
                    title = sQLiteDataReader.GetString(0);
                    name = sQLiteDataReader.GetString(1);
                    artist = sQLiteDataReader.GetString(2);
                    frequency = sQLiteDataReader.GetFloat(3);
                    photo = sQLiteDataReader.GetString(4);
                    if (photo == "" || Properties.Resources.ResourceManager.GetObject(photo) == null)
                    {
                        image = Properties.Resources.music;
                        image.Tag = "music";
                    }
                    else
                    {
                        image = (Image)Properties.Resources.ResourceManager.GetObject(photo);
                        image.Tag = photo;
                    }
                    year = sQLiteDataReader.GetInt32(5);
                    genre = sQLiteDataReader.GetString(6);

                    foreach (Artist artist1 in artistList)
                    {
                        if (artist1.name == artist)
                        {
                            currentArtist = artist1;
                            break;
                        }
                        else
                        {
                            ArrayList songs = new ArrayList();
                            currentArtist = new Artist(artist, songs);
                        }
                    }
                    
                    favourites.Add(new Song(name, title, currentArtist, frequency, image, year, genre));
                }
                connection.Close();

                favourites = songList.OrderBy(song => song.frequency).ToList();
                favourites.Reverse();
                int count = 0;
                foreach (Song song in favourites)
                {
                    if (count == numberOfFavourites)
                    {
                        break;
                    }

                    Button button = new Button();

                    button.FlatStyle = FlatStyle.Flat;
                    button.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
                    if (darkmode)
                    {
                        button.ForeColor = Color.Silver;
                    }
                    else
                    {
                        button.ForeColor = Color.FromArgb(64, 64, 64);
                    }
                    button.Location = list_of_songs.Location;
                    button.Margin = new Padding(0);
                    button.Size = list_of_songs.Size;
                    button.UseVisualStyleBackColor = false;
                    button.Text = song.title;
                    button.TextAlign = ContentAlignment.MiddleLeft;

                    Bitmap resizedImage = new Bitmap(song.photo, new Size(30, 30));
                    button.Image = resizedImage;
                    button.ImageAlign = ContentAlignment.MiddleLeft;
                    button.Tag = song.name;

                    this.Controls.Add(button);
                    button.BringToFront();

                    button.Parent = flowLayoutPanel1;
                    button.BackColor = Color.Transparent;

                    button.Click += new EventHandler(this.button_Click);
                    buttonList.Add(button);
                    list_of_songs.Top += list_of_songs.Height;
                    count++;
                }
                if(axWindowsMediaPlayer1.playState == WMPPlayState.wmppsPlaying || axWindowsMediaPlayer1.playState == WMPPlayState.wmppsPaused)
                {
                    pictureBox1.Visible = true;
                    label2.Visible = label4.Visible = label5.Visible = label6.Visible = true;
                }
            }
            flowLayoutPanel1.Visible = true;
            currentFolder = 1;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.Ctlcontrols.stop();
            axWindowsMediaPlayer1.Ctlcontrols.play();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.settings.volume = 10 * trackBar1.Value;
        }

        private void button14_Click(object sender, EventArgs e)
        {
            mute = !mute;
            if (mute)
            {
                trackBar1.Visible = false;
                button14.Image = Properties.Resources.mute;
                axWindowsMediaPlayer1.settings.volume = 0;
            }
            else
            {
                trackBar1.Visible = true;
                button14.Image = Properties.Resources.volume;
                axWindowsMediaPlayer1.settings.volume = 10 * trackBar1.Value;
            }
        }
    }
}
