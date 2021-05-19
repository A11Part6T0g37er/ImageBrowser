using ImageBrowser.Helpers;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ImageBrowser
{
    /// <summary>
    /// Homepage of browser.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        string taskName = "factorial";

        public MainPage()
        {
            InitializeComponent();

            // Required for go to previous page without loosing its state
            NavigationCacheMode = NavigationCacheMode.Enabled;

            signingOneDrive.IsEnabled = false;

            Task.Run(async () => await RegisterTaskAsync().ConfigureAwait(true));

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            base.OnNavigatedTo(e);
        }

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            await RegisterTaskAsync();
        }

        private async Task RegisterTaskAsync()
        {
            ApplicationData.Current.LocalSettings.Values["number"] = 6; // число для подсчета факториала
            var taskList = BackgroundTaskRegistration.AllTasks.Values;


            var task = taskList.FirstOrDefault(i => i.Name == taskName);
            task?.Unregister(true); // must to clear, Bg task lives throught life-cycle
            if (task == null)
            {
                var taskBuilder = new BackgroundTaskBuilder();
                taskBuilder.Name = taskName;
                taskBuilder.TaskEntryPoint = typeof(BackgroundTaskApp.MyBackgroundTask).ToString();

                ApplicationTrigger appTrigger = new ApplicationTrigger();
                   taskBuilder.SetTrigger(appTrigger);
          //     taskBuilder.SetTrigger(new SystemTrigger(SystemTriggerType.NetworkStateChange, false));
                taskBuilder.AddCondition(new SystemCondition(SystemConditionType.InternetNotAvailable));
               //TODO: nake it work again

                task = taskBuilder.Register();

                task.Progress += Task_Progress;
                task.Completed += Task_Completed;

                await appTrigger.RequestAsync();
                await BackgroundExecutionManager.RequestAccessAsync();

                //get network connectivity
                var temp = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile();

                startButton.IsEnabled = false;
                stopButton.IsEnabled = true;
            }
        }

        private async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Stop();
            await CallUIThreadHelper.CallOnUiThreadAsync(() => signingOneDrive.IsEnabled = true).ConfigureAwait(true);
            new ToastContentBuilder()
    .AddArgument("action", "viewConversation")
    .AddArgument("conversationId", 9813)
    .AddText("You have clicked stop!")
    .AddText("Background task has been aborted.")
    .Show();
        }

        private async void Task_Completed(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {

            await CallUIThreadHelper.CallOnUiThreadAsync(() => signingOneDrive.IsEnabled = false).ConfigureAwait(true);
            var result = ApplicationData.Current.LocalSettings.Values["factorial"];
            var progress = $"Результат: {result}";
            UpdateUI(progress);
            Stop();


        }

        private async void Task_Progress(BackgroundTaskRegistration sender, BackgroundTaskProgressEventArgs args)
        {
            await CallUIThreadHelper.CallOnUiThreadAsync(() => signingOneDrive.IsEnabled = true).ConfigureAwait(true);

            var progress = $"Progress: {args.Progress} %";
            UpdateUI(progress);
        }

        private async void UpdateUI(string progress)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                outputBlock.Text = progress;
            });
        }

        private async void Stop()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                var taskList = BackgroundTaskRegistration.AllTasks.Values;
                var task = taskList.FirstOrDefault(i => i.Name == taskName);
                if (task != null)
                {
                    task.Unregister(true);

                    stopButton.IsEnabled = false;
                    startButton.IsEnabled = true;
                }
            });
        }

    }
}