using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core;
 
namespace BackgroundTaskApp
{
    public sealed class MyBackgroundTask : IBackgroundTask
    {
        volatile bool _cancelRequested = false; // прервана ли задача
        BackgroundTaskDeferral _deferral;  // Note: defined at class scope so that we can mark it complete inside the OnCancel() callback if we choose to support cancellation
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            // оценка стоимости выполнения задачи для приложения
            var cost = BackgroundWorkCost.CurrentBackgroundWorkCost;
            if (cost == BackgroundWorkCostValue.High)
                return;

            // обрабатываем прерывание задачи
            var cancel = new CancellationTokenSource();
            taskInstance.Canceled += (s, e) =>
            {
                cancel.Cancel();
                cancel.Dispose();
                _cancelRequested = true;
            };
           
			 _deferral = taskInstance.GetDeferral();

            // await DoWork(taskInstance);
            Thread.Sleep(1500);
            _deferral.Complete();
            
        }
        private async Task DoWork(IBackgroundTaskInstance taskInstance)
        {       

            // получаем локальные настройки приложения

            var settings = ApplicationData.Current.LocalSettings;
            int number = (int)settings.Values["number"];
            long result = 1;
            for (uint progress = 1; progress <= number; progress++)
            {
                if (_cancelRequested) // если задача прервана, выходим из цикла
                {
                    break;
                }

                result *= progress;
                await Task.Delay(400); // имитация долгого выполнения
                                       // рассчет процентов выполнения
                taskInstance.Progress = (uint)(progress * 100 / number); // 1 * 100 / 6
            }

            settings.Values["factorial"] = result;
        }      
    }
}
