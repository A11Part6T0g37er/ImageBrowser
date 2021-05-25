using Windows.ApplicationModel.Background;
using System.Linq;
using System;

using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Notifications;

namespace ImageBrowser.Helpers
{
	public static class BackgroundTaskHelper
	{
		
		/// <summary>
		/// Register a background task with the specified taskEntryPoint, name, trigger, and condition (optional).
		/// </summary>
		/// <param name="taskEntryPoint">Task entry point for the background task.</param>
		/// <param name="taskName">A name for the background task.</param>
		/// <param name="trigger">The trigger for the background task.</param>
		/// <param name="condition">Optional parameter. A conditional event that must be true for the task to fire.</param>
		/// <returns><see cref="BackgroundTaskRegistration"/> </returns>
		public static async Task<BackgroundTaskRegistration> RegisterBackgroundTaskAsync(string taskEntryPoint,
																		string taskName,
																		IBackgroundTrigger trigger ,
																		IBackgroundCondition condition = null)
		{

			// Check for existing registrations of this background task.
			foreach (var cur in BackgroundTaskRegistration.AllTasks.Where(cur => cur.Value.Name == taskName))
			{
				cur.Value.Unregister(true);
			//	return (BackgroundTaskRegistration)(cur.Value);
			}
			
			BackgroundTaskRegistration task = await RegisterTaskBuilderAsync(taskEntryPoint, taskName, trigger, condition);

			return task;
		}

		private static async Task<BackgroundTaskRegistration> RegisterTaskBuilderAsync(string taskEntryPoint, string taskName, IBackgroundTrigger trigger, IBackgroundCondition condition)
		{
			var builder = new BackgroundTaskBuilder
			{
				Name = taskName,
				TaskEntryPoint = taskEntryPoint
			};
			
			builder.SetTrigger(trigger);

			if (condition != null)
			{

				builder.AddCondition(condition);
			}

			builder.AddCondition(new SystemCondition(SystemConditionType.BackgroundWorkCostNotHigh));
			
			await BackgroundExecutionManager.RequestAccessAsync();
			BackgroundTaskRegistration task = builder.Register();
			task.Completed += Task_Completed;
			return task;
		}
		private async static void Task_Completed(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
		{
			await CallUIThreadHelper.CallOnUiThreadAsync(() => new ToastContentBuilder().AddArgument("action", "viewConversation")
	.AddArgument("conversationId", 9813)
	.AddText("You have no internet!")
	.AddText("App may not operate normally.")
	.Show());
		}
	}
}
