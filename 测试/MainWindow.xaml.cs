using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HslCommunication;
using HslCommunication.Profinet.Omron;

namespace 测试
{

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private OmronFinsNet omronFinsNet = new OmronFinsNet("192.168.251.2", 9600);

        //读系统状态指示
        bool[] SystemStatus = new bool[4];
        //分柜使能数据更新标志位
        bool bzw1 = true;
        //分柜读取数据成功
        bool bzw2 = false;
        //分柜点位更新标志
        bool bzw3 = true;
        //系统参数更新标志位
        bool bzw4 = true;
        //读通信故障代码
        uint uint_D6;


        //停机上限温度
        uint TJSXWD;
        //严重对比温度
        uint YZDBWD;
        //报警偏差温度
        uint BJPCWD;
        //主副温度对比
        uint ZFWDDB;


        //读PLC状态代码
        uint uint_D8;



        //加热使能点表
        bool[] JRDB = new bool[368];
        //副加热点使能
        bool[] FJRDB = new bool[368];
        //工艺开关表
        bool[] GYKGB = new bool[368];
        //主温度表
        short[] ZWDB = new short[368];
        //主温度表
        short[] FZWDB = new short[368];
        //限温开关
        bool[] XWKGB = new bool[368];
        //加热状态表
        bool[] JRZTB = new bool[368];
        //保温状态表
        bool[] BWZTB = new bool[368];
        //PID输出表
        bool[] PIDSCB = new bool[368];
        //主传感器断线表
        bool[] ZCGQDXB = new bool[368];


        //读分柜使能表
        bool[] FGSNB = new bool[32];
        //读分柜通信
        bool[] FGSTX = new bool[32];
        //读分柜空开表
        bool[] FGKKB = new bool[32];
        //读分柜急停表
        bool[] FGJTB = new bool[32];
        //读分柜接触器表
        bool[] FGJCQB = new bool[32];
        //读分柜相序表
        bool[] FGXXB = new bool[32];
        //目标温度
        short[] JRDSDWS = new short[368];


        //要写入的分柜使能数组
        bool[] fgsn = {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true,
                            true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true};
        //写加热点使能
        bool[] jrdsns = {false, false, false, false, false, false, false, false,
                          false, false, false, false, false, false, false, false};
        //写副加热点使能
        bool[] fjrdsns = {false, false, false, false, false, false, false, false,
                          false, false, false, false, false, false, false, false};

        //写工艺开关表
        bool[] gykgs = {false, false, false, false, false, false, false, false,
                          false, false, false, false, false, false, false, false};

        //写加热点开关表
        bool[] jrkgds = {false, false, false, false, false, false, false, false,
                          false, false, false, false, false, false, false, false};

        //目标温度
        short MBWD;
        //升温时间
        short SWSJ;
        //保温时间
        short BWSJ;
        //加热段报警值
        short JRDBJZ;
        //写入曲线选择
        short XZQXH;
        //工艺号
        short GYH;
        //启动曲线
        short QDQXH;
        public MainWindow()
        {
            InitializeComponent();
            // SystemalarmRead();
            System.Timers.Timer t = new System.Timers.Timer(100);   //实例化Timer类，设置间隔时间为100毫秒；   
            t.Elapsed += new System.Timers.ElapsedEventHandler(UIgx); //到达时间的时候执行事件；   
            t.AutoReset = true;   //设置是执行一次（false）还是一直执行(true)；   
            t.Enabled = true;     //是否执行System.Timers.Timer.Elapsed事件； 
            //UIgx();
        }
        /// <summary>
        /// PLC连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlcConnect()
        {
            try
            {
                OperateResult connect = omronFinsNet.ConnectServer();
                if (connect.IsSuccess)
                {
                    MessageBox.Show("PLC连接成功！");
                    RwPLC();
                }
                else
                {
                    MessageBox.Show("PLC连接失败！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// 读写PLC
        /// </summary>
        private void RwPLC()
        {
            // SystemalarmRead();
            System.Timers.Timer t = new System.Timers.Timer(100);   //实例化Timer类，设置间隔时间为100毫秒；   
            t.Elapsed += new System.Timers.ElapsedEventHandler(ReadPlc); //到达时间的时候执行事件；   
            t.AutoReset = true;   //设置是执行一次（false）还是一直执行(true)；   
            t.Enabled = true;     //是否执行System.Timers.Timer.Elapsed事件； 


        }

        /// <summary>
        /// 读取PLC数据
        /// </summary>
        private void ReadPlc(object source, System.Timers.ElapsedEventArgs e)
        {
            SystemalarmRead();
            Enable();
            Cabinetstatus();
            Systemps();
        }
        /// <summary>
        /// 系统报警读取
        /// </summary>
        private void SystemalarmRead()
        {
            //运行指示
            SystemStatus[0] = omronFinsNet.ReadBool("w0.0").Content;
            //系统报警
            SystemStatus[1] = omronFinsNet.ReadBool("w0.1").Content;
            //通信故障
            SystemStatus[2] = omronFinsNet.ReadBool("w0.2").Content;
            //主急停
            SystemStatus[3] = omronFinsNet.ReadBool("w0.3").Content;

            //通信故障代码
            uint_D6 = omronFinsNet.ReadUInt32("D6").Content;
            //PLC状态代码
            uint_D8 = omronFinsNet.ReadUInt32("D8").Content;

        }
        /// <summary>
        /// 系统参数设置读取
        /// </summary>
        private void Systemps()
        {
            OperateResult<ushort[]> tjsxwd = omronFinsNet.ReadUInt16("D1", 5);
            {
                if (tjsxwd.IsSuccess)
                {
                    YZDBWD = tjsxwd.Content[0];
                    TJSXWD = tjsxwd.Content[1];
                    BJPCWD = tjsxwd.Content[2];
                    ZFWDDB = tjsxwd.Content[4];
                }
            }
            bzw2 = true;
        }
        /// <summary>
        /// 使能表读取
        /// </summary>
        private void Enable()
        {
            //分柜使能表
            OperateResult<bool[]> fgsn = omronFinsNet.ReadBool("h500", 32);
            {
                if (fgsn.IsSuccess)
                {
                    FGSNB = fgsn.Content;
                }
                else
                {
                    // 发生了异常
                }
            }
            //加热点使能
            OperateResult<bool[]> read = omronFinsNet.ReadBool("h1", 1024);
            {
                if (read.IsSuccess)
                {    //主加热点使能  
                    JRDB = read.Content.Skip(0).Take(368).ToArray();
                    //副加热点使能  
                    FJRDB = read.Content.Skip(512).Take(368).ToArray();
                }
                else
                {
                    // 发生了异常
                }
            }
            //读已经打开的工艺
            OperateResult<bool[]> gykgb = omronFinsNet.ReadBool("w10", 368);
            {
                if (gykgb.IsSuccess)
                {
                    GYKGB = gykgb.Content;
                }
                else
                {
                    // 发生了异常
                }
            }
        }
        /// <summary>
        /// 分柜状态读取
        /// 主副温度读取
        /// 限温开关读取
        /// pid输出
        /// 加热状态
        /// 保温状态
        /// 加热点设定温升
        /// </summary>
        private void Cabinetstatus()
        {
            //分柜状态
            OperateResult<bool[]> read = omronFinsNet.ReadBool("W500", 160);
            {
                if (read.IsSuccess)
                {    //分柜通信
                    FGSTX = read.Content.Skip(0).Take(32).ToArray();
                    //分柜空开  
                    FGKKB = read.Content.Skip(32).Take(32).ToArray();
                    //分柜急停  
                    FGJTB = read.Content.Skip(64).Take(32).ToArray();
                    //分柜接触器
                    FGJCQB = read.Content.Skip(96).Take(32).ToArray();
                    //分柜相序
                    FGJCQB = read.Content.Skip(127).Take(32).ToArray();
                }
                else
                {
                    // 发生了异常
                }
            }
            //主副温度
            OperateResult<short[]> zwdb = omronFinsNet.ReadInt16("D10772", 736);
            {
                if (zwdb.IsSuccess)
                {
                    ZWDB = zwdb.Content.Skip(0).Take(368).ToArray();
                    FZWDB = zwdb.Content.Skip(368).Take(368).ToArray();
                }
                else
                {
                    // 发生了异常
                }
            }
            //加热点设定温升
            OperateResult<short[]> Jresdws = omronFinsNet.ReadInt16("D11876", 368);
            {
                if (Jresdws.IsSuccess)
                {
                    JRDSDWS = Jresdws.Content;
                }
                else
                {
                    // 发生了异常
                }
            }
            //限温开关
            OperateResult<bool[]> Readxwkg = omronFinsNet.ReadBool("W362", 368);
            {
                if (Readxwkg.IsSuccess)
                {
                    XWKGB = Readxwkg.Content;
                }
                else
                {
                    // 发生了异常
                }
            }
            //pid输出表
            OperateResult<bool[]> Pidsc = omronFinsNet.ReadBool("W106", 368);
            {
                if (Pidsc.IsSuccess)
                {
                    PIDSCB = Pidsc.Content;
                }
                else
                {
                    MessageBox.Show("故障");
                    // 发生了异常
                }
            }
            //加热状态表
            OperateResult<bool[]> Jrztb = omronFinsNet.ReadBool("W42", 368);
            {
                if (Jrztb.IsSuccess)
                {
                    JRZTB = Jrztb.Content;
                }
                else
                {
                    // 发生了异常
                }
            }
            //保温状态表
            OperateResult<bool[]> Bwztb = omronFinsNet.ReadBool("W74", 368);
            {
                if (Bwztb.IsSuccess)
                {
                    BWZTB = Bwztb.Content;
                }
                else
                {
                    // 发生了异常
                }
            }


            ////读w42到169
            //OperateResult<bool[]> ceshi1 = omronFinsNet.ReadBool("W42", 1500);
            //{
            //    if (ceshi1.IsSuccess)
            //    {
            //        //加热状态
            //        JRZTB = ceshi1.Content.Skip(0).Take(368).ToArray();
            //        //保温状态
            //        BWZTB = ceshi1.Content.Skip(368).Take(368).ToArray();
            //        //PID加热状态
            //        PIDSCB = ceshi1.Content.Skip(736).Take(368).ToArray();
            //        //主传感器断线表
            //        ZCGQDXB = ceshi1.Content.Skip(1104).Take(368).ToArray();

            //    }
            //    else
            //    {
            //        // 发生了异常
            //    }
            //}
            ////w170到297
            //OperateResult<bool[]> ceshi3 = omronFinsNet.ReadBool("W170", 2044);
            //{
            //    if (ceshi3.IsSuccess)
            //    {
            //        bool[] ceshi4 = ceshi3.Content;
            //    }
            //    else
            //    {
            //        // 发生了异常
            //    }
            //}
            ////w298到425
            //OperateResult<bool[]> ceshi5 = omronFinsNet.ReadBool("W298", 2044);
            //{
            //    if (ceshi5.IsSuccess)
            //    {
            //        bool[] ceshi6 = ceshi5.Content;
            //    }
            //    else
            //    {
            //        // 发生了异常
            //    }
            //}
            ////w426到425
            //OperateResult<bool[]> ceshi7 = omronFinsNet.ReadBool("W426", 1500);
            //{
            //    if (ceshi7.IsSuccess)
            //    {
            //        bool[] ceshi8 = ceshi7.Content;
            //    }
            //    else
            //    {
            //        // 发生了异常
            //    }
            //}
        }

        /// <summary>
        /// 写bool[]
        /// </summary>
        /// <param name="aa">地址</param>
        /// <param name="vs">数据</param>
        private bool Xfgsn(string aa, bool[] vs)
        {

            OperateResult read = omronFinsNet.Write(aa, vs);
            {
                if (read.IsSuccess)
                {
                    //写完毕刷新数据
                    bzw3 = true;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        /// <summary>
        /// 写位short
        /// </summary>
        /// <param name="aa">地址</param>
        /// <param name="vs">数据</param>
        /// <returns></returns>
        private bool Xshort1(string aa, short vs)
        {

            OperateResult read = omronFinsNet.Write(aa, vs);
            {
                if (read.IsSuccess)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        /// <summary>
        /// 写short数组
        /// </summary>
        /// <param name="aa">写入位置</param>
        /// <param name="vs">写入数组</param>
        /// <returns></returns>
        private bool Xshort(string aa, short[] vs)
        {

            OperateResult read = omronFinsNet.Write(aa, vs);
            {
                if (read.IsSuccess)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        /// <summary>
        /// 窗口启动加载事件
        /// 开线程读写PLC
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Thread m = new Thread(new ThreadStart(PlcConnect));
            m.Start();

        }
        /// <summary>
        /// UI刷新
        /// </summary>
        private void UIgx(object source, System.Timers.ElapsedEventArgs e)
        {
            if (bzw2)
            {
                UIgxqjgx();
            }

            if (bzw1 & bzw2)
            {
                UIsnb();

                bzw1 = false;
            }
            if (bzw3 & bzw2)
            {
                UIdby();
                bzw3 = false;
            }
            UIfgzt();
            if (bzw4 & bzw2)
            {
                UIxtcs();
                bzw4 = false;
            }

        }
        /// <summary>
        /// 全局数据更新
        /// 包含分柜传感器数据
        /// </summary>
        private void UIgxqjgx()
        {
            new Thread(() =>
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    //系统运行
                    if (SystemStatus[0])
                    {
                        this.xtyun.Fill = new SolidColorBrush(Colors.Green);
                    }
                    else
                    {
                        this.xtyun.Fill = new SolidColorBrush(Colors.Gray);
                    }
                    //系统报警
                    if (SystemStatus[1])
                    {
                        this.xtbj.Fill = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        this.xtbj.Fill = new SolidColorBrush(Colors.Gray);
                    }
                    //通信故障
                    if (SystemStatus[2])
                    {
                        this.txzg.Fill = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {

                        this.txzg.Fill = new SolidColorBrush(Colors.Gray);
                    }
                    //主急停
                    if (SystemStatus[3])
                    {
                        this.jtzg.Fill = new SolidColorBrush(Colors.Green);
                    }
                    else
                    {
                        //图像填充
                        //this.Temperaturelimit1.Fill = new SolidColorBrush(Colors.Red);
                        this.jtzg.Fill = new SolidColorBrush(Colors.Gray);
                    }
                    //通信故障代码
                    this.PLCtxdm.Text = Convert.ToString(uint_D6, 16);
                    //PLC运行故障代码
                    this.PLCgzdm.Text = Convert.ToString(uint_D8, 16);

                    //加热点数据更新
                    int i = FGB.SelectedIndex;
                    i *= 16;
                    //主传感器温度
                    Label[] zwdbs = { ztwd1, ztwd2, ztwd3, ztwd4, ztwd5, ztwd6, ztwd7, ztwd8,
                                       ztwd9,ztwd10,ztwd11,ztwd12,ztwd13,ztwd14,ztwd15,ztwd16};
                    //副传感器温度
                    Label[] fwdbs = {ftwd1, ftwd2, ftwd3, ftwd4, ftwd5, ftwd6, ftwd7, ftwd8,
                                      ftwd9,ftwd10,ftwd11,ftwd12,ftwd13,ftwd14,ftwd15,ftwd16 };

                    //目标温度
                    Label[] mbwdss = { mbws1, mbws2, mbws3, mbws4, mbws5, mbws6, mbws7, mbws8,
                                         mbws9, mbws10, mbws11, mbws12, mbws13, mbws14, mbws15, mbws16};
                    //限温开关
                    Ellipse[] ceshi = {xwkg1, xwkg2, xwkg3, xwkg4, xwkg5, xwkg6, xwkg7, xwkg8,
                                        xwkg9, xwkg10, xwkg11, xwkg12,xwkg13,xwkg14,xwkg15,xwkg16};
                    //pid输出
                    Ellipse[] pidscs = { sczs1, sczs2, sczs3, sczs4, sczs5, sczs6, sczs7, sczs8,
                                          sczs9, sczs10, sczs11, sczs12, sczs13, sczs14, sczs15, sczs16,};
                    //加热
                    Ellipse[] jrds = { jrzs1, jrzs2, jrzs3, jrzs4, jrzs5, jrzs6, jrzs7, jrzs8,
                                        jrzs9, jrzs10, jrzs11, jrzs12, jrzs13, jrzs14, jrzs15, jrzs16};
                    //保温
                    Ellipse[] bwds = { bwzs1, bwzs2, bwzs3, bwzs4, bwzs5, bwzs6, bwzs7, bwzs8,
                                        bwzs9, bwzs10,bwzs11,bwzs12,bwzs13,bwzs14,bwzs15,bwzs16};

                    for (int a = 0; a < 15; a++)
                    {
                        if (JRDB[i + a])
                        {
                            //主温度
                            zwdbs[a].Content = Sjzh(ZWDB[i + a]);
                            //副温度
                            fwdbs[a].Content = Sjzh(FZWDB[i + a]);
                            mbwdss[a].Content = Sjzh(JRDSDWS[i + a]);
                            if (!XWKGB[i + a])
                            {
                                ceshi[a].Fill = new SolidColorBrush(Colors.Green);
                            }
                            else
                            {
                                ceshi[a].Fill = new SolidColorBrush(Colors.Gray);
                            }
                            if (PIDSCB[i + a])
                            {
                                pidscs[a].Fill = new SolidColorBrush(Colors.Green);
                            }
                            else
                            {
                                pidscs[a].Fill = new SolidColorBrush(Colors.Gray);
                            }
                            if (JRZTB[i + a])
                            {
                                jrds[a].Fill = new SolidColorBrush(Colors.Green);
                            }
                            else
                            {
                                jrds[a].Fill = new SolidColorBrush(Colors.Gray);
                            }
                            if (BWZTB[i + a])
                            {
                                bwds[a].Fill = new SolidColorBrush(Colors.Green);
                            }
                            else
                            {
                                bwds[a].Fill = new SolidColorBrush(Colors.Gray);
                            }
                        }
                        else
                        {
                            //主温度
                            zwdbs[a].Content = "";
                            //副温度
                            fwdbs[a].Content = "";
                            //目标温度
                            mbwdss[a].Content = "";
                            ceshi[a].Fill = new SolidColorBrush(Colors.Gray);
                            jrds[a].Fill = new SolidColorBrush(Colors.Gray);
                            bwds[a].Fill = new SolidColorBrush(Colors.Gray);
                            pidscs[a].Fill = new SolidColorBrush(Colors.Gray);
                        }
                    }

                }));
            }).Start();
        }
        /// <summary>
        /// 取浮点数
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private string Sjzh(short i)
        {
            //取浮点数
            if (i != 0)
            {
                float a = (float)(i / 10);
                a += (float)(i % (i / 10)) / 10;
                return Convert.ToString(a);
            }
            else
            {
                return Convert.ToString(i);
            }
        }
        /// <summary>
        /// 系统参数设置
        /// </summary>
        private void UIxtcs()
        {
            new Thread(() =>
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    if (TJSXWD != 0)
                    {
                        //最高上限取浮点数
                        float a = (float)(TJSXWD / 10);
                        a += (float)(TJSXWD % (TJSXWD / 10)) / 10;
                        this.tjsxwd.Text = Convert.ToString(a);
                        this.yzdbwd.Text = Convert.ToString(YZDBWD);
                        this.bjpcwd.Text = Convert.ToString(BJPCWD);
                        this.zfwddb.Text = Convert.ToString(ZFWDDB);
                    }
                    MessageBox.Show("更新成功");
                }));
            }).Start();
        }
        /// <summary>
        /// 分柜使能更新
        /// </summary>
        private void UIsnb()
        {
            new Thread(() =>
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    //分柜使能
                    CheckBox[] checks = { Cabinet1, Cabinet2, Cabinet3, Cabinet4, Cabinet5, Cabinet6, Cabinet7,
                                          Cabinet8, Cabinet9, Cabinet10, Cabinet11, Cabinet12, Cabinet13, Cabinet14,
                                          Cabinet15, Cabinet16, Cabinet17, Cabinet18, Cabinet19, Cabinet20, Cabinet21,
                                          Cabinet22, Cabinet23, Cabinet24, Cabinet25, Cabinet26, Cabinet27, Cabinet28,
                                           Cabinet29, Cabinet30, Cabinet31, Cabinet32};

                    for (int i = 0; i < FGSNB.Length; i++)
                    {
                        checks[i].IsChecked = !FGSNB[i];
                    }

                }));
            }).Start();
        }
        /// <summary>
        /// 切换分控柜更新数据
        /// </summary>
        private void UIdby()
        {
            new Thread(() =>
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    //获取打开哪个标签页
                    int i = FGB.SelectedIndex;
                    i *= 16;

                    fgyxztl.Header = Convert.ToString(FGB.SelectedIndex + 1) + "号分柜运行状态";
                    qdjrk.Header = Convert.ToString(FGB.SelectedIndex + 1) + "号分配工艺";
                    //主加热点使能数组
                    CheckBox[] zcgqs = { Heatingzone1, Heatingzone2, Heatingzone3, Heatingzone4, Heatingzone5, Heatingzone6,
                                        Heatingzone7,Heatingzone8,Heatingzone9,Heatingzone10,Heatingzone11,Heatingzone12,
                                        Heatingzone13,Heatingzone14,Heatingzone15,Heatingzone16};
                    //副加热点使能
                    CheckBox[] fcgqs = { Secondarysensor1, Secondarysensor2, Secondarysensor3, Secondarysensor4, Secondarysensor5,
                                        Secondarysensor6,Secondarysensor7,Secondarysensor8,Secondarysensor9,Secondarysensor10,
                                        Secondarysensor11,Secondarysensor12,Secondarysensor13,Secondarysensor14,Secondarysensor15,
                                         Secondarysensor16};                  
                    for (int i1 = 0; i1 < zcgqs.Length; i1++)
                    {

                        //判断分柜使能
                        if (FGSNB[FGB.SelectedIndex])
                        {
                            zcgqs[i1].IsEnabled = false;
                            fcgqs[i1].IsEnabled = false;                          
                        }
                        else
                        {
                            zcgqs[i1].IsEnabled = true;
                            zcgqs[i1].IsChecked = JRDB[i + i1];
                            fcgqs[i1].IsEnabled = true;
                            fcgqs[i1].IsChecked = FJRDB[i + i1];                          
                        }
                    }
                }));
            }).Start();
        }
        /// <summary>
        /// 分柜状态
        /// </summary>
        private void UIfgzt()
        {
            new Thread(() =>
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    // bool[] a1 = new bool[5];
                    int i = FGB.SelectedIndex;
                    //分柜通信
                    if (FGSTX[i])
                    {
                        this.FGTX.Fill = new SolidColorBrush(Colors.Green);
                    }
                    else
                    {
                        this.FGTX.Fill = new SolidColorBrush(Colors.Gray);
                    }
                    //分柜空开
                    if (FGKKB[i])
                    {
                        this.FGKK.Fill = new SolidColorBrush(Colors.Green);
                    }
                    else
                    {
                        this.FGKK.Fill = new SolidColorBrush(Colors.Gray);
                    }
                    //分柜急停
                    if (FGJTB[i])
                    {
                        this.FGJT.Fill = new SolidColorBrush(Colors.Green);
                    }
                    else
                    {
                        this.FGJT.Fill = new SolidColorBrush(Colors.Gray);
                    }
                    //分柜接触器
                    if (FGJCQB[i])
                    {
                        this.FGJCQ.Fill = new SolidColorBrush(Colors.Green);
                    }
                    else
                    {
                        this.FGJCQ.Fill = new SolidColorBrush(Colors.Gray);
                    }
                }));
            }).Start();
        }
        /// <summary>
        /// 数据更新按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //更新数据
            bzw1 = true;
            bzw3 = true;
        }


        /// <summary>
        /// 下拉菜单更新时，刷新数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FGB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bzw3 = true;
        }
        /// <summary>
        /// 系统参数更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void xtcsgx_Click(object sender, RoutedEventArgs e)
        {
            bzw4 = true;
        }
        /// <summary>
        /// 更改加热点使能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //加热点使能选框数组
            CheckBox[] zjrds = { Heatingzone1, Heatingzone2, Heatingzone3, Heatingzone4, Heatingzone5, Heatingzone6,
                                  Heatingzone7, Heatingzone8, Heatingzone9, Heatingzone10, Heatingzone11, Heatingzone12,
                                  Heatingzone13, Heatingzone14, Heatingzone15, Heatingzone16,};
            //副加热选框数组
            CheckBox[] fjrds = { Secondarysensor1, Secondarysensor2, Secondarysensor3, Secondarysensor4, Secondarysensor5,
                                 Secondarysensor6, Secondarysensor7, Secondarysensor8, Secondarysensor9, Secondarysensor10,
                                 Secondarysensor11, Secondarysensor12, Secondarysensor13, Secondarysensor14, Secondarysensor15,
                                 Secondarysensor16,};

            //判断加热点使能
            for (int i = 0; i < zjrds.Length; i++)
            {
                if ((bool)zjrds[i].IsChecked)
                {
                    jrdsns[i] = true;
                }
                else
                {
                    jrdsns[i] = false;
                }
            }
            //判断副加热点使能
            for (int i = 0; i < fjrds.Length; i++)
            {
                if ((bool)fjrds[i].IsChecked)
                {
                    fjrdsns[i] = true;
                }
                else
                {
                    fjrdsns[i] = false;
                }
            }
            //写入加热点使能
            string jrdwz = "H" + Convert.ToString((FGB.SelectedIndex + 1));
            Xfgsn(jrdwz, jrdsns);
            //写入副加热点使能
            string fjrdwz = "H" + Convert.ToString((FGB.SelectedIndex + 33));
            Xfgsn(fjrdwz, fjrdsns);


        }



        /// <summary>
        /// 下拉列表关闭时更新分柜使能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FGB_DropDownClosed(object sender, EventArgs e)
        {

            //分柜使能选框数组
            CheckBox[] fgbs = { Cabinet1, Cabinet2, Cabinet3, Cabinet4, Cabinet5, Cabinet6, Cabinet7,
                                Cabinet8, Cabinet9, Cabinet10,Cabinet11,Cabinet12,Cabinet13,Cabinet14,Cabinet15,
                                Cabinet16,Cabinet17,Cabinet18,Cabinet19,Cabinet20,Cabinet21,Cabinet22,Cabinet23,
                                Cabinet24,Cabinet25,Cabinet26,Cabinet27,Cabinet28,Cabinet29,Cabinet30,Cabinet31,
                                Cabinet32};
            //比对分柜使能数组
            bool[] vs = {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true,
                            true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true};
            //判断分柜勾选
            for (int i = 0; i < fgbs.Length; i++)
            {
                if ((bool)fgbs[i].IsChecked)
                {
                    vs[i] = false;
                }
                else
                {
                    vs[i] = true;
                }
            }
            //比较是否相同
            if (!Enumerable.SequenceEqual(vs, fgsn))
            {
                fgsn = vs;
                //写入分柜使能

                if (Xfgsn("H500", fgsn))
                {
                    MessageBox.Show("更新分柜使能成功");
                    //刷新页面
                    bzw3 = true;
                }
                else
                {
                    MessageBox.Show("更新分柜失败");
                }
            }

        }

        /// <summary>
        /// 保存曲线参数按键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

            short[] vs1 = new short[368];
            short[] vs2 = new short[368];
            short[] vs3 = new short[368];
            short[] vs4 = new short[368];
            try
            {
                if (mbwdsz.Text != "")
                {
                    float mbwd = Convert.ToSingle(mbwdsz.Text);
                    //整数型目标温度
                    MBWD = Convert.ToInt16((mbwd * 10));
                }
                else
                {
                    MessageBox.Show("输入目标温度");
                    return;
                }
                if (swsjsz.Text != "")
                {
                    SWSJ = Convert.ToInt16(swsjsz.Text);
                }
                else
                {
                    MessageBox.Show("输入升温时间");
                    return;
                }
                if (bwsjsz.Text != "")
                {
                    BWSJ = Convert.ToInt16(bwsjsz.Text);
                }
                else
                {
                    MessageBox.Show("输入保温时间");
                    return;
                }
                if (wcsz.Text != "")
                {
                    JRDBJZ = Convert.ToInt16(wcsz.Text);
                }
                else
                {
                    MessageBox.Show("输入温差报警");
                    return;
                }
                if (xrqxh.Text != "")
                {
                    int a0 = Convert.ToUInt16(xrqxh.Text);
                    if (a0 >= 1 && a0 <= 6)
                    {

                        XZQXH = Convert.ToInt16(xrqxh.Text);
                        XZQXH -= 1;
                    }
                    else
                    {
                        MessageBox.Show("输入曲线超出范围");
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("输入写入曲线");
                    return;
                }
            }
            catch (Exception)
            {

                MessageBox.Show("输入包含非数字");
            }

            for (int i = 0; i < vs1.Length - 6; i += 6)
            {
                vs1[i + XZQXH] = MBWD;
            }
            for (int i = 0; i < vs1.Length - 6; i += 6)
            {
                vs2[i + XZQXH] = SWSJ;
            }
            for (int i = 0; i < vs1.Length - 6; i += 6)
            {
                vs3[i + XZQXH] = BWSJ;
            }
            for (int i = 0; i < vs1.Length - 6; i += 6)
            {
                vs4[i + XZQXH] = JRDBJZ;
            }
            //目标温度
            int ii1 = 1572;
            Xdws(vs1, ii1);
            //升温时间
            int ii2 = 3780;
            Xdws(vs2, ii2);
            //保温时间
            int ii3 = 5988;
            Xdws(vs3, ii3);
            //加热报警值
            int ii4 = 8196;
            Xdws(vs4, ii4);
            MessageBox.Show("写入成功");

        }
        /// <summary>
        /// 写6遍368个数据
        /// </summary>
        /// <param name="vs"></param>
        /// <param name="ii"></param>
        private void Xdws(short[] vs, int ii)
        {

            for (int i = 0; i < 6; i++)
            {
                if (Xshort("D" + Convert.ToString(ii), vs))
                {
                    ii += 368;
                }
                else
                {
                    MessageBox.Show("写入失败");
                }
            }
        }
        /// <summary>
        /// 保存系统参数设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            short a1, a2, a3, a4;
            try
            {
                //停机上限
                a1 = Convert.ToInt16(tjsxwd.Text);
                a1 *= 10;
                Xshort1("D2", a1);
                // 严重对比温度
                a2 = Convert.ToInt16(yzdbwd.Text);
                Xshort1("D1", a2);
                // 报警偏差
                a3 = Convert.ToInt16(bjpcwd.Text);
                Xshort1("D3", a3);
                // 主副对比
                a4 = Convert.ToInt16(zfwddb.Text);
                Xshort1("D5", a4);
            }
            catch (Exception)
            {
                MessageBox.Show("设置数据不对");
            }
            MessageBox.Show("保存成功");
        }
        /// <summary>
        /// 启动加热，分配加热点工艺号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            //输入的工艺号
            short hqdgyh;
            //输入的曲线号
            short hqdxh;

            try
            {
                short tmel = Convert.ToInt16(qdgyh.Text);
                if (tmel > 0 && tmel < 368)
                {
                    hqdgyh = tmel;
                }
                else
                {
                    MessageBox.Show("错误：工艺需要输入1——368之内的数字！");
                    return;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("错误：工艺需要输入1——368之内的数字！");
                return;
            }
            
            try
            {
                short tmel1 = Convert.ToInt16(qxhsz.Text);
                if (tmel1 > 0 && tmel1 < 6)
                {
                    hqdxh = tmel1;
                }
                else
                {
                    MessageBox.Show("错误：曲线需要输入1——6之内的数字！");
                    return;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("错误：曲线需要输入1——6之内的数字！");
                return;
            }

            CheckBox[] checkBoxes = { Heatingswitch1, Heatingswitch2, Heatingswitch3, Heatingswitch4, Heatingswitch5,
                                         Heatingswitch6, Heatingswitch7, Heatingswitch8, Heatingswitch9, Heatingswitch10,
                                         Heatingswitch11, Heatingswitch12, Heatingswitch13, Heatingswitch14, Heatingswitch5,
                                         Heatingswitch16};
            //获取打开哪个标签页
            int i1 = FGB.SelectedIndex;
            i1 *= 16;

            //工艺起始地址D10404
            //标签页针对的地址
            int jbqydz = 10404 + i1;

            //曲线起始D14084
            int qxqsdzs = 14084 + i1;

            for (int i = 0; i < checkBoxes.Length; i++)
            {
                if ((bool)checkBoxes[i].IsChecked)
                {
                    if (!Xshort1("D" + Convert.ToString((jbqydz + i)), hqdgyh))
                    {
                        MessageBox.Show("错误：写入工艺失败！");
                        return;
                    }
                    if (!Xshort1("D" + Convert.ToString((qxqsdzs + i)), hqdxh))
                    {
                        MessageBox.Show("错误：写入曲线失败！");
                        return;
                    }
                }
            }
            if (i1 > 16) 
            {
                i1 /= 16;
            }
            //地址拼接
            string dizhi1 = "W" + Convert.ToString(i1 + 10) + "." + Convert.ToString((hqdgyh));
            //启动工艺开始加热
            OperateResult read = omronFinsNet.Write(dizhi1,true);
            {
                if (read.IsSuccess)
                {

                    MessageBox.Show("写入成功，工艺启动！");
                }
                else
                {
                    MessageBox.Show("数据写入未成功，工艺启动失败！");
                }
            }
        }
    }
 }
