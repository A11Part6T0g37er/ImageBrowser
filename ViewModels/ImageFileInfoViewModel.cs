﻿using ImageBrowser.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace ImageBrowser.ViewModels
{
    internal class ImageFileInfoViewModel 
    {
      
        private ObservableCollection<ImageFileInfo> observableCollection = new ObservableCollection<ImageFileInfo>() /*{ new ImageFileInfo("NotNull", null, null, "jpg") }*/;
       

        public ObservableCollection<ImageFileInfo> ObservableCollection { get => observableCollection; set => observableCollection=value; }
        public ImageFileInfoViewModel()
        {

        }
        public void ChangeObservCollection(ObservableCollection<ImageFileInfo> images)
        {
            observableCollection = images;
        }
        private ObservableCollection<GroupInfoList> groupedImagesInfos = new ObservableCollection<GroupInfoList>();
        public ObservableCollection<GroupInfoList> GroupedImagesInfos { get => groupedImagesInfos; set => groupedImagesInfos = value; }
      
        
        public void GenerateByDateGroup(ObservableCollection<ImageFileInfo> lisOfImages)
        {
            var query = from item in lisOfImages
                        group item by item.ImageProperties.DateTaken into dateKey
                        orderby dateKey.Key
                        select new { GroupName = dateKey.Key, Items = dateKey };
            if (GroupedImagesInfos.Count > 0)
            {
                GroupedImagesInfos.Clear(); 
            }
            foreach(var item in query)
            {
                GroupInfoList infoList = new GroupInfoList();
                infoList.Key = item.GroupName.Month + "/" + item.GroupName.Year + " (" + item.Items.Count() + ")";
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
