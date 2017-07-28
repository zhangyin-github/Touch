﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Windows.Foundation;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Touch.Models;
using Touch.ViewModels;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Hosting;
using System.Threading.Tasks;
using Windows.Media.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Touch.Views.Pages
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public sealed partial class StreetViewPage : Page
    {
        private readonly List<Point> _pathPoint = new List<Point>();

        private List<ImageViewModel> _test;

        private List<Point> _wayPoint = new List<Point>();

        private List<int> _insertWayNum = new List<int>();

        private List<List<int>> _clusteringResult;

        private bool _hasPath = false;
        /// <summary>
        /// 暂存要显示的路径点 
        /// </summary>
        private int _tmpNodeNum;
        /// <summary>
        /// 暂存要显示的游览点
        /// </summary>
        private int _tmpWayNum;

        private MediaPlayerElement _backgroungMusic;
        //
        public StreetViewPage()
        {
            InitializeComponent();
            // 进入全屏模式
            ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
            /*var localFolder = ApplicationData.Current.LocalFolder;
            Debug.WriteLine(localFolder.Path);
            var existingFile = localFolder.TryGetItemAsync("Test.html");
            if (existingFile == null)
            {
                Debug.WriteLine("null");
            }*/
            var uri = new Uri("ms-appx-web:///Web/Test.html");
            Webview1.Navigate(uri);
            BackButtonControl.OnBackButtonClicked += () =>
            {
                var rootFrame = Window.Current.Content as Frame;
                ApplicationView.GetForCurrentView().ExitFullScreenMode();
                rootFrame?.GoBack();
            };
            _backgroungMusic = new MediaPlayerElement()
            {
                Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Media/summer.mp3")),
                AutoPlay = true
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _test = e.Parameter as List<ImageViewModel>;
            var photoClustering = new PhotoClustering(_test);
            _wayPoint = photoClustering.GetPhotoClustering();
            _test = photoClustering.UpdateImageList();//去掉没有GPS的图片
            _clusteringResult = photoClustering.GetClusteringResult();
            _backgroungMusic.MediaPlayer.Play();//背景音乐播放
            DelayGetPath();//得出路径
        }

        private void DelayGetPath()
        {
            var delay = TimeSpan.FromSeconds(2);
            var delayTimer = ThreadPoolTimer.CreateTimer
            (async source =>
            {
                await Dispatcher.RunAsync(
                  CoreDispatcherPriority.High,
                   async () =>
                  {
                      string result = await Webview1.InvokeScriptAsync("eval", new string[] { "getNetCheck()" });
                      Debug.WriteLine(result);
                      if (result.Equals("Y"))
                          InvokeJsGetPath();//得出路径
                      else
                      {
                          Debug.WriteLine("network wrong!");
                      }
                  });
            }, delay);
        }

        private async void InvokeJsStart(string x, string y)
        {
            /*string[] script = { "panorama=new google.maps.StreetViewPanorama("+
            "document.getElementById('street-view'),"+
            "{position: { lat: "+x+", lng:"+y+"},"+
          "pov: { heading: 90, pitch: 0},"+
            "});" };*/
            string[] script =
            {
                "panorama.setPosition({lat:"
                + x + ",lng:" + y
                + "});" + "panorama.setVisible(true);"
            };
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await Webview1.InvokeScriptAsync("eval", new[] { "setIsGetPath()" });
                var result = await Webview1.InvokeScriptAsync("eval", script);
                //Debug.WriteLine(result);
            });
        }

        private async void InvokeJsEnd()
        {
            await Webview1.InvokeScriptAsync("eval", new[] { "streetShowEnd()" });
        }

        //嵌入移动
        private async void InvokeJsMove(string x, string y, string heading)
        {
            string[] script =
            {
                "setStreetViewPositon("+x
                +","+y
                +","+heading
                +")"
            };
            string result="";
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                result = await Webview1.InvokeScriptAsync("eval", script);
            });
        }

        //嵌入朝向
        private void InvokeJsHeading(int tmpNodeNum)
        {
            Debug.WriteLine("run");
            var delay = TimeSpan.FromSeconds(2);
            var delayTimer = ThreadPoolTimer.CreateTimer
            (async source =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    string[] insertMessage =
                    {
                        "insertMark(" +
                        _pathPoint[tmpNodeNum].X +
                        "," + _pathPoint[tmpNodeNum].Y +
                        "," + tmpNodeNum + ")"
                    };
                    var result = await Webview1.InvokeScriptAsync("eval", insertMessage);
                    /*string[] args = {"setMarkHeading()"};
                    result = await Webview1.InvokeScriptAsync("eval", args); //镜头转换，待改善
                    Debug.WriteLine("result" + result);*/
                });
            }, delay);
        }

        //得到路径插入中途点
        private async void InvokeJsGetPath()
        {
            var script = new string[1] ;
            if (_wayPoint.Count > 1)
            {
                for (var i = 1; i < _wayPoint.Count - 1; ++i)
                    script[0] += "addWayPoint(" + _wayPoint.ElementAt(i).X + ", " + _wayPoint.ElementAt(i).Y + ");";
                script[0] += "getPath(" + _wayPoint.ElementAt(0).X + "," + _wayPoint.ElementAt(0).Y + ","
                         + _wayPoint.ElementAt(_wayPoint.Count - 1).X + "," +
                         _wayPoint.ElementAt(_wayPoint.Count - 1).Y + ");";
                var result = await Webview1.InvokeScriptAsync("eval", script);
            }
            else
            {
                script[0] += "insertOneMark("+ _wayPoint[0].X
                    +","+_wayPoint[0].Y+
                    ")";
                var result = await Webview1.InvokeScriptAsync("eval", script);
                _pathPoint.Add(_wayPoint[0]);
            }
            //Debug.WriteLine("rerutn"+result);
        }

        //在路径中加入
        private void InsertWayPoint()
        {
            // Debug.WriteLine("insert Point");
            for (var i = 1; i < _wayPoint.Count - 1; ++i)
            {
                // Debug.WriteLine(_wayPoint.ElementAt(i));
                var x = _wayPoint[i].X * 1000;
                var y = _wayPoint[i].Y * 1000;
                var tmpx = _pathPoint[0].X * 1000;
                var tmpy = _pathPoint[0].Y * 1000;
                int numI = 0;
                var tmp = (tmpx - x) * (tmpx - x) + (tmpy - y) * (tmpy - y);
                for (var j = 1; j < _pathPoint.Count; ++j)
                {
                    tmpx = _pathPoint[j].X * 1000;
                    tmpy = _pathPoint[j].Y * 1000;
                    var newtmp = (tmpx - x) * (tmpx - x) + (tmpy - y) * (tmpy - y);
                    if (newtmp < tmp)
                    {
                        tmp = newtmp;
                        numI = j;
                    }
                }
                //Debug.WriteLine(numI);
                _pathPoint.Insert(numI, _wayPoint[i]);
                //_insertWayNum.Add(numI);
            }
            /* Debug.WriteLine("finish insert");
            for (int i = 0; i < _insertWayNum.Count; ++i)
                Debug.WriteLine(_insertWayNum.ElementAt(i));*/
        }

        //测试得到路径
        private void TestGetPath()
        {
            var completed = false;
            var delay = TimeSpan.FromSeconds(0.5);
            var delayTimer = ThreadPoolTimer.CreateTimer
                // ReSharper disable once ImplicitlyCapturedClosure
                (source => { completed = true; }, delay, async source =>
                {
                    await Dispatcher.RunAsync(
                        CoreDispatcherPriority.High,
                        async () =>
                        {
                            if (!completed) return;
                            string[] args = { "testIsGetPath()" };
                            var result = await Webview1.InvokeScriptAsync("eval", args);
                            if (result == "Y")
                            {
                                _hasPath = true;
                                await Webview1.InvokeScriptAsync("eval", new[] { "setIsGetPath()" });
                                var tmp = await Webview1.InvokeScriptAsync("eval", new[] { "getPathPoint()" });
                                var pathArray = tmp.Split('\n');
                                //Debug.WriteLine("path point length" + pathArray.Length);
                                HashSet<int> deletePoint = new HashSet<int>();//稀疏掉的数组
                                if (pathArray.Length > 80) //稀疏点
                                {
                                    double interval = pathArray.Length / ((pathArray.Length - 80 + 1) * 1.0);
                                    double cot = interval;
                                    while (cot < pathArray.Length)
                                    {
                                        deletePoint.Add((int)cot);
                                        cot += interval;
                                    }
                                    if (deletePoint.Contains(pathArray.Length)) deletePoint.Remove(pathArray.Length);
                                }
                                int cotNum = 0;
                                for (int i = 0; i < pathArray.Length; ++i)
                                {
                                    if (cotNum < deletePoint.Count && deletePoint.ElementAt(cotNum) == i)
                                    {
                                        cotNum++;
                                        continue;
                                    }
                                    if (pathArray[i].Length >= 3)
                                    {
                                        var pointArray = pathArray[i].Split(',');
                                        var lat = Convert.ToDouble(pointArray[0]);
                                        var lng = Convert.ToDouble(pointArray[1]);
                                        _pathPoint.Add(new Point(lat, lng));
                                    }
                                }
                                //Debug.WriteLine("path point length(after relax)" + _pathPoint.Count);
                                //for (int i = 0; i < _pathPoint.Count; ++i) Debug.WriteLine(_pathPoint.ElementAt(i));
                                //嵌入_waypoint点
                                InsertWayPoint();
                                for (int i = 0; i < _pathPoint.Count; ++i) Debug.WriteLine(i + " " + _pathPoint.ElementAt(i));
                                StartWalk();
                            }
                            else
                            {
                                TestGetPath();
                            }
                            // Timer completed.
                        });
                });
        }

        //测试点击label
        private void TestClick(int nodeNum, int wayNum)
        {
            var completed = false;
            var delay = TimeSpan.FromSeconds(0.5);
            var delayTimer = ThreadPoolTimer.CreateTimer
                // ReSharper disable once ImplicitlyCapturedClosure
                (source => { completed = true; }, delay, async source =>
                {
                    await Dispatcher.RunAsync(
                        CoreDispatcherPriority.High,
                        async () =>
                        {
                            if (!completed) return;
                            string[] args = { "getClick()" };
                            var result = await Webview1.InvokeScriptAsync("eval", args);
                            if (result == "click")
                            {
                                _tmpNodeNum = nodeNum; //保护现场
                                _tmpWayNum = wayNum; //保护现场
                                List<ImageViewModel> thisPointPhoto = new List<ImageViewModel>();//得出改点的图片
                                List<int> list = _clusteringResult.ElementAt(wayNum - 1);
                                Debug.WriteLine("click" + (wayNum - 1));
                                for (int i = 0; i < list.Count; ++i)
                                {
                                    thisPointPhoto.Add(_test[list.ElementAt(i)]);
                                }
                                
                                StreetGalleryControl.StreetImageListViewModel.AddImages(thisPointPhoto);
                                StreetGalleryControl.Shown = true;

                                //if (nodeNum<_pathPoint.Count)
                                //    ShowPath(nodeNum, wayNum);
                                //else InvokeJsEnd();//结束
                            }
                            else
                                TestClick(nodeNum, wayNum);
                            // Timer completed.
                        });
                });
        }

        //显示路径
        private void ShowPath(int nodeNum, int wayNum)
        {
            var completed = false;
            var delay = TimeSpan.FromSeconds(2);
            //string streetStatus="";
            var delayTimer = ThreadPoolTimer.CreateTimer
            ( source =>
            {
                //
                // TODO: Work
                //
                var x = _pathPoint.ElementAt(nodeNum).X.ToString(CultureInfo.CurrentCulture);
                var y = _pathPoint.ElementAt(nodeNum).Y.ToString(CultureInfo.CurrentCulture);
                //Debug.WriteLine("get point"+tmp);
                Debug.WriteLine(x);
                Debug.WriteLine(y);
                var pathpov = new PathPov(_pathPoint.ElementAt(nodeNum - 1), _pathPoint.ElementAt(nodeNum));
                var tmpheading = pathpov.GetHeading().ToString();
                 InvokeJsMove(x, y, tmpheading);


                /*await Dispatcher.RunAsync(
                CoreDispatcherPriority.High,
                async () =>
                {
                    string[] script =
                    {
                        "setStreetViewPositon("+x
                        +","+y
                        +","+tmpheading
                        +")"
                    };
                    streetStatus = await Webview1.InvokeScriptAsync("eval", script);
                    Debug.WriteLine("street:"+streetStatus);
                });*/

                completed = true;
            }, delay, async source =>
            {
                //
                // Update the UI thread by using the UI core dispatcher.
                //
                await Dispatcher.RunAsync(
                    CoreDispatcherPriority.High,
                    () =>
                    {
                        Debug.WriteLine(nodeNum);
                        if (!completed) return;
                        if (nodeNum == _pathPoint.Count - 1)
                        {
                            Debug.WriteLine("finish");
                            InvokeJsHeading(nodeNum);
                            TestClick(nodeNum + 1, wayNum + 1);
                        }
                        else if (wayNum < _wayPoint.Count - 1 && _wayPoint[wayNum] == _pathPoint[nodeNum])
                        {
                            Debug.WriteLine("touch here");
                            InvokeJsHeading(nodeNum);
                            TestClick(nodeNum + 1, wayNum + 1);
                        }
                        else
                        {
                            ShowPath(nodeNum + 1, wayNum);
                        }
                    });
            });
        }

        //开始行走
        private void StartWalk()
        {
            if (_pathPoint.Count == 0)
            {
                Debug.WriteLine("no path");
            }
            else
            {
                var x = _pathPoint.ElementAt(0).X.ToString(CultureInfo.CurrentCulture);
                var y = _pathPoint.ElementAt(0).Y.ToString(CultureInfo.CurrentCulture);
                InvokeJsStart(x, y);
                Debug.WriteLine("test run");
                InvokeJsHeading(0);
                var delay = TimeSpan.FromSeconds(2);
                var delayTimer = ThreadPoolTimer.CreateTimer
                (source =>
                {
                    if (_pathPoint.Count > 1)
                        TestClick(1, 1);
                    else
                    {
                        TestClick(1, 1);
                        Debug.WriteLine("can't move");
                    }
                }, delay);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!_hasPath)
            {
                TestGetPath();
                Debug.WriteLine("click button");
            }
            else
            {
                StartWalk();
            }
        }

        private void ShowEndButton()
        {
            if (_tmpNodeNum < _pathPoint.Count)
                ShowPath(_tmpNodeNum, _tmpWayNum);
            else
            {
                InvokeJsEnd();//结束
                _backgroungMusic.MediaPlayer.Pause();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            /*if (!_hasPath)
            {
                _hasPath = true;
                InvokeJsGetPath();
            }
            else
            {
                Debug.Write("already");
            }*/
            InvokeJsGetPath();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Run(async () =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    // 延迟两秒后把progress隐藏
                    await Task.Delay(3000);
                    ProgressRingGrid.Visibility = Visibility.Collapsed;
                });
            });
        }
    }
}