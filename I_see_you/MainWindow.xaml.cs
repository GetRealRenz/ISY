using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using System.Linq;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Windows.Threading;
using MahApps.Metro.Controls.Dialogs;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Media.Animation;
using I_see_you.Config_Class;
using I_see_you.Class;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.Windows.Media;
using System.Collections.Generic;
using Ninject;
using ISY.Domain.Abstract;
using ISY.Domain.Entities;
using ISY.Domain.Concrete;
using ISY_SERV;
using System.Data.SqlClient;
using System.Data.Linq;


namespace I_see_you
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        TcpClient client = new TcpClient();                    //Клиента
        DispatcherTimer timer = new DispatcherTimer();         //Таймеры
        NetworkStream serverStream = default(NetworkStream);   //Сокеты
        Configer ConfigClass = new Configer();                 //Клас конфигурации
        string readData = null;                                //Считываемая строка с сервера
        string InfoUserStr = null;                             //Инфо о пользователе
        Thread ctThread;                                       //Поток
        double La = 0;                                         //Шерина
        double Lo = 0;                                         //Долгота
        bool ServerOff = true;                                 //Выключен или включен сервер

        IRepositoryBase<Profile> rep;
        public MainWindow(IRepositoryBase<Profile> _rep)
        {
            InitializeComponent();
            //*******************************************
            
            rep = _rep;
            //Считывание с конфигурационного файла
            try
            {
                XmlSerializer XmlSerConfig = new XmlSerializer(typeof(Configer));
                TextReader r = new StreamReader(@"Configurations.xml");
                ConfigClass = (Configer)XmlSerConfig.Deserialize(r);
                IpServer.Text = ConfigClass.IP;
                r.Close();
            }
            //Если программа была запущенна первый раз (Создание конфигурационного файла)
            catch
            {
                ConfigClass.IP = IpServer.Text;
                XmlSerializer serializer = new XmlSerializer(typeof(Configer));
                TextWriter textWriter = new StreamWriter(@"Configurations.xml");
                serializer.Serialize(textWriter, ConfigClass);
                textWriter.Close();
            }

            //*******************************************
            LoadMap();
            LoadServer();
            timer.Tick += new EventHandler(timerTick);
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();
        }


        //Выводит окошко "сменил с / на"
        public void ShowMenuName(string Name1, string Name2)
        {
            MesageName.N1_N2.Content = Name1 +" изменил(а) имя на " + Name2;
            DoubleAnimation da = new DoubleAnimation();
            da.From = 0;
            da.To = 1.5;
            da.DecelerationRatio = 1;
            da.AutoReverse = true;
            da.Duration = new Duration(TimeSpan.FromSeconds(2));
            MesageName.BeginAnimation(OpacityProperty, da);

        }

        //Выводит окошко "Вышел"
        public void ShowMenuDisconected(string Name1)
        {
            MesageName.N1_N2.Content = Name1 + " : Вышел";
            DoubleAnimation da = new DoubleAnimation();
            da.From = 0;
            da.To = 1.5;
            da.DecelerationRatio = 1;
            da.AutoReverse = true;
            da.Duration = new Duration(TimeSpan.FromSeconds(2));
            MesageName.BeginAnimation(OpacityProperty, da);

        }


        //Получения и парсинг кординат / смены имени / Отключения пользователя
        private void timerTick(object sender, EventArgs e)
        {
            #region переменные
            string NameClient = "No Name";
            string q1;
            string q2;
            string ssh;
            string Cheat;
            #endregion

            #region Смена имени
            try
            {
                Cheat = readData.Substring(readData.IndexOf(':'), 17);

                //Сменил имя
                if (Cheat == ":changed name on:")
                {
                    q1 = readData.Substring(0, readData.IndexOf(':'));
                    ssh = readData.Substring(readData.IndexOf(':') + 1);
                    q2 = ssh.Substring(ssh.IndexOf(':') + 1);


                    foreach (var li in ListConnections.Items)
                    {
                        var g = (Label)li;
                        if (g.Content.ToString() == q1)
                        {
                            ShowMenuName(q1,q2); //Вывод меню с низу

                            g.Content = q2;
                            foreach (var ff in gMapControl1.Markers)
                            {
                                var wert = (MarkerUSE)ff.Shape;
                                try
                                {
                                    if (wert.NameClientGPS.Content.ToString() == q1)
                                    {
                                        wert.NameClientGPS.Content = q2;
                                        return;
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
            catch { }
            #endregion

            #region Потерянно сойденение с сервером
            if (!ServerOff)
            {
                this.ShowMessageAsync("Потерянно соединения с сервером", "Ошибка");
                timer.Stop();
                ButtonRec.IsEnabled = true;
                StatusConnect.Foreground = Brushes.Red;
                ErorMesa.Content = "Нет соединения с сервером!";
                StatusConnect.Content = "Не удалось подключиться к серверу";

                try
                {
                    ctThread.Abort();
                    client.Close();
                }
                catch { }
                return;
            }
            #endregion

            #region Disconect user
            if (readData != "" && readData != null)
            {
                //Anton, disconnected
                //Disconect
                try
                {
                    if (readData.Substring(readData.IndexOf("disconnected")) == "disconnected")
                    {
                        string NameDisconectet = readData.Substring(0, readData.IndexOf(','));

                        foreach (var li in ListConnections.Items)
                        {
                            var g = (Label)li;
                            if (g.Content.ToString() == NameDisconectet)
                            {
                                ListConnections.Items.Remove(li);
                                foreach (var ff in gMapControl1.Markers)
                                {
                                    var wert = (MarkerUSE)ff.Shape;

                                    try
                                    {
                                        if (wert.NameClientGPS.Content.ToString() == NameDisconectet)
                                        {
                                            ShowMenuDisconected(NameDisconectet);
                                            ff.Clear();
                                            readData = "";
                                            return;
                                        }
                                    }
                                    catch { }
                                }       
                            }
                        }
                    }
                }
                catch { }
            #endregion

            #region Add new user
                //Парсинг кординат и считывание имени 
                try
                {

                    InfoUserStr = readData.Remove(readData.IndexOf('{'));
                    string Pos2 = readData.Remove(0, readData.IndexOf('{'));


                    string[] inffor;

                    inffor = InfoUserStr.Split('&');

                    Pos2 = Pos2.Replace(',', ':');
                    string[] StrArrey;

                    StrArrey = Pos2.Split(':');

                    StrArrey[2] = StrArrey[2].Remove(0, 1);
                    StrArrey[4] = StrArrey[4].Remove(0, 1);

                    NameClient = inffor[0];


                    StrArrey[7] = StrArrey[7].Remove(StrArrey[7].IndexOf('"'));
                    q1 = StrArrey[5];
                    q2 = StrArrey[7];

                    q1 = q1.Replace('.', ',');
                    q2 = q2.Replace('.', ',');

                    if (q1 == "" || q1 == null)
                        return;
                    if (q2 == "" || q2 == null)
                        return;


                    La = double.Parse(q1);
                    Lo = double.Parse(q2);

                    //*****************************

                    bool CheckClient = true;

                    //Добавления если user>1 или подключения тогоже самого
                    foreach (var li in ListConnections.Items)
                    {
                        var g = (Label)li;
                        if (g.Content.ToString() == NameClient)
                        {
                            foreach (var ff in gMapControl1.Markers)
                            {
                               var wert=(MarkerUSE)ff.Shape;

                               if (wert.NameClientGPS.Content.ToString() == NameClient)
                               {
                                   ff.Position = new PointLatLng(La, Lo);
                                    Coordinates_user CU= new Coordinates_user();
                                    CU.C_La = La;
                                    CU.C_Lo = Lo;

                                    #region Повторное считывание дданых 
                                    Person_marks Pers1 = new Person_marks();
                                    GetSet_info_person(Pers1, La, Lo, NameClient);

                                    wert.Tag = Pers1;
                                    #endregion



                                    g.Tag = CU; //Повторная запись кординат для быстрого перехода
                                   return;
                               }     
                 
                            }
                            CheckClient = false;
                            break;
                        }
                    }

                    //Добавления в list
                    if (CheckClient)
                    {
                        Label tet = new Label();
                        Coordinates_user CU= new Coordinates_user();
                        CU.C_La = La;
                        CU.C_Lo = Lo;

                        tet.Tag = CU;
                        tet.MouseDoubleClick+=tet_MouseDoubleClick;
                        tet.Content = NameClient;
                        ListConnections.Items.Add(tet);
                    }


                    GMapMarker mroute = new GMapMarker((new PointLatLng(La, Lo)));
                    var b = new MarkerUSE();
                    b.MouseDoubleClick+=b_MouseDoubleClick;

#region Контактные дданые
                    Person_marks Pers = new Person_marks();
                    GetSet_info_person(Pers, La, Lo, NameClient);
                    b.Tag = Pers;


#endregion
                    b.NameClientGPS.Content = NameClient;
                    mroute.Shape = b;
                   // gMapControl1.Markers.Clear();
                    gMapControl1.Markers.Add(mroute);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
                #endregion

            }
        }

 

        //Вывод подробной информации о клиенте
        void b_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
           var b = (MarkerUSE)sender;

           Person_marks Pers = (Person_marks)b.Tag;

           AddresText.Text=Pers.address;
           LoginText.Content = Pers.Login;
           CordText.Content = Pers.cordiantes;
           TextName.Content=Pers.FName;
           TextLname.Content=Pers.Lname;
           TextSex.Content=Pers.sex;
           TextEmail.Content=Pers.Email;
           TextAge.Content=Pers.age;

            PodrobInfo.IsOpen = true;
        }



        //Перейти к Пользователю на карте
        private void tet_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Label GoUserName = (Label)sender;
            Coordinates_user CU = (Coordinates_user)GoUserName.Tag;

            gMapControl1.Position = new PointLatLng(CU.C_La, CU.C_Lo);
            gMapControl1.Zoom = 15;
        }

        //Загрузка сервера
        public void LoadServer()
        {
            try
            {
                IPAddress ipAddress = IPAddress.Parse(IpServer.Text);
                client.Connect(ipAddress, 11000);
                serverStream = client.GetStream();

              /*  byte[] outStream = Encoding.ASCII.GetBytes("" + "$");
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();*/
                ctThread = new Thread(getMessage);
                ctThread.Start();
                ErorMesa.Content = "";

                StatusConnect.Foreground = Brushes.Green;
                ErorMesa.Content = "";
                StatusConnect.Content = "Подключено";
                ButtonRec.IsEnabled = false;

            }
            catch (Exception ex)
            {
                ServerOff = false;
                ButtonRec.IsEnabled = true;
                StatusConnect.Foreground = Brushes.Red;
                ErorMesa.Content = "Нет соединения с сервером!";
                StatusConnect.Content = "Не удалось подключиться к серверу";
                return;
            }
        }


        //Загрузка карты
        public void LoadMap()
        {
            gMapControl1.MapProvider = GoogleMapProvider.Instance;
            //get tiles from server only
            gMapControl1.Manager.Mode = AccessMode.ServerOnly;
            //not use proxy
            GMapProvider.WebProxy = null;
            //center map on moscow
            gMapControl1.Position = new PointLatLng(55.755786121111, 37.617633343333);

            gMapControl1.IgnoreMarkerOnMouseWheel = true;

            gMapControl1.MaxZoom = 20;
            gMapControl1.MinZoom = 1;
            gMapControl1.Zoom = 2;

            SliderZoom.Value=gMapControl1.Zoom;

            //Марка default
            /* GMapMarker mroute = new GMapMarker((new PointLatLng(55.755786121111, 37.617633343333)));
               var b = new MarkerUSE();
               b.NameClientGPS.Content = "Тест";
               mroute.Shape = b;
               gMapControl1.Markers.Add(mroute);*/
        }


        //Сервер 
        private void getMessage()
        {
            while (true)
            {
                try
                {
                    serverStream = client.GetStream();
                    int buffSize = 0;
                    byte[] inStream = new byte[client.ReceiveBufferSize];
                    buffSize = client.ReceiveBufferSize;
                    serverStream.Read(inStream, 0, inStream.Length);
                    string returndata = Encoding.ASCII.GetString(inStream);
                    string txtEnding = returndata.Remove(returndata.IndexOf("\0"));
                    readData = txtEnding;
                }
                catch (Exception ex)
                {
                    ServerOff = false;
                    break;
                }
            }
        }

        //Закрытие программы
        private void CloseWindowISO(object sender, EventArgs e)
        {
            ConfigClass.IP = IpServer.Text;
            XmlSerializer serializer = new XmlSerializer(typeof(Configer));
            TextWriter textWriter = new StreamWriter(@"Configurations.xml");
            serializer.Serialize(textWriter, ConfigClass);
            textWriter.Close();
            try
            {
                ctThread.Abort();
                client.Close();
            }
            catch { }
        }


        //Открытие шторки
        private void ShowInstal(object sender, RoutedEventArgs e)
        {       
            InsFlip.IsOpen = true;

        }

        //Переподключения к серверу
        private void ReConnect(object sender, RoutedEventArgs e)
        {
            StatusConnect.Foreground = Brushes.Yellow;
            StatusConnect.Content = "Подключение...";
            try
            {
                ctThread.Abort();
                client.Close();
            }
            catch { }
            client = new TcpClient();

            try
            {
                IPAddress ipAddress = IPAddress.Parse(IpServer.Text);
                client.Connect(ipAddress, 11000);
                serverStream = client.GetStream();
                ctThread = new Thread(getMessage);
                /*byte[] outStream = Encoding.ASCII.GetBytes("" + "$");
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();*/
                ctThread.Start();
                ErorMesa.Content = "";
                ButtonRec.IsEnabled = true;
            }
            catch (Exception ex)
            {
                StatusConnect.Foreground = Brushes.Red;
                ErorMesa.Content = "Нет соединения с сервером!";
                StatusConnect.Content = "Не удалось подключиться к серверу";
                ServerOff = false;
                return;
            }
            ServerOff = true;
            timer.Start();
            ButtonRec.IsEnabled = false;
            StatusConnect.Foreground = Brushes.Green;
            ErorMesa.Content = "";
            StatusConnect.Content = "Подключено";

        }

        //Отключения от сервера
        private void DisconnectEvent(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            ButtonRec.IsEnabled = true;
            StatusConnect.Foreground = Brushes.Red;
            ErorMesa.Content = "Нет соединения с сервером!";
            StatusConnect.Content = "Не удалось подключиться к серверу";

            try
            {
                ctThread.Abort();
                client.Close();
            }
            catch { }
        }

        //Кнопка очистки карты
        private void Clear_map(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageBox.Show("Внимание!\n С карты будут удалены все пометки, и очищен список подключений. Продолжить?", "Очистить подключения", MessageBoxButton.YesNo, MessageBoxImage.Question))
            {
                gMapControl1.Markers.Clear();
                ListConnections.Items.Clear();
            }

        }



        #region Пакет разработчика
        //Пакет разработчика: Получить липовый покет 
        private void GetPacket(object sender, RoutedEventArgs e)
        {

            readData = "plexa&vladlen&pleshenkov&19&wertizator@ram.ru&Мужской{\"TypePacket\":3,\"PacketMass\":\"plexa :Latitude: 48.4426988, Longitude: 35.0165651\"}";
            
        }

        
        int InumerNaame = 0;
        String NameTest = "plexa";

        //Пакет разработчика: Смена имени 
        private void GetPacketNewName(object sender, RoutedEventArgs e)
        {
            InumerNaame++;
            readData = NameTest + ":changed name on:"+NameTest+InumerNaame;
            NameTest += InumerNaame;
        }
        //Пакет разработчика: Выщел 
        private void GetPacketDisconected(object sender, RoutedEventArgs e)
        {
            readData = NameTest+", disconnected";
        }
        #endregion



        //Зумирование карты (2 функции)
        //Зумирование слайдером
        private void SliderZoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            gMapControl1.Zoom = SliderZoom.Value;
        }

        //Зумирование колёсиком
        private void ZoomingMap()
        {
            SliderZoom.Value = gMapControl1.Zoom;
        }

        //Выбор карты
        private void SelectionMapBox(object sender, SelectionChangedEventArgs e)
        {
            if (0 == MapChenges.SelectedIndex)
                gMapControl1.MapProvider = GoogleMapProvider.Instance;
            if(1==MapChenges.SelectedIndex)
                gMapControl1.MapProvider = BingMapProvider.Instance;
            if (2 == MapChenges.SelectedIndex)
            gMapControl1.MapProvider = YandexMapProvider.Instance;
            if (3 == MapChenges.SelectedIndex)
                gMapControl1.MapProvider = WikiMapiaMapProvider.Instance;

            
        }



        //Вывод setings lable
        private void LeaveShow(object sender, MouseEventArgs e)
        {
            DoubleAnimation da = new DoubleAnimation();
            da.From = 1;
            da.To = 0;
            //da.DecelerationRatio = 1;
            da.AutoReverse = false;
            da.Duration = new Duration(TimeSpan.FromMilliseconds(70));
            LablSetings.BeginAnimation(OpacityProperty, da);


            ThicknessAnimation da1 = new ThicknessAnimation();
            da1.From = new Thickness(0, 2, 30, 0); 
            da1.To =new Thickness(0, -17, 30, 0); 
            //da.DecelerationRatio = 1;
            da1.AutoReverse = false;
            da1.Duration = new Duration(TimeSpan.FromMilliseconds(100));
            LablSetings.BeginAnimation(MarginProperty, da1);


            

            


        }

        //Спрятать setings lable
        private void EnterShow(object sender, MouseEventArgs e)
        {

            DoubleAnimation da = new DoubleAnimation();
            da.From = 0;
            da.To = 1;
            //da.DecelerationRatio = 1;
            da.AutoReverse = false;
            da.Duration = new Duration(TimeSpan.FromMilliseconds(70));
            LablSetings.BeginAnimation(OpacityProperty, da);



            ThicknessAnimation da1 = new ThicknessAnimation();
            da1.From = new Thickness(0, -17, 30, 0);
            da1.To =new Thickness(0, 2, 30, 0); 
            //da.DecelerationRatio = 1;
            da1.AutoReverse = false;
            da1.Duration = new Duration(TimeSpan.FromMilliseconds(100));
            LablSetings.BeginAnimation(MarginProperty, da1);
        }

        
        //Считывание с базы инфо о пользователе  
        public void GetSet_info_person(Person_marks PM, double latitude, double longitude, string LoginClient)
        {


            //  PM.FName=cust.Name;

            string[] inffor;

            inffor = InfoUserStr.Split('&');

            PM.Login = inffor[0];
            PM.FName = inffor[1];
            PM.Lname = inffor[2];
            PM.age = inffor[3];
            PM.Email = inffor[4];
            PM.sex  =    inffor[5];
           
            


            //Картинка


            PM.cordiantes = latitude.ToString() + " : " + longitude.ToString();
            PM.Login = LoginClient;


            List<Placemark> plc = null;
            var st = GMapProviders.GoogleMap.GetPlacemarks(new PointLatLng(La, Lo), out plc);
            if (st == GeoCoderStatusCode.G_GEO_SUCCESS && plc != null)
            {
                foreach (var pl in plc)
                {
                    PM.address = pl.Address;
                    break;
                }
            }

        }

    }





    /* 1-Тип пакета 1-4 
     * 2-Массив даных
     * 3-Размер пакета
     *  
     * mass[] Packet={Login,pasvord,age,addres....}
     * 
     * 
        SendMesage(1,mass,sizeof(mass))
     * {
     * obj send=new obj();
     * 
     * send.type=1;
     * sent.mass=mass;
     * send.sizeppackeg=sizeof(mass);
     * 
     * string packetJson=jsonserializable(obj);
     * 
     * отправка стринга
     * 
     * }
     * 
     * class obj()
     * {
     * int type;
     * obcjekt mass[];
     * int sizeppackeg
     * }
     * 
     * 
    */
}
