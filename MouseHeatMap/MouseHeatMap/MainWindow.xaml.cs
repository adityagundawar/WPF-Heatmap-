using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Documents.Serialization;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Drawing;

namespace MouseHeatMap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        double[,] heatMap;
        System.Windows.Shapes.Rectangle rect;
        BitmapImage finalHeatMap;


        public MainWindow()
        {
            InitializeComponent();
        }
        
        public void onStartUp(Canvas canvas)
        {
            rect = new System.Windows.Shapes.Rectangle();
            rect.Name = "canvasRect";
            rect.Width = canvas.ActualWidth / 2;
            rect.Height = canvas.ActualHeight / 2;
            rect.Stroke = System.Windows.Media.Brushes.Black;
            rect.Fill = System.Windows.Media.Brushes.SkyBlue;
            canvas.Children.Add(rect);
            double left = (canvas.ActualWidth - rect.Width) / 2;
            Canvas.SetLeft(rect, left);
            double top = (canvas.ActualHeight - rect.Height) / 2;
            Canvas.SetTop(rect, top);        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            heatMap = new double[(int)rect.Width, (int)rect.Height];
            myCanvas.MouseMove += new MouseEventHandler(updateCounter);
            finalHeatMap = new BitmapImage();
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
        }

        protected void updateCounter(object sender, MouseEventArgs e)
        {
            System.Windows.Point p = e.GetPosition(myCanvas);
            //p = PointToScreen(p);
            //Console.WriteLine(p);
            double left = (myCanvas.ActualWidth - rect.Width) / 2;
            double top = (myCanvas.ActualHeight - rect.Height) / 2;
            if (p.X > left && p.X < left + rect.Width && p.Y > top && p.Y < top + rect.Height)
            {
                p.X = (int)(p.X - left);
                p.Y = (int)(p.Y - top);
                //Console.WriteLine(p);
                //plotHeatMap(p);
                plotHeatMap2(p);
            }
        }

        /**public void plotHeatMap(Point p)
        {
            int i, j, i_end, j_end, j_start;
            if ((int)(p.X-50)<0)
            {
                i = 0;
            }
            else
            {
                i = (int)(p.X - 50);
            }
            if ((int)(p.Y - 50) < 0)
            {
                j = 0;
                j_start = 0;
            }
            else
            {
                j = (int)(p.Y - 50);
                j_start = (int)(p.Y - 50);
            }
            if ((int)(p.X + 50) > rect.Width)
            {
                i_end = (int)rect.Width;
            }
            else
            {
                i_end = (int)(p.X + 50);
            }
            if ((int)(p.Y + 50) > rect.Height)
            {
                j_end = (int)rect.Height;
            }
            else
            {
                j_end = (int)(p.Y + 50);
            }

            while (i < i_end)
            {
                while (j < j_end)
                {
                    heatMap[i, j] += 2 * Math.Exp(-(((i - p.X) * (i - p.X) / 2)+ ((j - p.Y) * (j - p.Y) / 2)));
                    j++;
                }
                j = j_start;
                i++;
            }
        }**/

        protected void plotHeatMap2(System.Windows.Point p)
        {
            for(int i = Math.Max(0, (int)(p.X - 50)); i < Math.Min(rect.Width-1,(int)(p.X + 50)); i++)
            {
                for(int j = Math.Max(0, (int)(p.Y - 50)); j < Math.Min(rect.Height-1, (int)(p.Y + 50)); j++)
                {
                    heatMap[i, j] += 3* Math.Exp(-(((i - p.X) * (i - p.X) / 2) + ((j - p.Y) * (j - p.Y) / 2)));
                }
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            myCanvas.MouseMove -= updateCounter;
            StopButton.IsEnabled = false;
            StartButton.IsEnabled = true;
            //Console.WriteLine(heatMap);
            //Console.WriteLine(heatMap.GetLength(0) + "  " + rect.Width);
            //Console.WriteLine(heatMap.GetLength(1) + "  " + rect.Height);
            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            using (StreamWriter outputFile = new StreamWriter(System.IO.Path.Combine(docPath, "heatmap.csv")))
            {
                for(int i = 0; i < heatMap.GetLength(1); i++)
                {
                    for(int j = 0; j < heatMap.GetLength(0); j++)
                    {
                        //outputFile.WriteLine(heatMap[i,j]+",");
                        outputFile.Write(heatMap[j,i] + ",");
                    }
                    outputFile.Write('\n');
                }
                    
            }
            double max = heatMap.Cast<double>().Max();
            int[,] integers = new int[(int)rect.Width, (int)rect.Height];
            for (int i = 0; i < heatMap.GetLength(1); i++)
            {
                for (int j = 0; j < heatMap.GetLength(0); j++)
                {
                    byte[] bgra;
                    if(heatMap[j,i]<max && heatMap[j, i] >= 4 * max / 5)
                    {
                        bgra = new byte[] { (byte)(255), (byte)(0), (byte)(0), 255 };
                    }
                    else if (heatMap[j, i] < 4 * max / 5 && heatMap[j, i] >= 3 * max / 5)
                    {
                        bgra = new byte[] { (byte)(255), (byte)(165), (byte)(0), 255 };
                    }
                    else if (heatMap[j, i] < 3 * max / 5 && heatMap[j, i] >= 2 * max / 5)
                    {
                        bgra = new byte[] { (byte)(255), (byte)(255), (byte)(0), 255 };
                    }
                    else if (heatMap[j, i] < 2 * max / 5 && heatMap[j, i] >= max / 5)
                    {
                        bgra = new byte[] { (byte)(0), (byte)(255), (byte)(0), 255 };
                    }
                    else
                    {
                        bgra = new byte[] { (byte)(0), (byte)(0), (byte)(255), 255 };
                    }
                    integers[j,i] = BitConverter.ToInt32(bgra, 0);
                }
            }
            Bitmap bitmap;
            unsafe
            {
                fixed (int* intPtr = &integers[0, 0])
                {
                    bitmap = new Bitmap(heatMap.GetLength(0), heatMap.GetLength(1), heatMap.GetLength(0)*4, System.Drawing.Imaging.PixelFormat.Format32bppRgb, new IntPtr(intPtr));
                    bitmap.Save("D:/Computer_Science/Prof_Alam_Project/heatmap.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                }
            }
        }
    }
}
