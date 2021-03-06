﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace ZumaLevelDrawer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ZumaEditor : Window
    {
        private struct PathPoint
        {
            public Point pos;
            public bool canHit;
            public int layer;
        }

        ResourceDictionary lang;
        private List<PathPoint> pathPoint = new List<PathPoint>();

        private bool canHit = true;
        private int layer = 0;

        private int precision = 10;

        public ZumaEditor()
        {
            List<ResourceDictionary> dictionaryList = new List<ResourceDictionary>();
            foreach (ResourceDictionary dictionary in Application.Current.Resources.MergedDictionaries)
            {
                dictionaryList.Add(dictionary);
            }

            string requestedCulture;
            if (System.Threading.Thread.CurrentThread.CurrentCulture.Name == "zh-CN")
                requestedCulture = @"Resources\Lang\zh-cn.xaml";
            else
                requestedCulture = @"Resources\Lang\en-us.xaml";
            ResourceDictionary resourceDictionary
                = dictionaryList.FirstOrDefault(d => d.Source.OriginalString.Equals(requestedCulture));

            Application.Current.Resources.MergedDictionaries.Remove(resourceDictionary);
            Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);

            lang = resourceDictionary;

            InitializeComponent();
        }

        private void SetPreviewButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = $"{lang["ImageFile"]}|*.jpg;*.png;*.gif";
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            EditorCanvas.Background = new ImageBrush(new BitmapImage(new Uri(dialog.FileName)));
        }

        private void AddPoint(PathPoint point)
        {
            pathPoint.Add(point);

            if (pathPoint.Count == 1)
            {
                var startPoint = new Ellipse
                {
                    Height = 10,
                    Width = 10,
                    Fill = Brushes.Orange,
                    Margin = new Thickness(point.pos.X, point.pos.Y, 0, 0),
                };
                EditorCanvas.Children.Add(startPoint);
                return;
            }

            var lastPoint = pathPoint[pathPoint.Count - 2].pos;
            var currentPoint = point;

            var connectLine = new Line
            {
                X1 = lastPoint.X,
                Y1 = lastPoint.Y,
                X2 = currentPoint.pos.X,
                Y2 = currentPoint.pos.Y,
                StrokeThickness = 5,
                Stroke = point.layer == 0 ?
                    (point.canHit ? Brushes.Green : Brushes.Red) : 
                    (point.canHit ? Brushes.DarkGreen : Brushes.DarkRed)
            };
            EditorCanvas.Children.Add(connectLine);
        }

        private void EditorCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            var currentMousePos = e.GetPosition(EditorCanvas);

            if (pathPoint.Count == 0)
            {
                AddPoint(new PathPoint
                {
                    pos = currentMousePos,
                    canHit = canHit,
                    layer = layer
                });

                return;
            }

            var lastPoint = pathPoint[pathPoint.Count - 1].pos;
            var offset = currentMousePos - lastPoint;

            if (offset.Length < precision)
                return;

            // 循环插入点
            for (var dist = 1; dist < offset.Length; dist += 1)
            {
                var currentPoint = lastPoint + offset / offset.Length * dist;
                AddPoint(new PathPoint
                {
                    pos = currentPoint,
                    canHit = canHit,
                    layer = layer
                });
            }
        }

        private void ClearPointButton_Click(object sender, RoutedEventArgs e)
        {
            EditorCanvas.Children.Clear();
            pathPoint.Clear();
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            if (pathPoint.Count == 0)
            {
                MessageBox.Show(lang["Waypoint"].ToString());
                return;
            }

            var dialog = new SaveFileDialog();
            dialog.Filter = $"{lang["TextFile"]}|*.txt";
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            var file = File.CreateText(dialog.FileName);

            // 开始点
            var startPoint = pathPoint[0];
            file.Write((float)startPoint.pos.X);
            file.Write(" ");
            file.Write((float)startPoint.pos.Y);
            file.Write(" ");
            file.Write(startPoint.canHit ? 0 : 1);
            file.Write(" ");
            file.Write(startPoint.layer);
            file.Write("\r");

            // 后续点
            for (var i = 1; i < pathPoint.Count; ++i)
            {
                var point = pathPoint[i];
                var offset = point.pos - startPoint.pos;

                file.Write((int)(offset.X * 100));
                file.Write(" ");
                file.Write((int)(offset.Y * 100));
                file.Write(" ");
                file.Write(point.canHit ? 0 : 1);
                file.Write(" ");
                file.Write(point.layer);
                file.Write("\r");

                startPoint = point;
            }
            MessageBox.Show(lang["SaveSuccessful"].ToString());

            file.Close();
        }
        private void GenerateButton2_Click(object sender, RoutedEventArgs e)
        {
            if (pathPoint.Count == 0)
            {
                MessageBox.Show("Waypoint not created");
                return;
            }

            var dialog = new SaveFileDialog();
            dialog.Filter = "Text file|*.txt";
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            var file = File.CreateText(dialog.FileName);

            // 开始点
            var startPoint = pathPoint[0];
            file.Write((float)startPoint.pos.X);
            file.Write(" ");
            file.Write((float)startPoint.pos.Y);
            file.Write(" ");
            file.Write(startPoint.canHit ? 0 : 1);
            file.Write(" ");
            file.Write(startPoint.layer);
            file.Write("\r");

            // 后续点
            for (var i = 1; i < pathPoint.Count; ++i)
            {
                var point = pathPoint[i];
                var offset = point.pos - startPoint.pos;

                file.Write((int)(offset.X * 100));
                file.Write(" ");
                file.Write((int)(offset.Y * 100));
                file.Write(" ");
                file.Write(point.canHit ? 0 : 1);
                file.Write(" ");
                file.Write(point.layer);
                file.Write("\r");

                startPoint = point;
            }

            MessageBox.Show("Saved successfully, press OK to generate .dat file");
            var levelGeneratorPath = Environment.CurrentDirectory+"\\ZumaLevelBuilder.exe";
            if (!File.Exists(levelGeneratorPath))
            {
                MessageBox.Show(lang["ExpectionToolNotFound"].ToString());
                return;
            }
            string name = dialog.FileName;
            name = name.Replace(".txt", "");
            var process = Process.Start(levelGeneratorPath, $"\"{name + ".txt"}\" \"{name}\"");
            if (!process.WaitForExit(10000) || process.ExitCode != 0)
            {
                process.Kill();
            }
            file.Close();
        }
        private void RefreshCurrentState()
        {
            if (canHit)
                CanHitLabel.Content = $"{lang["Yes"]}";
            else
                CanHitLabel.Content = $"{lang["No"]}";

            LayerLabel.Content = layer;
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
                canHit = false;

            if (e.Key == Key.Tab)
                layer = 1;

            RefreshCurrentState();
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
                canHit = true;
            if (e.Key == Key.Tab)
                layer = 0;

            RefreshCurrentState();
        }

        private void PrecisionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            e.Handled = int.TryParse(PrecisionTextBox.Text, out precision);
        }
    }
}
