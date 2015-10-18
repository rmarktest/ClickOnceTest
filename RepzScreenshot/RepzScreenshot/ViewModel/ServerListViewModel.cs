using RepzScreenshot.DataAccess;
using RepzScreenshot.Error;
using RepzScreenshot.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace RepzScreenshot.ViewModel
{
    class ServerListViewModel : WorkspaceViewModel
    {
        
        private Timer RefreshTimer = new Timer(5000);

        #region properties
        public ObservableCollection<ServerViewModel> Servers { get; private set; }
        
        public override string Title
        {
            get
            {
                return "Server Browser";
            }
            
        }

        private RepzDataAccess RepzDataAccess { get; set; }



        #endregion // properties


        #region commands

        #endregion commands


        #region constructor
        public ServerListViewModel():base(false)
        {
            RepzDataAccess = new RepzDataAccess();
            Servers = new ObservableCollection<ServerViewModel>();

            LoadServers();

            RefreshTimer.Elapsed += RefreshTimer_Elapsed;
            //RefreshTimer.Start();
        }

        
        #endregion //constructor


        #region command methods

        
        #endregion //command methods


        #region methods

        
        private async void LoadServers()
        {
            IsLoading = true;
            RefreshTimer.Stop();
            
            try
            {
                List<Server> servers = await RepzDataAccess.GetServersAsync();
                Servers.Clear();

                foreach (Server s in servers)
                {
                    Servers.Add(new ServerViewModel(s));
                }
                RefreshTimer.Start();
            }
            catch(ExceptionBase ex)
            {
                SetError(ex, LoadServers);
            }
            catch(Exception)
            {
                SetError(new Exception("Unknown error"), LoadServers);
            }
            IsLoading = false;
        }

        private async void UpdateServers()
        {
            if (IsLoading)
            {
                Console.WriteLine("Skipping update of ServerList");
                return;
            }
                
            IsLoading = true;
           
            try
            {
                await RepzDataAccess.UpdateCollection(Servers, RepzDataAccess.GetServersAsync, x => x.Server, x => x.Hostname, x => test(x));
                RefreshTimer.Start();
            }
            catch(ExceptionBase ex)
            {
                SetError(ex, UpdateServers);
                RefreshTimer.Stop();
            }
            catch (Exception)
            {
                SetError(new Exception("Unknown error"), UpdateServers);
                RefreshTimer.Stop();
            }
            finally
            {
                IsLoading = false;
                
            }
            

        }
        private void test(Server s)
        {
            ServerViewModel vm = new ServerViewModel(s);
            Servers.Add(vm);
        }
        #endregion //methods


        #region event handler methods

        void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            
            Console.WriteLine("Timer_Elapsed: enabled " + RefreshTimer.Enabled);
           
            
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                RefreshTimer.Stop();
                UpdateServers();
                RefreshTimer.Start();
            });
            
        }

        #endregion //event handler methods


    }
}
