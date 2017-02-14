using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Threading;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using Emgu.CV.Cuda;
using Emgu.CV.Cvb;
using SerializeLibrary;

namespace UDE_MachineVision
{
    [Serializable]
    public class emguMatchPattern
    {
        /// <summary>
        /// pattern影像
        /// </summary>
        public Image<Bgr, Byte> Pattern;
        /// <summary>
        /// pattern搜尋角度範圍
        /// </summary>
        public double setAngle = 0.0;
        /// <summary>
        /// pattern計算壓縮次數
        /// </summary>
        public int reduce_level = 3;
        /// <summary>
        /// 建構子
        /// </summary>
        public emguMatchPattern()
        {
            
        }
    }
}
