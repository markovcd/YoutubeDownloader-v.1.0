using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using ToastNotifications.Messages;

namespace YoutubeDownloader
{
    abstract class BaseViewModel : BindableBase, IDataErrorInfo
    {
        private readonly Notifier _shortToastMessage;
        private readonly Notifier _longToastMessage;
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
            _shortToastMessage = SetToastMessages(3, 5);
            _longToastMessage = SetToastMessages(8, 8);

            _validationDictionary = new Dictionary<string, Func<string>>();

        }

        protected void AddValidationMapping(string propertyName, Func<string> validationFunc)
        {
            _validationDictionary.Add(propertyName, validationFunc);
        }

        protected void ShowInformation(string message, bool isShortMessage = false)
        {
            DispatchService.Invoke(() => (isShortMessage ? _shortToastMessage : _longToastMessage).ShowInformation(message));
        }

        protected void ShowSuccess(string message, bool isShortMessage = false)
        {
            DispatchService.Invoke(() => (isShortMessage ? _shortToastMessage : _longToastMessage).ShowSuccess(message));
        }

        protected void ShowWarning(string message, bool isShortMessage = false)
        {
            DispatchService.Invoke(() => (isShortMessage ? _shortToastMessage : _longToastMessage).ShowWarning(message));
        }

        protected void ShowError(string message, bool isShortMessage = false)
        {
            DispatchService.Invoke(() => (isShortMessage ? _shortToastMessage : _longToastMessage).ShowError(message));
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
