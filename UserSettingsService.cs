using System.Threading.Tasks;
using Medgreat.Services;
using Moq;
using NUnit.Framework;
using Shared.Services.Notify;
using Shared.Services.UserSettings;
using Shared.Services.Web;

namespace UnitTests.Services
{
    [TestFixture]
    public class UserSettingsServiceTest
    {
        private ICache _cache;
        private IWebService _webService;
        private UserSettingsDto _dto;
        private IToastService _toastService;

        [SetUp]
        public void Initialize()
        {
            _cache = new StubCache();
            _webService = Mock.Of<IWebService>();
            _toastService = Mock.Of<IToastService>();
        }


        [Test(TestOf = typeof(UserSettingsService))]
        public async Task Get_SettingsWereNotSavedBefore_ReturnsTrueForVibroAndSounds()
        {
            var settings = await new UserSettingsService(_cache, _webService,_toastService).Get();

            Assert.True(settings.IsSoundsEnabled);
            Assert.True(settings.IsVibroEnabled);
        }


        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public async Task Save_AllSettingTrue_GetReturnsSameResult(bool vibroEnabled, bool soundsEnabled)
        {
            //arrange
            var userSettingsService = new UserSettingsService(_cache, _webService,_toastService);

            var settings = await userSettingsService.Get();
            settings.IsSoundsEnabled = soundsEnabled;
            settings.IsVibroEnabled = vibroEnabled;

            //act
            await userSettingsService.Save(settings);

            //assert
            var newSettings = await userSettingsService.Get();
            Assert.True(newSettings.IsVibroEnabled == vibroEnabled);
            Assert.True(newSettings.IsSoundsEnabled == soundsEnabled);
        }

        [Test(TestOf = typeof(UserSettingsService))]
        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public async Task Remove_SettingsWasChanged_GetReturnDefaultSettings(bool vibroEnabled, bool soundsEnabled)
        {
            //arrange
            var userSettingsService = new UserSettingsService(_cache, _webService,_toastService);

            var settings = await userSettingsService.Get();
            settings.IsSoundsEnabled = soundsEnabled;
            settings.IsVibroEnabled = vibroEnabled;

            //act
            await userSettingsService.Save(settings);
            await userSettingsService.Remove();

            //assert
            var newSettings = await userSettingsService.Get();
            Assert.True(newSettings.IsVibroEnabled);
            Assert.True(newSettings.IsSoundsEnabled);
        }

    }
}
