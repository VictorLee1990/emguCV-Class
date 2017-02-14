using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Cvb;
using Emgu.Util;
using Emgu.CV.Structure;

namespace UDE_MachineVision
{
    public class emguObject
    {
        #region 宣告參數
        //private 
        private CvBlobDetector eBlobs_Detector = new CvBlobDetector();
        private CvBlobs eBlobs = new CvBlobs();
        private Image<Bgr, Byte> source_bgr_Img;
        private Image<Gray, Byte> source_gray_Img;
        private Image<Gray, Byte> source_binary_Img;
        //public
        /// <summary>
        /// 每個團塊的外接方形尺寸座標(X,Y,W,H)
        /// </summary>
        public Rectangle[] BoundingBox;
        /// <summary>
        /// 每個團塊的座標資料,上下左右邊界及中心點
        /// </summary>
        public eBlob_data[] data;
        /// <summary>
        /// emgu團塊資料結構
        /// </summary>
        public struct eBlob_data
        {
            /// <summary>
            /// 最高點Y座標
            /// </summary>
            public int Top;
            /// <summary>
            /// 最低點Y座標
            /// </summary>
            public int Bottom;
            /// <summary>
            /// 最右點X座標
            /// </summary>
            public int Right;
            /// <summary>
            /// 最左點X座標
            /// </summary>
            public int Left;
            /// <summary>
            /// 中心點X座標
            /// </summary>
            public float CenterX;
            /// <summary>
            /// 中心點Y座標
            /// </summary>
            public float CenterY;
        }
        #endregion

        /// <summary>
        /// emguObject建構子,團塊分析工具
        /// </summary>
        /// <param name="source_Img">輸入被分析的影像</param>
        public emguObject(Image<Bgr, Byte> source_Img)
        {
            source_bgr_Img = new Image<Bgr, Byte>(source_Img.Bitmap);
            toGray();
        }
        /// <summary>
        /// 團塊分析,請輸入二值化閥值,團塊是否為白色,移除最小面積,移除最大面積
        /// </summary>
        /// <param name="threshold">閥值0~255,輸入-1為自動(Otsu)</param>
        /// <param name="isWhite">1=白色團塊,0=黑色團塊</param>
        /// <param name="remove_LessArea">移除低於多少的面積</param>
        /// <param name="remove_GreaterArea">移除高於多少的面積,0為不移除</param>
        public void Detect(int threshold, int isWhite, int remove_LessArea, int remove_GreaterArea)
        {
            toBinary(threshold);
            if (isWhite == 0) // 0 = 將影像黑白反轉,因為團塊分析工具主要是搜尋白色團塊
            {
                source_binary_Img = source_binary_Img.Not();
            }
            eBlobs_Detector.Detect(source_binary_Img, eBlobs); // CvBlobDetector團塊分析工具
            if (remove_GreaterArea == 0)
            {
                eBlobs.FilterByArea(remove_LessArea, source_binary_Img.Width * source_binary_Img.Height); // 面積過濾
            }
            else
            {
                eBlobs.FilterByArea(remove_LessArea, remove_GreaterArea); // 面積過濾
            }

            BoundingBox = new Rectangle[eBlobs.Count];
            data = new eBlob_data[eBlobs.Count];
            int i = 0;
            foreach (var blob_object in eBlobs)
            {
                CvBlob cvb = blob_object.Value;
                BoundingBox[i] = cvb.BoundingBox;
                i++;
            }
            toData();
        }
        private void toGray() // 建構子使用,將傳入的彩色影像轉成灰階
        {
            source_gray_Img = source_bgr_Img.Convert<Gray, Byte>();
            source_binary_Img = new Image<Gray, Byte>(source_gray_Img.Width, source_gray_Img.Height);
        }
        private void toBinary(int threshold) // 檢測使用,將傳入的灰階影像轉成黑白
        {
            if (threshold >= 0 && threshold <= 255)
            {
                CvInvoke.Threshold(source_gray_Img, source_binary_Img, threshold, 255, Emgu.CV.CvEnum.ThresholdType.Binary);
                source_gray_Img.Save("gray.bmp");
                source_binary_Img.Save("binary.bmp");
            }
            else
            {
                CvInvoke.Threshold(source_gray_Img, source_binary_Img, 0, 255, Emgu.CV.CvEnum.ThresholdType.Otsu);
                source_gray_Img.Save("gray.bmp");
                source_binary_Img.Save("binary.bmp");
            }
        }
        private void toInverse() // 檢測使用,影像反轉,黑 <=> 白
        {
            source_binary_Img = source_binary_Img.Not();
        }
        private void toData() // 檢測使用,將結果轉成eBlob_data結構
        {
            int length = data.Length;
            for (int i = 0; i < length; i++)
            {
                data[i].Top = BoundingBox[i].Y;
                data[i].Bottom = BoundingBox[i].Y + BoundingBox[i].Height;
                data[i].Left = BoundingBox[i].X;
                data[i].Right = BoundingBox[i].X + BoundingBox[i].Width;
                data[i].CenterX = BoundingBox[i].X + BoundingBox[i].Width / 2.0f;
                data[i].CenterY = BoundingBox[i].Y + BoundingBox[i].Height / 2.0f;
            }
        }
    }
}
