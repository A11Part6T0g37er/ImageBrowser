using ImageBrowser.Helpers;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections;
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

         

          //  signingOneDrive.IsEnabled = false;

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
				SystemTrigger internet = new SystemTrigger(SystemTriggerType.NetworkStateChange, false);
				SystemCondition conditionNOInternet = new SystemCondition(SystemConditionType.InternetNotAvailable);

			string taskEntryPoint = typeof(BackgroundTaskApp.MyBackgroundTask).ToString();
			var taska = await BackgroundTaskHelper.RegisterBackgroundTaskAsync(taskEntryPoint, taskName, internet, conditionNOInternet);
			
			//taska.Completed += Taska_Completed;
			/*ApplicationData.Current.LocalSettings.Values["number"] = 6; // число для подсчета факториала
			var taskList = BackgroundTaskRegistration.AllTasks.Values;

			var task = taskList.FirstOrDefault(i => i.Name == taskName);
			task?.Unregister(true); // must to clear, Bg task lives throught life-cycle
			if (task == null)
			{
				var taskBuilder = new BackgroundTaskBuilder();
				taskBuilder.Name = taskName;
				taskBuilder.TaskEntryPoint = typeof(BackgroundTaskApp.MyBackgroundTask).ToString();

				//ApplicationTrigger appTrigger = new ApplicationTrigger();
				//taskBuilder.SetTrigger(appTrigger);
				taskBuilder.SetTrigger(internet);

				taskBuilder.AddCondition(conditionNOInternet);
				// taskBuilder.CancelOnConditionLoss = true;
				//TODO: nake it work after completed

				await BackgroundExecutionManager.RequestAccessAsync();
				task = taskBuilder.Register();

				task.Progress += Task_Progress;
				task.Completed += Task_Completed;

				//await appTrigger.RequestAsync();

				//get network connectivity
				var temp = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile();

			//	startButton.IsEnabled = false;
			//	stopButton.IsEnabled = true;
			}
	*/	}

		private async void Taska_Completed(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
		{
			await CallUIThreadHelper.CallOnUiThreadAsync(() => new ToastContentBuilder().AddArgument("action", "viewConversation")
	.AddArgument("conversationId", 9813)
	.AddText("You have nNONONOo internet!")
	.AddText("App may not operate normally.")
	.Show());
		}

		private async void StopButton_Click(object sender, RoutedEventArgs e)
		{
		//	Stop();
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
		//	Stop();

			await CallUIThreadHelper.CallOnUiThreadAsync(() => new ToastContentBuilder().AddArgument("action", "viewConversation")
	.AddArgument("conversationId", 9813)
	.AddText("You have no internet!")
	.AddText("App may not operate normally.")
	.Show());
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
			//	outputBlock.Text = progress;
			});
		}



	}
}