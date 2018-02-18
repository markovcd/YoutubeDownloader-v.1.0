using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;

namespace YoutubeDownloader
{
    abstract class BaseViewModel : BindableBase, IDataErrorInfo
    {
        protected Notifier shortToastMessage;
        protected Notifier longToastMessage;

        private readonly Dictionary<string, Func<object, string>> _validationDictionary;

        public virtual string Error { get { return string.Empty; } }

        public abstract string this[string columnName]
        {
            get
            {
                if ()
                if (columnName == nameof(YoutubeUrl)) return ValidateYoutubeUrl(YoutubeUrl);
                return string.Empty;
            }
        }

        protected BaseViewModel()
        {
            SetToastMessages();
            SetLongToastMessages();

            _validationDictionary = new Dictionary<string, Func<object, string>>();
        }

        protected abstract void AddValidationMapping(string propertyName, Func<object, string> validationFunc);

        private void SetToastMessages()
        {
            shortToastMessage = new Notifier(cfg =>
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

    }
}
