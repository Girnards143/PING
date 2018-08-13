using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;

namespace PING
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = args[0];
            string mailSubject = null;//mail subject info
            int Interval = Convert.ToInt32(args[1]);
            int c = 0; // counter for try-catch.With this counter if program crashes multiple times it will break from loop.

            //Adding Monitor Ips to IpList FROM DOCUMENT
            string dosya_yolu = path + "monitorIpList.txt";
            if (File.Exists(dosya_yolu) == false)//if file creating first time then add "IP,Room,Device" string 
                                                 //to understand which pattern should be used when writing IPs,Rooms and Devices 
            {
                FileStream fsM = new FileStream(dosya_yolu, FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter swM = new StreamWriter(fsM);
                swM.WriteLine("Write hospital name here!!");
                swM.WriteLine("IP,Room,Device");
                swM.Flush();
                fsM.Close();
            }
            FileStream fs = new FileStream(dosya_yolu, FileMode.OpenOrCreate, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);
            List<string> MonitorIpList = new List<string>();
            string yazi = sr.ReadLine();
            mailSubject = yazi;
            while (yazi != null)
            {
                if (yazi.Contains(".") == false)
                {
                    yazi = sr.ReadLine();
                    continue;
                }
                string[] array = new string[10];
                array = yazi.Split(",");
                MonitorIpList.Add(array[0]);
                yazi = sr.ReadLine();
            }


            //Adding Vent IPs to VentIPList From Document

            if (File.Exists(path + "VentIPList.txt") == false)// same process for monitorIPList.txt
            {
                FileStream fsM = new FileStream(path + "VentIPList.txt", FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter swM = new StreamWriter(fsM);
                swM.WriteLine("Write hospital name here!!");
                swM.WriteLine("IP,Room,Device");
                swM.Flush();
                fsM.Close();
            }
            FileStream fsVent = new FileStream(path + "VentIPList.txt", FileMode.OpenOrCreate, FileAccess.Read);
            StreamReader srVent = new StreamReader(fsVent);
            List<string> VentIPList = new List<string>();
            string satir = srVent.ReadLine();
            mailSubject = satir;
            while (satir != null)
            {
                if (satir.Contains(".") == false)
                {
                    satir = srVent.ReadLine();
                    continue;
                }
                string[] array = new string[10];
                array = satir.Split(",");
                VentIPList.Add(array[0]);
                satir = srVent.ReadLine();
            }

            //Adding  TO  e-mails to ePostaList FROM DOCUMENT
            List<string> ePostaList = new List<string>();
            FileStream fsEposta = new FileStream(path + "EpostaTo.txt", FileMode.OpenOrCreate, FileAccess.Read);
            StreamReader srEposta = new StreamReader(fsEposta);
            string temp = srEposta.ReadLine();
            while (temp != null)
            {
                ePostaList.Add(temp);
                temp = srEposta.ReadLine();
            }

            //Adding CC e-mails to ePostaListCC FROM DOCUMENT
            List<string> ePostaListCC = new List<string>();
            FileStream fsEpostaCC = new FileStream(path + "EpostaCC.txt", FileMode.OpenOrCreate, FileAccess.Read);
            StreamReader srEpostaCC = new StreamReader(fsEpostaCC);
            string t = srEpostaCC.ReadLine();
            while (t != null)
            {
                ePostaListCC.Add(t);
                t = srEpostaCC.ReadLine();
            }

            fsEpostaCC.Close();
            srEpostaCC.Close();
            fsEposta.Close();
            srEposta.Close();
            fs.Close();
            sr.Close();

            // Creating log file
            // create a new log file if the log file is inflated
            string file_path = path + "log.txt";
            if (File.Exists(file_path) == true)
            {
                FileInfo fileinfo = new FileInfo(file_path);//If "log.txt" is greater than 20MB, rename the file
                if (fileinfo.Length > 20000000)
                {
                    string s;
                    s = DateTime.Now.ToString();
                    s = s.Replace('.', '_');    //edit operation for valid path
                    s = s.Replace(':', '_');
                    s = s.Replace(' ', '_');
                    File.Move(path + "log.txt", path + "log" + s + ".txt");
                }
            }
            FileStream fslog;
            StreamWriter srlog;
            if (File.Exists(path + "log.txt") == true)
            {
                fslog = new FileStream(file_path, FileMode.Append, FileAccess.Write);
                srlog = new StreamWriter(fslog);
            }
            else
            {
                fslog = new FileStream(file_path, FileMode.Create, FileAccess.Write);
                srlog = new StreamWriter(fslog);
            }



            //Creating MailTime.txt to check LastMailTime
            FileStream fsMail;
            StreamWriter swMail;
            DateTime LastMailTime;
            string LastLine; // to read the last line of MailTime.txt
            if (File.Exists(path + "MailTime.txt") == false)
            {
                fsMail = new FileStream(path + "MailTime.txt", FileMode.Create, FileAccess.Write);
                swMail = new StreamWriter(fsMail);
                swMail.WriteLine((DateTime.Now - new TimeSpan(3, 0, 0)).ToString());
                swMail.Flush();
            }
            else
            {
                fsMail = new FileStream(path + "MailTime.txt", FileMode.Append, FileAccess.Write);
            }
            fsMail.Close();
            //Read last line
            LastLine = File.ReadLines(path + "MailTime.txt").Last();
            LastMailTime = Convert.ToDateTime(LastLine);


            Functions obje = new Functions(); //This object uses functions from functions class


            //creating statistics


            IDictionary<string, int> StatDict = new Dictionary<string, int>();  // dictionary for statistics file
                                                                                //string=ip , int=counter

            int total = 0;//holds the number of trial

            if (VentIPList.Count != 0 && File.Exists(path + "statisticsVent.txt") == false)//if VentIPList isn't empty and statisticsVent.txt isn't exists
                                                                                           //then create stat file for VentIPList

            {
                FileStream fss = new FileStream(path + "statisticsVent.txt", FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fss);
                sw.WriteLine("IP\t\t\t\tInterruption Count\t\tNumber of Trial");
                sw.WriteLine("---------------\t\t\t---------------\t\t\t---------------");
                foreach (var ip in VentIPList)
                {
                    StatDict.Add(ip, 0); // every interruption count is 0 at the beginning
                }
                foreach (var entry in StatDict)
                {
                    sw.WriteLine("{0}\t\t\t\t{1}\t\t\t\t{2}", entry.Key, entry.Value, total);
                }
                sw.Flush();
                fss.Close();
            }
            StatDict.Clear();


            if (MonitorIpList.Count != 0 && File.Exists(path + "statisticsMonitor.txt") == false)//if MonitorIPList isn't empty and exists then create stat file for VentIPList

            {
                FileStream fss = new FileStream(path + "statisticsMonitor.txt", FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fss);
                sw.WriteLine("IP\t\t\t\tInterruption Count\t\tNumber of Trial");
                sw.WriteLine("---------------\t\t\t---------------\t\t\t---------------");
                foreach (var ip in MonitorIpList)
                {
                    StatDict.Add(ip, 0); // every interruption count is 0 at the beginning
                }
                foreach (var entry in StatDict)
                {
                    sw.WriteLine("{0}\t\t\t\t{1}\t\t\t\t{2}", entry.Key, entry.Value, total);
                }
                sw.Flush();
                fss.Close();
            }
            StatDict.Clear();



            for (int k = 0; k < 20; k++)
            {
                try
                {
                    srlog.WriteLine("\n\n\n" + "Program Basladi ==> " + DateTime.Now);

                    Console.WriteLine("Program Basladi...>\n" + DateTime.Now);

                    List<string> MonitorFailList = new List<string>(); // This list holds the failed MonitorIPs
                    List<string> VentFailList = new List<string>();//This list hols failed VentIPs

                    // Sending ping to IP's
                    // If they are not reachable then add to FailList

                    Ping p = new Ping();
                    Console.WriteLine("\nMonitor IPlerine Ping Atılıyor...>\n\n");
                    foreach (var i in MonitorIpList)
                    {
                        PingReply pReply = p.Send(i, 100);
                        if (pReply.Status == IPStatus.Success)
                        {
                            Console.WriteLine("Ping başarılı: {0} adresine", i);
                        }
                        else
                        {
                            MonitorFailList.Add(i);

                            Console.WriteLine("Ping başarısız: {0} adresine", i);
                        }
                    }
                    srlog.WriteLine("\nMonitorler pinglendi...>" + DateTime.Now);

                    Console.WriteLine("\nMonitor IPlerine Ping Atıldı...>\n\n" + DateTime.Now);

                    Console.WriteLine("\nVentilator IPlerine Ping Atılıyor...>\n\n");

                    //sending ping to vent IPs 
                    foreach (var i in VentIPList)
                    {
                        PingReply pReply = p.Send(i, 100);
                        if (pReply.Status == IPStatus.Success)
                        {
                            Console.WriteLine("Ping başarılı: {0} adresine", i);
                        }
                        else
                        {
                            VentFailList.Add(i);

                            Console.WriteLine("Ping başarısız: {0} adresine", i);
                        }
                    }
                    Console.WriteLine("\nVentilator IPlerine Ping Atıldı...>\n\n" + DateTime.Now);



                    //Writing interrupted IPs to log.
                    srlog.WriteLine("\nInterrupted Monitors:");
                    foreach (var Item in MonitorFailList)
                    {
                        srlog.WriteLine(Item + "\n");
                    }
                    foreach (var Item in VentFailList)
                    {
                        srlog.WriteLine(Item + "\n");
                    }


                    if (File.Exists(path + "statisticsVent.txt") == true)
                    {
                        //getting number of trial from statistics.txt file
                        FileStream fsStat = new FileStream(path + "statisticsVent.txt", FileMode.OpenOrCreate, FileAccess.Read);
                        StreamReader srStat = new StreamReader(fsStat);
                        string lineV = srStat.ReadLine();
                        while (lineV != null)
                        {
                            if (lineV.Contains(".") == true)// If '.'  exist in line, that is the correct line for pulling IP and Counter
                            {
                                string[] dizi = new string[10];
                                dizi = lineV.Split("\t\t\t\t");  //Split the line with "\t\t\t" seperators 
                                total = Convert.ToInt32(dizi[2]);
                                total++;
                                Console.WriteLine("\nToplam Deneme Sayısı:{0}\n", total);
                                break;
                            }
                            lineV = srStat.ReadLine();
                        }
                        fsStat.Close();
                    }




                    //If FailLists are not empty then send e-mail
                    if (MonitorFailList.Count != 0 || VentFailList.Count != 0)
                    {

                        //Creating stat file for counting the Interrupt IPs


                        //read the text file and pull IPs and counters for adding dictionary
                        FileStream fsStat1 = new FileStream(path + "statisticsVent.txt", FileMode.OpenOrCreate, FileAccess.Read);
                        StreamReader srStat1 = new StreamReader(fsStat1);
                        string lne = srStat1.ReadLine();
                        while (lne != null)
                        {
                            if (lne.Contains(".") == true)// If '.' char exist in line, that is the correct line for pulling IP and Counter
                            {
                                string[] dizi = new string[100];
                                dizi = lne.Split("\t\t\t\t");  //Split the line with "\t\t\t" seperators 
                                string ip = dizi[0]; //First index of array holds the IP
                                string counter = dizi[1].Trim(); // Second index of array holds the counter
                                StatDict.Add(ip, Convert.ToInt32(counter));
                            }
                            lne = srStat1.ReadLine();
                        }
                        fsStat1.Close();

                        //increase counter value for Interrupt IPs
                        foreach (var i in VentFailList)
                        {
                            StatDict[i]++;
                        }
                        File.WriteAllText(path + "statisticsVent.txt", String.Empty); //clear the txt file before writing new counter values
                        FileStream fsStat2 = new FileStream(path + "statisticsVent.txt", FileMode.OpenOrCreate, FileAccess.Write);
                        StreamWriter swStat2 = new StreamWriter(fsStat2);
                        swStat2.WriteLine("IP\t\t\t\tInterruption Count\t\tNumber of Trial");
                        swStat2.WriteLine("---------------\t\t\t---------------\t\t\t---------------");
                        foreach (var entry in StatDict)
                        {
                            swStat2.WriteLine("{0}\t\t\t\t{1}\t\t\t\t{2}", entry.Key, entry.Value, total);
                        }
                        StatDict.Clear();
                        swStat2.Flush();
                        fsStat2.Close();



                        //same stat process for monitor IPs
                        FileStream fsMont = new FileStream(path + "statisticsMonitor.txt", FileMode.OpenOrCreate, FileAccess.Read);
                        StreamReader srMont = new StreamReader(fsMont);
                        string str = srMont.ReadLine();
                        while (str != null)
                        {
                            if (str.Contains(".") == true)// If '.' char exist in line, that is the correct line for pulling IP and Counter
                            {
                                string[] dizi = new string[100];
                                dizi = str.Split("\t\t\t\t");  //Split the line with "\t\t\t" seperators 
                                string ip = dizi[0]; //First index of array holds the IP
                                string counter = dizi[1].Trim(); // Second index of array holds the counter
                                StatDict.Add(ip, Convert.ToInt32(counter));
                            }
                            str = srMont.ReadLine();
                        }
                        fsMont.Close();

                        //increase counter value for Interrupt IPs
                        foreach (var i in MonitorFailList)
                        {
                            StatDict[i]++;
                        }
                        File.WriteAllText(path + "statisticsMonitor.txt", String.Empty); //clear the txt file before writing new counter values
                        FileStream fsMont2 = new FileStream(path + "statisticsMonitor.txt", FileMode.OpenOrCreate, FileAccess.Write);
                        StreamWriter swMont2 = new StreamWriter(fsMont2);
                        swMont2.WriteLine("IP\t\t\t\tInterruption Count\t\tNumber of Trial");
                        swMont2.WriteLine("---------------\t\t\t---------------\t\t\t---------------");
                        foreach (var entry in StatDict)
                        {
                            swMont2.WriteLine("{0}\t\t\t\t{1}\t\t\t\t{2}", entry.Key, entry.Value, total);
                        }
                        StatDict.Clear();
                        swMont2.Flush();
                        fsMont2.Close();




                        //Reading MailTime.txt's LastLine to pull LastMailTime
                        LastLine = File.ReadLines(path + "MailTime.txt").Last();
                        LastMailTime = Convert.ToDateTime(LastLine);


                        //sending e-mail every 2 hours
                        if (LastMailTime + new TimeSpan(2, 0, 0) < DateTime.Now)
                        {


                            // compare VentFailList with VentIPList to find IP,Room and Device
                            string bodyVent = "";
                            foreach (var i in VentFailList)
                            {
                                FileStream fsBody = new FileStream(path + "VentIPList.txt", FileMode.OpenOrCreate, FileAccess.Read);
                                StreamReader srBody = new StreamReader(fsBody);
                                string line = srBody.ReadLine();
                                while (line != null)
                                {
                                    if (line.Contains(".") == false)//escape to first line 
                                    {
                                        line = srBody.ReadLine();
                                        continue;
                                    }
                                    string[] array = new string[10];
                                    array = line.Split(",");
                                    if (i == array[0])// if fail IP matches add to bodyVent
                                    {
                                        bodyVent += "ODA:" + array[1] + "\tVENTİLATÖR:" + array[2] + "\t IP:" + array[0] + "\n";
                                    }
                                    line = srBody.ReadLine();
                                }
                            }
                            // compare MonitorFailList with MonitorIPList to find IP,Room and Device
                            string bodyMonitor = "";
                            foreach (var i in MonitorFailList)
                            {
                                FileStream fsBody = new FileStream(path + "monitorIpList.txt", FileMode.OpenOrCreate, FileAccess.Read);
                                StreamReader srBody = new StreamReader(fsBody);
                                string line = srBody.ReadLine();
                                while (line != null)
                                {
                                    if (line.Contains(".") == false)//escape to first line 
                                    {
                                        line = srBody.ReadLine();
                                        continue;
                                    }
                                    string[] array = new string[10];
                                    array = line.Split(",");
                                    if (i == array[0])// if fail IP matches add to bodyVent
                                    {
                                        bodyMonitor += "ODA:" + array[1] + "\tMONİTÖR:" + array[2] + "\t IP:" + array[0] + "\n";
                                    }
                                    line = srBody.ReadLine();
                                }
                            }
                            string body = bodyVent + bodyMonitor;
                            string subject= mailSubject + "\tVentilator-Monitor Baglantisi Kesildi";

                            bool check = obje.MailGonder(ePostaList, subject, body);
                            bool check1 = obje.CCMailGonder(ePostaListCC, subject, body);
                            if (check == true & check1 == true)
                            {
                                Console.Write("E-mailler Gönderildi==> " + DateTime.Now + "\n");
                                Console.WriteLine("2 saat bekleniyor...");
                                srlog.WriteLine("\nE-Mail Gönderildi==>" + DateTime.Now);
                                srlog.WriteLine("\n 2 saat bekleniyor...\n");
                                //Adding LastMailTime to MailTime.txt
                                fsMail = new FileStream(path + "MailTime.txt", FileMode.Append, FileAccess.Write);
                                swMail = new StreamWriter(fsMail);
                                swMail.WriteLine(DateTime.Now.ToString());//Adding LastMailTime
                                swMail.Flush();
                                fsMail.Close();
                            }
                            else if (check == true && check1 == false)
                            {
                                Console.WriteLine("TO E-mail Gönderildi ==>" + DateTime.Now);
                                Console.WriteLine("2 saat bekleniyor...");
                                srlog.WriteLine("\n TO E-Mail Gönderildi==>" + DateTime.Now);
                                srlog.WriteLine("\n 2 saat bekleniyor...\n");
                                //Adding LastMailTime to MailTime.txt
                                fsMail = new FileStream(path + "MailTime.txt", FileMode.Append, FileAccess.Write);
                                swMail = new StreamWriter(fsMail);
                                swMail.WriteLine(DateTime.Now.ToString());//Adding LastMailTime
                                swMail.Flush();
                                fsMail.Close();
                            }
                            else if (check1 == true && check == false)
                            {
                                Console.WriteLine("CC E-mail Gönderildi ==>" + DateTime.Now);
                                Console.WriteLine("2 saat bekleniyor...");
                                srlog.WriteLine("\n CC E-Mail Gönderildi==>" + DateTime.Now);
                                srlog.WriteLine("\n 2 saat bekleniyor...\n");
                                //Adding LastMailTime to MailTime.txt
                                fsMail = new FileStream(path + "MailTime.txt", FileMode.Append, FileAccess.Write);
                                swMail = new StreamWriter(fsMail);
                                swMail.WriteLine(DateTime.Now.ToString());//Adding LastMailTime
                                swMail.Flush();
                                fsMail.Close();
                            }
                            else
                            {
                                Console.WriteLine("Email Gönderilemedi==>" + DateTime.Now);
                                srlog.Write("\n" + "E-Mail Gönderilemedi==>" + DateTime.Now);
                            }
                        }
                    }
                    // means all connections are fine
                    else
                    {
                        Console.WriteLine("Baglanti problemi yok. Email gonderilmedi. " + DateTime.Now);
                        srlog.WriteLine("Baglanti problemi yok. Email gonderilmedi." + DateTime.Now);
                    }

                    srlog.Flush();

                }
                catch (Exception E)
                {
                    c++;
                    if (c < 10)
                    {
                        Console.WriteLine("Hata ===>\n {0}", E);
                        srlog.WriteLine("Döngü içinde HATA!!! {0}", E);
                        continue;
                    }
                    else
                    {
                        srlog.WriteLine("Program tıkandı.\nYeniden başlatılıyor.");
                        break;
                    }
                }
            }
            Thread.Sleep(Interval);
        }
    }
}
