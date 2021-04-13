using ImageBrowser.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace ImageBrowser.ViewModels
{
    internal class ImageFileInfoViewModel 
    {
      
        private ObservableCollection<ImageFileInfo> observableCollection = new ObservableCollection<ImageFileInfo>();
       

        public ObservableCollection<ImageFileInfo> ObservableCollection { get => observableCollection; }
        public ImageFileInfoViewModel()
        {

        }
        public void ChangeObservCollection(ObservableCollection<ImageFileInfo> images)
        {
            observableCollection = images;
        }
        private ObservableCollection<GroupInfoList<object>> groupedImagesInfos = new ObservableCollection<GroupInfoList<object>>();
        public ObservableCollection<GroupInfoList<object>> GroupedImagesInfos { get => groupedImagesInfos;  }
      
        
        public void GenerateByDateGroup(ObservableCollection<ImageFileInfo> lisOfImages)
        {
            var query = from item in lisOfImages
                        group item by  new {yy = item.ImageProperties.DateTaken.Year, mm = item.ImageProperties.DateTaken.Month } into dateKey 
                        orderby dateKey.Key.yy descending
                        select new { GroupName = dateKey.Key, Items = dateKey };
            if (GroupedImagesInfos.Count > 0)
            {
                GroupedImagesInfos.Clear(); 
            }
            foreach(var item in query)
            {
                GroupInfoList<object> infoList = new GroupInfoList<object>();
                infoList.Key = item.GroupName.mm + "/" + item.GroupName.yy + " (" + item.Items.Count() + ")";
                foreach (var something in item.Items)
                {
                    infoList.Add(something);
                }
                GroupedImagesInfos.Add(infoList);
            }
        }

        internal void Initialize()
        {
            var data = ObservableCollection;

            GenerateByDateGroup(data);
        }
        
    }
}
