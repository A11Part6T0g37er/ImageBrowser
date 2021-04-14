using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageBrowser.Models
{
    public class GroupInfoList<T> : List<T>
    {
        public T Key { get; set; }
    }
}
