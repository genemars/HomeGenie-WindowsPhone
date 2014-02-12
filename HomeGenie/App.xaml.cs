using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using HomeGenie;
using HomeGenie.ViewModel;
using Microsoft.Phone.Notification;
using Microsoft.Phone.Info;
using System.IO.IsolatedStorage;
using Coding4Fun.Toolkit.Controls;

namespace HomeGenie
{
    public partial class App : Application
    {
        private static MainViewModel viewModel = null;
        private static HTTPRequestQueue _httpreqhandler = new HTTPRequestQueue();

        /// Holds the push channel that is created or found.
        private static HttpNotificationChannel pushChannel;

        /// <summary>
        /// Oggetto ViewModel statico utilizzato dalle visualizzazioni con cui eseguire l'associazione.
        /// </summary>
        /// <returns>Oggetto MainViewModel.</returns>
        public static MainViewModel ViewModel
        {
            get
            {
                // Ritardare la creazione del modello di visualizzazione finché necessario
                if (viewModel == null)
                    viewModel = new MainViewModel();

                return viewModel;
            }
        }

        #region Utility App Methods

        public static HTTPRequestQueue HttpManager
        {
            get { return _httpreqhandler; }
        }

        #endregion

        /// <summary>
        /// Offre facile accesso al frame radice dell'applicazione Windows Phone.
        /// </summary>
        /// <returns>Nome radice dell'applicazione Windows Phone.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Costruttore dell'oggetto Application.
        /// </summary>
        public App()
        {
            // Gestore globale delle eccezioni non rilevate. 
            UnhandledException += Application_UnhandledException;

            // Inizializzazione Silverlight standard
            InitializeComponent();

            // Inizializzazione specifica del telefono
            InitializePhoneApplication();


            _httpreqhandler.Start();
            ViewModel.SetDispatcher(RootVisual.Dispatcher);


            // Visualizza informazioni di profilatura delle immagini durante il debug.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Visualizza i contatori della frequenza fotogrammi corrente
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Visualizza le aree dell'applicazione che vengono ridisegnate in ogni fotogramma.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Abilitare la modalità di visualizzazione dell'analisi non di produzione, 
                // che consente di visualizzare le aree di una pagina passate alla GPU con una sovrapposizione colorata.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Disabilitare il rilevamento dell'inattività dell'applicazione impostando la proprietà UserIdleDetectionMode
                // dell'oggetto PhoneApplicationService dell'applicazione su Disabled.
                // Attenzione: utilizzare questa opzione solo in modalità di debug. L'applicazione che disabilita il rilevamento dell'inattività dell'utente continuerà ad essere eseguita
                // e a consumare energia quando l'utente non utilizza il telefono.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }
        }

        // Codice da eseguire all'avvio dell'applicazione (ad esempio da Start)
        // Questo codice non verrà eseguito quando l'applicazione viene riattivata
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
        }

        // Codice da eseguire quando l'applicazione viene attivata (portata in primo piano)
        // Questo codice non verrà eseguito al primo avvio dell'applicazione
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            // Verificare che lo stato dell'applicazione sia ripristinato in modo appropriato
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }

        // Codice da eseguire quando l'applicazione viene disattivata (inviata in background)
        // Questo codice non verrà eseguito alla chiusura dell'applicazione
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
        }

        // Codice da eseguire alla chiusura dell'applicazione (ad esempio se l'utente fa clic su Indietro)
        // Questo codice non verrà eseguito quando l'applicazione viene disattivata
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            // Verificare che lo stato dell'applicazione richiesto sia persistente qui.
        }

        // Codice da eseguire se un'operazione di navigazione ha esito negativo
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Un'operazione di navigazione ha avuto esito negativo; inserire un'interruzione nel debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Codice da eseguire in caso di eccezioni non gestite
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Si è verificata un'eccezione non gestita; inserire un'interruzione nel debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Inizializzazione dell'applicazione Windows Phone

        // Evitare la doppia inizializzazione
        private bool phoneApplicationInitialized = false;

        // Non aggiungere altro codice a questo metodo
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Creare il fotogramma ma non impostarlo ancora come RootVisual; in questo modo
            // la schermata iniziale rimane attiva finché non viene completata la preparazione al rendering dell'applicazione.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Gestisce gli errori di navigazione
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Accertarsi che l'inizializzazione non venga ripetuta
            phoneApplicationInitialized = true;
        }

        // Non aggiungere altro codice a questo metodo
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Impostare l'elemento visivo radice per consentire il rendering dell'applicazione
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Rimuovere il gestore in quanto non più necessario
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion








        #region Notifications




        public static void SetupPushChannel()
        {

            // The name of our push channel.
            string channelName = "HGPushChannel";

            // Try to find the push channel.
            pushChannel = HttpNotificationChannel.Find(channelName);

            // If the channel was not found, then create a new connection to the push service.
            if (pushChannel == null)
            {
                pushChannel = new HttpNotificationChannel(channelName);
                // Register for all the events before attempting to open the channel.
                pushChannel.ChannelUriUpdated += new EventHandler<NotificationChannelUriEventArgs>(PushChannel_ChannelUriUpdated);
                pushChannel.ErrorOccurred += new EventHandler<NotificationChannelErrorEventArgs>(PushChannel_ErrorOccurred);
                pushChannel.ShellToastNotificationReceived += new EventHandler<NotificationEventArgs>(pushChannel_ShellToastNotificationReceived);
                pushChannel.Open();
            }
            else
            {
                // The channel was already open, so just register for all the events.
                pushChannel.ChannelUriUpdated += new EventHandler<NotificationChannelUriEventArgs>(PushChannel_ChannelUriUpdated);
                pushChannel.ErrorOccurred += new EventHandler<NotificationChannelErrorEventArgs>(PushChannel_ErrorOccurred);
                pushChannel.ShellToastNotificationReceived += new EventHandler<NotificationEventArgs>(pushChannel_ShellToastNotificationReceived);
            }

            // Bind this new channel for events.
            if (!pushChannel.IsShellTileBound)
                pushChannel.BindToShellTile();
            if (!pushChannel.IsShellToastBound)
                pushChannel.BindToShellToast();

        }

        public static void RemovePushChannel()
        {
            if (pushChannel != null)
            {
                try
                {
                    pushChannel.Close();
                } catch { }
                pushChannel = null;
            }
        }

        public static void SubscribePushNotifications()
        {
            if (pushChannel != null && pushChannel.ChannelUri != null)
            {
                SubscribePushNotifications(pushChannel.ChannelUri.ToString());
            }
        }

        public static void UnsubscribePushNotifications()
        {

            try
            {
                byte[] devid = (byte[])DeviceExtendedProperties.GetValue("DeviceUniqueId");
                // query homegenie service to get new events data
                string url = "http://" + (string)IsolatedStorageSettings.ApplicationSettings["RemoteServerAddress"];
                WebClient wc = new WebClient();
                if (IsolatedStorageSettings.ApplicationSettings.Contains("RemoteServerUsername") &&
                    (string)IsolatedStorageSettings.ApplicationSettings["RemoteServerUsername"] != "" &&
                    IsolatedStorageSettings.ApplicationSettings.Contains("RemoteServerPassword") &&
                    (string)IsolatedStorageSettings.ApplicationSettings["RemoteServerPassword"] != "")
                {
                    wc.Credentials = new NetworkCredential((string)IsolatedStorageSettings.ApplicationSettings["RemoteServerUsername"], (string)IsolatedStorageSettings.ApplicationSettings["RemoteServerPassword"]);
                }
                wc.DownloadStringAsync(new Uri(url + "/api/HomeAutomaion.HomeGenie/Messaging/WindowsPhone.RegisterNotificationUrl/" + Convert.ToBase64String(devid, 0, devid.Length) + "/"));
            }
            catch (Exception ex) { }
        }

        private static void SubscribePushNotifications(string notificationurl)
        {

            try
            {
                byte[] devid = (byte[])DeviceExtendedProperties.GetValue("DeviceUniqueId");
                // query homegenie service to get new events data
                string url = "http://" + (string)IsolatedStorageSettings.ApplicationSettings["RemoteServerAddress"];
                WebClient wc = new WebClient();
                if (IsolatedStorageSettings.ApplicationSettings.Contains("RemoteServerUsername") &&
                    (string)IsolatedStorageSettings.ApplicationSettings["RemoteServerUsername"] != "" &&
                    IsolatedStorageSettings.ApplicationSettings.Contains("RemoteServerPassword") &&
                    (string)IsolatedStorageSettings.ApplicationSettings["RemoteServerPassword"] != "")
                {
                    wc.Credentials = new NetworkCredential((string)IsolatedStorageSettings.ApplicationSettings["RemoteServerUsername"], (string)IsolatedStorageSettings.ApplicationSettings["RemoteServerPassword"]);
                }
                wc.DownloadStringAsync(new Uri(url + "/api/HomeAutomaion.HomeGenie/Messaging/WindowsPhone.RegisterNotificationUrl/" + Convert.ToBase64String(devid, 0, devid.Length) + "/" + Uri.EscapeUriString(notificationurl)));
                //wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wc_DownloadStringCompleted);

            }
            catch (Exception ex) { }
        }

        static void pushChannel_ShellToastNotificationReceived(object sender, NotificationEventArgs e)
        {
            string relativeUri = string.Empty;
            string title = "";
            string text = "";

            // Parse out the information that was part of the message.
            foreach (string key in e.Collection.Keys)
            {

                if (string.Compare(
                    key,
                    "wp:Param",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.CompareOptions.IgnoreCase) == 0)
                {
                    relativeUri = e.Collection[key];
                }
                else if (string.Compare(
                    key,
                    "wp:Text1",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.CompareOptions.IgnoreCase) == 0)
                {
                    title = e.Collection[key];
                }
                else if (string.Compare(
                    key,
                    "wp:Text2",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.CompareOptions.IgnoreCase) == 0)
                {
                    text = e.Collection[key];
                }

            }

            ViewModel.Dispatcher.BeginInvoke(() =>
            {
                ToastPrompt toast = new ToastPrompt();
                //toast.Completed += toast_Completed;
                toast.Title = title;
                toast.Message = text;
                //toast.ImageSource = new BitmapImage(new Uri("Assets/toasticon.png", UriKind.RelativeOrAbsolute));
                toast.Show();
            });

        }

        static void PushChannel_ErrorOccurred(object sender, NotificationChannelErrorEventArgs e)
        {
            // Error handling logic for your particular application would be here.
            ViewModel.Dispatcher.BeginInvoke(() =>
                MessageBox.Show(String.Format("A push notification {0} error occurred.  {1} ({2}) {3}",
                    e.ErrorType, e.Message, e.ErrorCode, e.ErrorAdditionalData))
                    );
        }

        static void PushChannel_ChannelUriUpdated(object sender, NotificationChannelUriEventArgs e)
        {

            SubscribePushNotifications(e.ChannelUri.ToString());

        }


        #endregion


    }
}
