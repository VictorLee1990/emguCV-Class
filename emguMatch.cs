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

namespace UDE_MachineVision
{
    public class emguMatch
    {
        private double[] minV, maxV;
        private Point[] minP, maxP;
        private Point[] rotatePoint = new Point[4];
        private double[] V = new double[13];
        private double[] ang = new double[13];// 計算次數=13次
        private emguMatchPattern Pattern = new emguMatchPattern();
        /// <summary>
        /// pattern四個角點,依序是左上/右上/右下/左下
        /// </summary>
        public Point[] pPoint = new Point[4];
        /// <summary>
        /// pattern的旋轉角度
        /// </summary>
        public double patternAngle = 0.0;
        /// <summary>
        /// pattern的相關係數分數
        /// </summary>
        public double patternScore = 0.0;
        /// <summary>
        /// pattern中心點座標
        /// </summary>
        public Point patternCenter;
        /// <summary>
        /// 建構子emguMatch工具
        /// </summary>
        public emguMatch() { }
        /// <summary>
        /// 學習樣本
        /// </summary>
        /// <param name="Pattern">輸入一張樣本影像</param>
        public void LearnPattern(Image<Bgr, Byte> Pattern) { this.Pattern.Pattern = new Image<Bgr, byte>(Pattern.Bitmap); }
        /// <summary>
        /// 讀取樣本資料
        /// </summary>
        /// <param name="path">讀取樣本路徑</param>
        /// <returns>回傳讀取檔案是否成功</returns>
        public bool LoadPattern(string path)
        {
            bool isLoad = false;
            if (Path.GetExtension(path) == ".emh")
            {
                this.Pattern = SerializeLibrary.DeSerialize.BinaryDeserializeItem<emguMatchPattern>(path, ref isLoad);
                return isLoad;
            }
            else
            {
                return isLoad;
            }
        }
        /// <summary>
        /// 保存樣本資料
        /// </summary>
        /// <param name="path">保存至樣本路徑</param>
        /// <returns>回傳保存檔案是否成功</returns>
        public bool SavePattern(string path)
        {
            bool isSave = false;
            if (Path.GetExtension(path) == ".emh")
            {
                SerializeLibrary.Serialize.BinarySerializeItem<emguMatchPattern>(path, this.Pattern, ref isSave);
                return isSave;
            }
            else
            {
                return isSave;
            }
        }
        /// <summary>
        /// 設定搜尋角度範圍
        /// </summary>
        /// <param name="angle">搜尋角度範圍,EX： angle = 10 , range = -10 ~ 10</param>
        public void SetAngle(double angle)
        {//ang
            if (angle != 0)
            {
                this.Pattern.setAngle = angle;
                for (int i = 0; i < 13; i++)
                {
                    ang[i] = (-1.0) * angle + i * (2.0 * angle) / 12.0;
                }
            }
            else
            {
                this.Pattern.setAngle = 0.0;
            }
        }
        /// <summary>
        /// 設定壓縮等級,金字塔壓縮層數,level越高計算速度越快,相對的精度越低
        /// </summary>
        /// <param name="level">壓縮層數</param>
        public void SetLevel(int level) { this.Pattern.reduce_level = level; }
        /// <summary>
        /// 樣本比對功能
        /// </summary>
        /// <param name="srcImage">搜尋樣本的目標影像</param>
        public void Match(Image<Bgr, Byte> srcImage)
        {
            Image<Gray, Byte> src_gray_reduce_Image;
            Image<Gray, Byte>[] rotate_buffer_Image = new Image<Gray,byte>[13];
            Image<Gray, float> match_result;
            Gray black = new Gray(0);
            Image<Gray, Byte> sPattern;

            if (Pattern.setAngle == 0) // 不做角度搜尋
            {
                src_gray_reduce_Image = srcImage.Convert<Gray, Byte>();
                sPattern = this.Pattern.Pattern.Convert<Gray, Byte>();
                for (int i = 0; i < Pattern.reduce_level; i++)
                {
                    src_gray_reduce_Image = src_gray_reduce_Image.PyrDown(); // 高斯金字塔壓縮影像level次
                    sPattern = sPattern.PyrDown(); // 高斯金字塔壓縮樣本level次
                }
                rotate_buffer_Image[0] = src_gray_reduce_Image;
                match_result = rotate_buffer_Image[0].MatchTemplate(sPattern, Emgu.CV.CvEnum.TemplateMatchingType.CcorrNormed);
                match_result.MinMax(out minV, out maxV, out minP, out maxP);
                patternAngle = 0.0;
            }
            else // angle_range 搜尋樣本
            {
                if (srcImage.Width >= 1600) // 圖比較大的壓縮4次,比較小的壓縮3次
                {
                    src_gray_reduce_Image = srcImage.Convert<Gray, Byte>().PyrDown().PyrDown().PyrDown().PyrDown(); // 高斯金字塔壓縮影像4次
                    sPattern = this.Pattern.Pattern.Convert<Gray, Byte>().PyrDown().PyrDown().PyrDown().PyrDown();// 高斯金字塔壓縮樣本4次
                }
                else
                {
                    src_gray_reduce_Image = srcImage.Convert<Gray, Byte>().PyrDown().PyrDown().PyrDown(); // 高斯金字塔壓縮影像3次
                    sPattern = this.Pattern.Pattern.Convert<Gray, Byte>().PyrDown().PyrDown().PyrDown();// 高斯金字塔壓縮樣本3次
                }
                
                System.Threading.Tasks.Parallel.For(0, 13, index =>
                    {
                        rotate_buffer_Image[index] = src_gray_reduce_Image.Rotate(ang[index], black, true);
                        match_result = rotate_buffer_Image[index].MatchTemplate(sPattern, Emgu.CV.CvEnum.TemplateMatchingType.CcorrNormed);
                        match_result.MinMax(out minV, out maxV, out minP, out maxP);
                        V[index] = maxV[0];
                    }); // 平行for迴圈,旋轉13個角度做相關係數比對法;

                double[] Rough_Angle = Find_Rough_Angle(V, ang); // 尋找樣本角度的區間以及分割角度
                double[] V2 = new double[13];
                double[] ang2 = new double[13];
                src_gray_reduce_Image = srcImage.Convert<Gray, Byte>();
                sPattern = this.Pattern.Pattern.Convert<Gray, Byte>();
                for (int i = 0; i < Pattern.reduce_level; i++)
                {
                    src_gray_reduce_Image = src_gray_reduce_Image.PyrDown(); // 高斯金字塔壓縮影像level次
                    sPattern = sPattern.PyrDown(); // 高斯金字塔壓縮樣本level次
                }

                System.Threading.Tasks.Parallel.For(0, 13, index =>
                {
                    ang2[index] = Rough_Angle[0] + index * Rough_Angle[1];
                    rotate_buffer_Image[index] = src_gray_reduce_Image.Rotate(ang2[index], black, true);

                    match_result = rotate_buffer_Image[index].MatchTemplate(sPattern, Emgu.CV.CvEnum.TemplateMatchingType.CcorrNormed);
                    match_result.MinMax(out minV, out maxV, out minP, out maxP);
                    V2[index] = maxV[0];
                }); // 平行for迴圈,旋轉13個角度做相關係數比對法

                patternAngle = Find_Pattern_Angle(V2, ang2); // 用內插法算出樣本的角度
                rotate_buffer_Image[0] = src_gray_reduce_Image.Rotate(patternAngle, black, true); // 使用內插法的結果再旋轉一次
                match_result = rotate_buffer_Image[0].MatchTemplate(sPattern, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed); // 用旋轉過的影像再比對一次得到精確位置及分數
                match_result.MinMax(out minV, out maxV, out minP, out maxP);
            }

            #region 將pattern的四個角點記錄下來,反運算回原圖的座標
            rotatePoint[0] = maxP[0];
            rotatePoint[1] = new Point(maxP[0].X + sPattern.Width, maxP[0].Y);
            rotatePoint[2] = new Point(maxP[0].X + sPattern.Width, maxP[0].Y + sPattern.Height);
            rotatePoint[3] = new Point(maxP[0].X, maxP[0].Y + sPattern.Height);
            double[] bufferPoint = Find_Org_Location(rotatePoint[0], new Point(src_gray_reduce_Image.Width / 2, src_gray_reduce_Image.Height / 2), new Point(rotate_buffer_Image[0].Width / 2, rotate_buffer_Image[0].Height / 2), patternAngle);
            pPoint[0] = new Point((int)bufferPoint[0], (int)bufferPoint[1]);

            bufferPoint = Find_Org_Location(rotatePoint[1], new Point(src_gray_reduce_Image.Width / 2, src_gray_reduce_Image.Height / 2), new Point(rotate_buffer_Image[0].Width / 2, rotate_buffer_Image[0].Height / 2), patternAngle);
            pPoint[1] = new Point((int)bufferPoint[0], (int)bufferPoint[1]);

            bufferPoint = Find_Org_Location(rotatePoint[2], new Point(src_gray_reduce_Image.Width / 2, src_gray_reduce_Image.Height / 2), new Point(rotate_buffer_Image[0].Width / 2, rotate_buffer_Image[0].Height / 2), patternAngle);
            pPoint[2] = new Point((int)bufferPoint[0], (int)bufferPoint[1]);

            bufferPoint = Find_Org_Location(rotatePoint[3], new Point(src_gray_reduce_Image.Width / 2, src_gray_reduce_Image.Height / 2), new Point(rotate_buffer_Image[0].Width / 2, rotate_buffer_Image[0].Height / 2), patternAngle);
            pPoint[3] = new Point((int)bufferPoint[0], (int)bufferPoint[1]);
            #endregion

            patternScore = maxV[0]; // 最終相關係數分數
            patternCenter = new Point((pPoint[0].X + pPoint[1].X + pPoint[2].X + pPoint[3].X) / 4, (pPoint[0].Y + pPoint[1].Y + pPoint[2].Y + pPoint[3].Y) / 4); // 中心點座標
        }
        /// <summary>
        /// 使用者介面顯示Pattern位置的函數
        /// </summary>
        /// <param name="g">Graphics控制物件</param>
        /// <param name="Color">顯示框顏色</param>
        /// <param name="zoomX">縮放比例X</param>
        /// <param name="zoomY">縮放比例Y</param>
        public void DrawPattern(Graphics g, Pen Color, float zoomX, float zoomY)
        {
            g.DrawLine(Color, (float)pPoint[0].X * zoomX, (float)pPoint[0].Y * zoomY, (float)pPoint[1].X * zoomX, (float)pPoint[1].Y * zoomY);
            g.DrawLine(Color, (float)pPoint[1].X * zoomX, (float)pPoint[1].Y * zoomY, (float)pPoint[2].X * zoomX, (float)pPoint[2].Y * zoomY);
            g.DrawLine(Color, (float)pPoint[2].X * zoomX, (float)pPoint[2].Y * zoomY, (float)pPoint[3].X * zoomX, (float)pPoint[3].Y * zoomY);
            g.DrawLine(Color, (float)pPoint[3].X * zoomX, (float)pPoint[3].Y * zoomY, (float)pPoint[0].X * zoomX, (float)pPoint[0].Y * zoomY);
        }
        /// <summary>
        /// 內插法求角度
        /// </summary>
        /// <param name="Value">所有角度的分數</param>
        /// <param name="ang">所有角度</param>
        /// <returns>內插的結果</returns>
        private double Find_Pattern_Angle(double[] Value, double[] ang)
        {
            double Max = Value.Max();
            double Sec = double.MinValue;
            int iMax = 0, iSec = 0;
            for (int i = 0; i < Value.Length; i++) // 找出第一高分及第二高分
            {
                double n = Value[i];
                if (n == Max)
                {
                    iMax = i;

                    if (i < Value.Length - 1 && i > 0)
                    {
                        if (Value[i - 1] > Value[i + 1])
                        {
                            Sec = Value[i - 1];
                            iSec = i - 1;
                        }
                        else
                        {
                            Sec = Value[i + 1];
                            iSec = i + 1;
                        }
                    }
                    else
                    {
                        if (i + 1 == Value.Length)
                        {
                            Sec = Value[i - 1];
                            iSec = i - 1;
                        }
                        if (i == 0)
                        {
                            Sec = Value[i + 1];
                            iSec = i + 1;
                        }
                    }
                    break;
                }
            }

            double angle = 0.0;

            angle = ((1 - Sec) * ang[iMax] + (1 - Max) * ang[iSec]) / ((1 - Sec) + (1 - Max)); // 內插求角度

            return angle;
        }
        /// <summary>
        /// 找出大概的角度範圍
        /// </summary>
        /// <param name="Value">所有角度的分數</param>
        /// <param name="ang">所有角度-180~180</param>
        /// <returns>回傳陣列0=角度起始點,陣列1=角度間隔</returns>
        private double[] Find_Rough_Angle(double[] Value, double[] ang)
        {
            double[] range_Angle = new double[2];
            double Max = Value.Max();
            int count = Value.Length;
            for (int i = 0; i < count; i++)
            {
                double n = Value[i];
                if (n == Max)
                {
                    if (i == 0)
                    {
                        range_Angle[0] = ang[0];
                        range_Angle[1] = ((ang[count - 1] - ang[0]) / ((double)count - 1.0)) * 2.0 / ((double)count - 1.0) / 2.0;
                    }
                    else if (i == count - 1)
                    {
                        range_Angle[0] = ang[i - 1];
                        range_Angle[1] = ((ang[count - 1] - ang[0]) / ((double)count - 1.0)) * 2.0 / ((double)count - 1.0) / 2.0;
                    }
                    else
                    {
                        range_Angle[0] = ang[i - 1];
                        range_Angle[1] = ((ang[count - 1] - ang[0]) / ((double)count - 1.0)) * 2.0 / ((double)count - 1.0);
                    }
                }
            }
            return range_Angle;
        }
        /// <summary>
        /// 反運算原始座標
        /// </summary>
        /// <param name="p">旋轉後的點</param>
        /// <param name="center">原圖旋轉中心點</param>
        /// <param name="offsetP">旋轉後的圖旋轉中心點</param>
        /// <param name="ang">旋轉的角度</param>
        /// <returns>回傳座標點</returns>
        private double[] Find_Org_Location(Point p, Point center, Point offsetP, double ang)
        {
            double[] OrgP = new double[2];

            OrgP[0] = ((double)(p.X - offsetP.X) * Math.Cos(ang * Math.PI / 180) + (double)(p.Y - offsetP.Y) * Math.Sin(ang * Math.PI / 180) + center.X) * Math.Pow(2, Pattern.reduce_level);
            OrgP[1] = ((double)(p.X - offsetP.X) * Math.Sin(ang * Math.PI / 180) * (-1) + (double)(p.Y - offsetP.Y) * Math.Cos(ang * Math.PI / 180) + center.Y) * Math.Pow(2, Pattern.reduce_level);

            return OrgP;
        }
        /// <summary>
        /// 尋找第i個pattern位置及分數
        /// </summary>
        /// <param name="data">相關係數陣列</param>
        /// <param name="Pos">第i個pattern位置</param>
        /// <param name="score">第i個pattern分數</param>
        /// <param name="index">指定第i個pattern,由0開始</param>
        private void getPattern_position(float[, ,] data, out Point Pos, out float score, int index)
        {
            int y = data.GetLength(0);
            int x = data.GetLength(1);
            int xy = x * y;
            float[] data_1D = new float[xy];
            float[] data_1D_sort = new float[xy];
            Buffer.BlockCopy(data, 0, data_1D, 0, xy * sizeof(float));
            Buffer.BlockCopy(data, 0, data_1D_sort, 0, xy * sizeof(float));

            Array.Sort(data_1D_sort);
            score = data_1D_sort[xy - 1 - index];
            int pX = 0, pY = 0;
            int i = Array.IndexOf(data_1D, score);

            pY = i / x;
            pX = i % x;

            Pos = new Point(pX, pY);
        }
    }
}
