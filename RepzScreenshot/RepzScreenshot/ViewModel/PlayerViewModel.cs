using Microsoft.Win32;
using RepzScreenshot.DataAccess;
using RepzScreenshot.Error;
using RepzScreenshot.Helper;
using RepzScreenshot.Model;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RepzScreenshot.ViewModel
{
    class PlayerViewModel : WorkspaceViewModel
    {

        private BitmapImage screenshot;
        private string screenshotUrl;
        private DateTime? screenshotDate;
        private bool isScreenUploaded;

        #region properties
        public Player Player { get; private set; }

        private RepzDataAccess RepzDataAccess { get; set; }

        public override string Title
        {
            get
            {
                return PlayerName;
            }
        }

        public int PlayerId
        {
            get
            {
                return Player.Id;
            }
            set
            {
                if (value != Player.Id)
                {
                    Player.Id = value;
                    NotifyPropertyChanged("PlayerId");
                }
            }
        }

        public string PlayerName 
        { 
            get 
            {
                return UIHelper.RemoveColor(Player.Name);
            }
            set 
            { 
                if(value != Player.Name)
                {
                    Player.Name = value;
                    NotifyPropertyChanged("PlayerName");
                }
            }
        }

        public int PlayerScore
        {
            get
            {
                return Player.Score;
            }
            set
            {
                if (value != Player.Score)
                {
                    Player.Score = value;
                    NotifyPropertyChanged("PlayerScore");
                }
            }
        }

        public int PlayerPing
        {
            get
            {
                return Player.Ping;
            }
            set
            {
                if (value != Player.Ping)
                {
                    Player.Ping = value;
                    NotifyPropertyChanged("PlayerPing");
                }
            }
        }

        
        public BitmapImage Screenshot
        {
            get
            {
                return screenshot;
            }
            set
            {
                if(screenshot != value)
                {
                    screenshot = value;
                    NotifyPropertyChanged("Screenshot");
                    SaveImageCommand.NotifyCanExecuteChanged();
                    UploadImageCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public string ScreenshotUrl
        {
            get
            {
                return screenshotUrl;
            }
            set
            {
                if (screenshotUrl != value)
                {
                    screenshotUrl = value;
                    NotifyPropertyChanged("ScreenshotUrl");
                    
                }
            }
        }

        public bool IsScreenUploaded
        {
            get
            {
                return isScreenUploaded;
            }
            set
            {
                if (isScreenUploaded != value)
                {
                    isScreenUploaded = value;
                    NotifyPropertyChanged("IsScreenUploaded");

                }
            }
        }

        
        public DateTime? ScreenshotDate
        {
            get
            {
                return screenshotDate;
            }
            private set
            {
                if (screenshotDate != value)
                {
                    screenshotDate = value;
                    NotifyPropertyChanged("ScreenshotDate");
                }
            }
        }

        
        public string ServerHostname
        {
            get
            {
                return UIHelper.RemoveColor(Player.Hostname);
            }
            set
            {
                if (Player.Hostname != value)
                {
                    Player.Hostname = value;
                    NotifyPropertyChanged("ServerHostname");
                }
            }
        }

        public Brush StatusBrush {
            get
            {
                if (Error is ErrorViewModel && Error.ErrorMessage != String.Empty)
                    return Brushes.Red;
                else if (IsLoading)
                    return Brushes.Yellow;
                else if (Screenshot is BitmapImage)
                    return MainWindowViewModel.Workspaces.Contains(this) ? Brushes.Green : Brushes.LightGreen;
                
                else
                    return null;
            }
        }

        #endregion //properties


        #region Commands

        public Command ScreenshotCommand { get; protected set; }
        public Command ReloadCommand { get; protected set; }
        public Command SaveImageCommand { get; protected set; }

        public Command UploadImageCommand { get; protected set; }


        private void InitCommands()
        {
            ScreenshotCommand = new Command(CmdGetScreenshot, CanGetScreenshot);
            ReloadCommand = new Command(CmdReload, CanReload);
            SaveImageCommand = new Command(CmdSaveImage, CanSaveImage);
            UploadImageCommand = new Command(CmdUploadImage, CanUploadImage);
        }
        #endregion //Commands


        #region constructor

        public PlayerViewModel(Player player)
        {
            Player = player;

            Player.PropertyChanged += Player_PropertyChanged;
            this.PropertyChanged += PlayerViewModel_PropertyChanged;
            InitCommands();

            RepzDataAccess = new RepzDataAccess();
        }


        #endregion


        #region Command methods

        private bool CanGetScreenshot()
        {
            return !(MainWindowViewModel.Workspaces.Contains(this));
        }

        private void CmdGetScreenshot()
        {
            MainWindowViewModel.AddWorkspace(this);
            NotifyPropertyChanged("StatusBrush");
            this.RequestClose += PlayerViewModel_RequestClose;
            ScreenshotCommand.NotifyCanExecuteChanged();

            if(Screenshot == null && Error == null)
                GetScreenshot();
        }

        private bool CanReload()
        {
            return (!IsLoading);
        }

        private void CmdReload()
        {
            GetScreenshot();
        }

        private bool CanSaveImage()
        {
            return (Screenshot is BitmapImage);
        }

        private void CmdSaveImage()
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";
            saveFileDialog1.Title = "Save Screenshot";
            saveFileDialog1.FileName = Player.Name;
            if (saveFileDialog1.ShowDialog() == true)
            {

                if (saveFileDialog1.FileName != "")
                {
                    using (System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile())
                    {

                        BitmapEncoder encoder;
                        switch (saveFileDialog1.FilterIndex)
                        {
                            default:
                                encoder = new JpegBitmapEncoder();
                                encoder.Frames.Add(BitmapFrame.Create(Screenshot));
                                break;

                            case 2:
                                encoder = new BmpBitmapEncoder();
                                encoder.Frames.Add(BitmapFrame.Create(Screenshot));
                                break;

                            case 3:
                                encoder = new GifBitmapEncoder();
                                encoder.Frames.Add(BitmapFrame.Create(Screenshot));
                                break;
                        }
                        encoder.Save(fs);
                        fs.Close();
                    }
                }
            }
        }

        private bool CanUploadImage()
        {
            return CanSaveImage() && !IsScreenUploaded && !IsLoading;
        }

        private async void CmdUploadImage()
        {
            IsLoading = true;
            try
            {
                string url = await ImgurDataAccess.UploadImage(Player, Screenshot);
                ScreenshotUrl = url;
                IsScreenUploaded = true;
            }
            catch (ExceptionBase ex)
            {
                SetError(ex, CmdUploadImage);
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion


        #region methods

        private async void GetScreenshot()
        {
            IsLoading = true;
            await RepzDataAccess.GetIdAsync(Player);
            try
            {
                await RepzDataAccess.GetPresenceDataAsync(Player);
                Screenshot = await RepzDataAccess.GetScreenshotAsync(Player);
                ScreenshotDate = DateTime.Now;
                IsScreenUploaded = false;
            }
            catch (ExceptionBase ex)
            {
                SetError(ex, CmdReload);
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                IsLoading = false;
            }
           
        }
        

        #endregion //methods


        #region event handler methods
        void PlayerViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "IsLoading":
                    ReloadCommand.NotifyCanExecuteChanged();
                    NotifyPropertyChanged("StatusBrush");
                    UploadImageCommand.NotifyCanExecuteChanged();
                    break;
                case "Error":
                    NotifyPropertyChanged("StatusBrush");
                    break;
                case "PlayerName":
                    NotifyPropertyChanged("Title");
                    break;
                case "IsScreenUploaded":
                    UploadImageCommand.NotifyCanExecuteChanged();
                    break;
            }
        }

        void PlayerViewModel_RequestClose(object sender, EventArgs e)
        {
            ScreenshotCommand.NotifyCanExecuteChanged();
            NotifyPropertyChanged("StatusBrush");
        }

        void Player_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            string property = null;
            switch(e.PropertyName)
            {
                case "Id":
                    property = "PlayerId";
                    break;
                case "Name":
                    property = "PlayerName";
                    break;
                case "Score":
                    property = "PlayerScore";
                    break;
                case "Ping":
                    property = "PlayerPing";
                    break;
                case "Hostname":
                    property = "ServerHostname";
                    break;
                    
            }
            if (property != null)
                NotifyPropertyChanged(property);
        }

        #endregion

        
    }
}
