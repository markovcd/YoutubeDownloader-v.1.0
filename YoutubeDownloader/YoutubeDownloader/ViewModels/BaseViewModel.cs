using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;

namespace YoutubeDownloader
{
    abstract class BaseViewModel : BindableBase, IDataErrorInfo
    {
        protected readonly Notifier shortToastMessage;
        protected readonly Notifier longToastMessage;

        private readonly IDictionary<string, Func<string>> _validationDictionary;

        public virtual string Error { get { return string.Empty; } }

        public string this[string columnName]
        {
            get
            {
                return _validationDictionary.ContainsKey(columnName) ?
                           _validationDictionary[columnName]() : string.Empty;
            }
        }

        protected BaseViewModel()
        {
            shortToastMessage = SetToastMessages(3, 5);
            longToastMessage = SetToastMessages(8, 8);

            _validationDictionary = new Dictionary<string, Func<string>>();

        }

        protected void AddValidationMapping(string propertyName, Func<string> validationFunc)
        {
            _validationDictionary.Add(propertyName, validationFunc);
        }

        private Notifier SetToastMessages(int notificationLifetime, int maximumNotificationCount)
        {
            return new Notifier(cfg =>
            {
                cfg.PositionProvider = new WindowPositionProvider(
                    parentWindow: Application.Current.MainWindow,
                    corner: Corner.BottomRight,
                    offsetX: 10,
                    offsetY: 10);

                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    notificationLifetime: TimeSpan.FromSeconds(notificationLifetime),
                    maximumNotificationCount: MaximumNotificationCount.FromCount(maximumNotificationCount));

                cfg.Dispatcher = Application.Current.Dispatcher;
            });
        }

    }
}
