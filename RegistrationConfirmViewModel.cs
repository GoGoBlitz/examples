using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Medgreat.Services.Authentication;
using Polly;
using Shared;
using Shared.Constants;
using Shared.Extensions;
using Shared.Services.Analytics;
using Shared.Services.Notify;
using Xamarin.Forms;

namespace Medgreat.Pacient.Page.Registration.Confirm
{
    class RegistrationConfirmViewModel : INotifyPropertyChanged
    {
        private readonly IMedgreatApplication _medgreatApplication;
        private readonly IAuthenticationService _authenticationService;
        private readonly IToastService _toastNotificator;
        private readonly IAnalyticsService _analyticsService;

        public RegistrationConfirmViewModel(string phone, string token,
            IMedgreatApplication medgreatApplication,
            IAuthenticationService authenticationService,
            INavigation navigation,
            IToastService toastNotificator,
            IAnalyticsService analyticsService,
            Type redirectAfter = null)
        {
            Token = token;
            _medgreatApplication = medgreatApplication;
            _authenticationService = authenticationService;
            _toastNotificator = toastNotificator;
            _analyticsService = analyticsService;
            IsButtonEnabled = true;
            ButtonText = "ПОДТВЕРДИТЬ";
            IsCodeCanBeResended = true;

            ConfirmCommand = new Command(async () =>
            {
                _analyticsService.LogEvent("Экран подтверждения регистрации. Нажата кнопка <Подтвердить>");

                await ConfirmAction(navigation, redirectAfter);
            });

            ResendCodeCommand = new Command(async () =>
            {
                _analyticsService.LogEvent("Экран подтверждения регистрации. Нажата кнопка <Отправить повторно>");

                await new Func<Task>(async () =>
                {
                    await Policy.Handle<ModernClientHttpResponseException>(r => r.StatusCode != HttpStatusCode.Gone && r.StatusCode != HttpStatusCode.Unauthorized).Or<WebException>().Or<ModernClientHttpResponseException>()
                                   .WaitAndRetryAsync(retryCount: 4, sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
                                   .ExecuteAsync(async () => await _authenticationService.ResendCode(phone, Token));
                }).SaveExecuteHttpRequest(_toastNotificator, true, "Ошибка при отправке запроса, попробуйте позже");

                IsCodeCanBeResended = false;
            });
        }

        public ICommand ConfirmCommand { get; private set; }

        public ICommand ResendCodeCommand { get; private set; }

        private bool _isButtonEnabled;
        public bool IsButtonEnabled
        {
            get { return _isButtonEnabled; }
            set
            {
                if (value != _isButtonEnabled)
                {
                    _isButtonEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Token { get; set; }

        private string _buttonText;

        public string ButtonText
        {
            get { return _buttonText; }
            set
            {
                if (value != _buttonText)
                {
                    _buttonText = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Code { get; set; }

        private bool _isCodeCanBeResended;
        public bool IsCodeCanBeResended
        {
            get { return _isCodeCanBeResended; }
            set
            {
                if (value != _isCodeCanBeResended)
                {
                    _isCodeCanBeResended = value;
                    OnPropertyChanged();
                }
            }
        }


        private async Task ConfirmAction(INavigation navigation, Type redirectAfter)
        {
            if (string.IsNullOrEmpty(Code))
            {
                _analyticsService.LogEvent("Экран подтверждения регистрации. Неверный код подтверждения");

                Device.BeginInvokeOnMainThread(async () =>
                {
                    await _toastNotificator.Notify(ToastNotificationType.Info, "Ошибка", "Введите код подтверждения из смс", TimeSpan.FromSeconds(2));
                });

                return;
            }

            IsButtonEnabled = false;

            ButtonText = "подтверждение..";

            await Task.Run(async () =>
            {
                string result = null;

                bool success = await new Func<Task>(async () =>
                 {
                     result = await Policy.Handle<ModernClientHttpResponseException>(r => r.StatusCode != HttpStatusCode.Gone && r.StatusCode != HttpStatusCode.Unauthorized).Or<WebException>().Or<ModernClientHttpResponseException>()
                                     .WaitAndRetryAsync(retryCount: 4, sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
                                     .ExecuteAsync(async () => await _authenticationService.Confirm(Code, Token));
                 }).SaveExecuteHttpRequest(_toastNotificator, true, "Ошибка при попытке зарегистрироваться, попробуйте позже");

                if (!success)
                    return;


                if (!string.IsNullOrEmpty(result))
                {
                    await Task.Run(() =>
                    {
                        _medgreatApplication.ConfigureWebSockets(result);
                    });

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        _medgreatApplication.ChangeRootPage(new RootPage());
                    });

                    MessagingCenter.Send(SharedConstants.AuthorizedEvent, SharedConstants.AuthorizedEvent);
                }
                else
                {
                    _analyticsService.LogEvent("Сервер вернул ошибку <Код подтверждения регистрации неверен>");

                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await
                            _toastNotificator.Notify(ToastNotificationType.Info, "Внимание", "Код подтверждения неверен",
                                TimeSpan.FromSeconds(2));
                    });

                    ButtonText = "ПОДТВЕРДИТЬ";
                    IsButtonEnabled = true;
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
