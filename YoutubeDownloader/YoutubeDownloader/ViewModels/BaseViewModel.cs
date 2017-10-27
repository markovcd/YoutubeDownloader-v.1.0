using System;
using System.ComponentModel;
using System.Windows;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;

namespace YoutubeDownloader
{
    class BaseViewModel : INotifyPropertyChanged
    {
        protected Notifier notifier;
        protected Notifier longToastMessage;
        protected FileHelper fileHelper;

        public event PropertyChangedEventHandler PropertyChanged;

        public BaseViewModel()
        {
            SetToastMessages();
            SetLongToastMessages();
            CreateDirectoryIfNotExists();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetToastMessages()
        {
            notifier = new Notifier(cfg =>
            {
                cfg.PositionProvider = new WindowPositionProvider(
                    parentWindow: Application.Current.MainWindow,
                    corner: Corner.BottomRight,
                    offsetX: 10,
                    offsetY: 10);

                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    notificationLifetime: TimeSpan.FromSeconds(3),
                    maximumNotificationCount: MaximumNotificationCount.FromCount(5));

                cfg.Dispatcher = Application.Current.Dispatcher;
            });
        }

        private void SetLongToastMessages()
        {
            longToastMessage = new Notifier(cfg =>
            {
                cfg.PositionProvider = new WindowPositionProvider(
                    parentWindow: Application.Current.MainWindow,
                    corner: Corner.BottomRight,
                    offsetX: 10,
                    offsetY: 10);

                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    notificationLifetime: TimeSpan.FromSeconds(8),
                    maximumNotificationCount: MaximumNotificationCount.FromCount(8));

                cfg.Dispatcher = Application.Current.Dispatcher;
            });
        }

        private void CreateDirectoryIfNotExists()
        {
            fileHelper = new FileHelper();
            if (fileHelper != null)
            {
                fileHelper.CheckIfDirectoryExists();
            }
        }

        protected bool CanExecute()
        {
            // TODO: some logic
            return true;
        }
    }
}
