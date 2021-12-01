using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using WindowsInput;
using WindowsInput.Native;
using System.Xml;
using System.IO;

namespace CST_Automation
{
    public class Program
    {
        public static void Main()
        {
            bool programRunning = true;

            var f = new Functions();

            if (f.UIComplete == false)
            {
                f.UI();
            }
            
            if (f.UIComplete == true)
            {
                Console.WriteLine("Checking for save file...\n");
                f.CreateSaveFile();
                f.LoadData();
                Console.WriteLine("Flushing leftover reports...\n");
                f.DetectAndDeleteReports();
                Thread.Sleep(1000);
                Console.WriteLine("Starting program...\n");

                while (programRunning == true)
                {
                    f.ControllerFunc();
                    Thread.Sleep(500);
                }
            }

            else
            {
                Console.WriteLine("Make sure all fields are complete!\n \n");
                Main();
            }
        }
    }
    public class Functions
    {
        string brEmail;
        string brPassword;
        string cstEmail;
        string cstPassword;
        string filePathString;
        string userName;

        int cycleNumber = -1;

        public bool UIComplete = false;
        bool newComputerDetected = false;
        bool calledGetBRData = false;
        bool calledConvertData = false;
        bool calledSendToCST = false;
        bool getBRDataFinished = false;
        bool convertDataFinished = false;
        bool chromeBookCSTFinished = false;
        bool sentChromebook = false;

        ChromeOptions options = new ChromeOptions();

        List<ComputerInformation> computerInfoList = new List<ComputerInformation>();
        List<ChromebookInformation> chromeBInfoList = new List<ChromebookInformation>();
        List<string> reportIDList = new List<string>();
        public void ControllerFunc()
        {
            if(calledGetBRData == false)
            {
                try
                {
                    GetBRData();
                }
                catch
                {
                    reportIDList.Clear();
                    Console.Clear();
                    LoadData();

                    calledGetBRData = false;
                    getBRDataFinished = false;
                    ControllerFunc();
                }
            }

            if(getBRDataFinished == true && calledConvertData == false)
            {
                try
                {
                    ConvertData();
                }
                catch
                {
                    Console.Clear();
                    calledConvertData = false;
                    ControllerFunc();
                }
            }

            if(convertDataFinished == true && calledSendToCST == false)
            {
                try
                {
                    SendToCST();
                }
                catch
                {
                    Console.Clear();
                    calledSendToCST = false;
                    convertDataFinished = true;
                    ControllerFunc();
                }
            }
        }
        public async void SaveData()
        {
            string[] originalList = reportIDList.ToArray();
            string[] compareList = File.ReadAllLines(filePathString);

            using StreamWriter file = new StreamWriter(filePathString, append: true);

            foreach(string ID in originalList)
            {
                if (!compareList.Contains(ID))
                {
                     await file.WriteLineAsync(ID);
                }
            }
        }
        public void CreateSaveFile()
        {
            userName = Environment.UserName;
            filePathString = @"C:\Users\" + userName + @"\Desktop\DONOTDELETE_ReportSave.txt";
            if (!File.Exists(filePathString))
            {
                Console.WriteLine("Creating save file...\n");
                var file = File.Create(filePathString);
                file.Close();
            }
        }
        public async void LoadData()
        {
            Console.WriteLine("Loading save file...\n");

            string[] originalList = await File.ReadAllLinesAsync(filePathString);

            reportIDList = originalList.ToList();
        }
        public void UI()
        {
            Console.WriteLine("CST Automation");
            Console.WriteLine("");

            Console.WriteLine("Press \"ENTER\" To Continue");
            Console.ReadLine();

            Console.WriteLine("Please enter your BitRaser email below");
            brEmail = Console.ReadLine();
            Console.WriteLine("");

            Console.WriteLine("Please enter your BitRaser password below");
            brPassword = Console.ReadLine();
            Console.WriteLine("");

            Console.WriteLine("Please enter your CST email");
            cstEmail = Console.ReadLine();
            Console.WriteLine("");

            Console.WriteLine("Please enter your CST password");
            cstPassword = Console.ReadLine();
            Console.Clear();
            
            if((brEmail != "") && (brPassword != "") && (cstEmail != "") && (cstPassword != ""))
            {
                UIComplete = true;
            }
        }
        public void GetBRData()
        {
            calledGetBRData = true;
            int iNumber = 1;
            cycleNumber++;

            ComputerInformation computer = new ComputerInformation();
            InputSimulator sim = new InputSimulator();
            string btRaserLink = "https://bitrasercloud.com/mc-cloud-Authentication/Path000/00000/";
            
            IWebDriver webDriver = new ChromeDriver(options);

            Console.Clear();

            Console.WriteLine("\nLogging in to BitRaser...\n");

            webDriver.Navigate().GoToUrl(btRaserLink);

            webDriver.FindElement(By.XPath("/html/body/div[1]/div[2]/form/div[1]/div[1]/div/input")).SendKeys(brEmail);
            webDriver.FindElement(By.XPath("/html/body/div[1]/div[2]/form/div[1]/div[2]/div/input")).SendKeys(brPassword);
            webDriver.FindElement(By.XPath("/html/body/div[1]/div[2]/form/div[2]/div[2]/button")).Click();

            Thread.Sleep(1000);

            while ((webDriver.Url == "https://bitrasercloud.com/mc-cloud-Authentication/Path000/00000/index.php") || (webDriver.Url == "https://bitrasercloud.com/mc-cloud-Authentication/Path000/00000/index.php?m=1"))
            {
                webDriver.Quit();
                Console.Clear();
                Console.WriteLine("ERROR: Login to BitRaser failed.\nPress \"ENTER\" to continue.\n");
                Console.ReadLine();
                Console.WriteLine("Please enter your BitRaser email below");
                brEmail = Console.ReadLine();
                Console.WriteLine("");
                Console.WriteLine("Your email is " + brEmail + "\n");

                Console.WriteLine("Please enter your BitRaser password below");
                brPassword = Console.ReadLine();
                Console.WriteLine("");
                Console.WriteLine("Your password is " + brPassword + "\n");
                Console.WriteLine("Press \"ENTER\" to confirm.");
                Console.ReadLine();

                Console.WriteLine("\nLogging in to BitRaser...\n");
                webDriver = new ChromeDriver(options);

                webDriver.Navigate().GoToUrl(btRaserLink);

                webDriver.FindElement(By.XPath("/html/body/div[1]/div[2]/form/div[1]/div[1]/div/input")).SendKeys(brEmail);
                webDriver.FindElement(By.XPath("/html/body/div[1]/div[2]/form/div[1]/div[2]/div/input")).SendKeys(brPassword);
                webDriver.FindElement(By.XPath("/html/body/div[1]/div[2]/form/div[2]/div[2]/button")).Click();

                Thread.Sleep(1000);
            }

            webDriver.FindElement(By.XPath("/html/body/div[6]/div[2]/nav/div/div[2]/ul/li[3]")).Click();

            Console.WriteLine("Finding computer...\n");

            while (newComputerDetected == false)
            {
                webDriver.Navigate().Refresh();

                string rowAmount;

                try
                {
                    rowAmount = webDriver.FindElement(By.XPath("/html/body/div[6]/div[2]/div[1]/div/div/div[4]/table/tbody/tr[100]/td[2]")).Text;
                }
                catch
                {
                    try
                    {
                        rowAmount = webDriver.FindElement(By.XPath("/html/body/div[6]/div[2]/div[1]/div/div/div[4]/table/tbody/tr[50]/td[2]")).Text;
                    }
                    catch
                    {
                        try
                        {
                            rowAmount = webDriver.FindElement(By.XPath("/html/body/div[6]/div[2]/div[1]/div/div/div[4]/table/tbody/tr[25]/td[2]")).Text;
                        }
                        catch
                        {
                            rowAmount = "15";
                        }
                    }
                }

                int realRowAmount = Convert.ToInt32(rowAmount);

                for (int i = realRowAmount; i > 0; i--)
                {
                    string reportID = webDriver.FindElement(By.XPath("/html/body/div[6]/div[2]/div[1]/div/div/div[4]/table/tbody/tr[" + i + "]/td[4]")).Text;
                    string status = webDriver.FindElement(By.XPath("/html/body/div[6]/div[2]/div[1]/div/div/div[4]/table/tbody/tr["+ i + "]/td[9]/font")).Text;

                    bool alreadyDone = reportIDList.Contains(reportID);

                    if (alreadyDone == false && status == "Erased")
                    {
                        newComputerDetected = true;
                        reportIDList.Add(reportID);
                        iNumber = i;
                        Console.WriteLine("Computer detected...\n");
                        break;
                    }

                    Thread.Sleep(25);
                }
            }

            if (newComputerDetected == true)
            {
                Console.WriteLine("Retrieving report...\n");

                webDriver.FindElement(By.XPath("/html/body/div[6]/div[2]/div[1]/div/div/div[4]/table/tbody/tr[" + iNumber + "]/td[1]/div/input")).Click();
                webDriver.FindElement(By.XPath("/html/body/div[6]/div[2]/div[1]/div/div/div[5]/div/button[1]")).Click();

                Thread.Sleep(1000);

                webDriver.FindElement(By.XPath("/html/body/div[6]/div[2]/div[1]/div/div/div[4]/div[1]/div/div/div[2]/div[1]/input")).Click();
                webDriver.FindElement(By.XPath("/html/body/div[6]/div[2]/div[1]/div/div/div[4]/div[1]/div/div/div[2]/div[4]/input")).SendKeys(Convert.ToString(0));
                webDriver.FindElement(By.XPath("/html/body/div[6]/div[2]/div[1]/div/div/div[4]/div[1]/div/div/div[2]/button")).Click();

                Thread.Sleep(1250);

                Actions builder = new Actions(webDriver);
                string tabCountString = webDriver.FindElement(By.XPath("/html/body/div[6]/div[2]/div[1]/div/div/nav/ul/li[1]/a")).Text;
                tabCountString = tabCountString.Remove(0, 6);
                int tabCountRef = Convert.ToInt32(tabCountString);
                int tabCount;

                if (tabCountRef > 10)
                {
                    tabCount = 20;
                }

                else
                {
                    tabCount = 10 + (Convert.ToInt32(tabCountString));
                }

                for (int i = 0; i < tabCount; i++)
                {
                    sim.Keyboard.KeyDown(VirtualKeyCode.TAB);
                    Thread.Sleep(10);
                }

                sim.Keyboard.KeyDown(VirtualKeyCode.RETURN);
           
                Thread.Sleep(500);

                webDriver.Quit();

                getBRDataFinished = true;
            }
        }
        public void ConvertData()
        {
            calledConvertData = true;
            Console.Clear();

            string responseToManual;
            string storageString;
            string batteryStatus;
            

            XmlDocument report = new XmlDocument();
            try
            {
                report.Load(@"C:\Users\" + userName + @"\Downloads\" + 0 + "-BitRaserReport.xml");
            }
            catch
            {
                reportIDList.Clear();
                LoadData();

                calledGetBRData = false;
                newComputerDetected = false;
                getBRDataFinished = false;
                calledConvertData = false;
                convertDataFinished = false;
                calledSendToCST = false;

                ControllerFunc();
            }
            ComputerInformation computer = new ComputerInformation();

            computer.condition = "";

            Console.WriteLine("Converting report data...\n");

            try
            {
                storageString = report.SelectSingleNode("/BitRaser_Report/Components[15]/Components[@name=\"Disk0\"]//Component[5]").InnerText;
            }

            catch (NullReferenceException e)
            {
                storageString = report.SelectSingleNode("/BitRaser_Report/Components[16]/Components[@name=\"Disk0\"]//Component[5]").InnerText;
            }

            storageString = storageString.Remove(3);

            string ramString = report.SelectSingleNode("/BitRaser_Report/Components[5]//Component[8]").InnerText;

            if (ramString.Length == 3)
            {
                ramString = ramString.Remove(1);
            }
            else if (ramString.Length == 4)
            {
                ramString = ramString.Remove(2);
            }

            int storage = Convert.ToInt32(storageString);
            int realStorage = 0;

            if(storage >= 450)
            {
                realStorage = 500;
            }
            else if (storage >= 200)
            {
                realStorage = 256;
            }
            else if (storage >= 100)
            {
                realStorage = 128;
            }

            computer.serialNum = report.SelectSingleNode("/BitRaser_Report/Components[5]//Component[5]").InnerText;
            computer.model = report.SelectSingleNode("/BitRaser_Report/Components[5]//Component[3]").InnerText;
            computer.ramAmnt = ramString;
            computer.storageAmnt = Convert.ToString(realStorage);
            computer.processor = report.SelectSingleNode("/BitRaser_Report/Components[7]/Components[@name=\"Processor\"]//Component[2]").InnerText;

            try
            {
                computer.condition = (report.SelectSingleNode("/BitRaser_Report/Components[2]//Component[1]").InnerText).ToUpper();
            }
            catch
            {
                computer.condition = "";
            }

            string motherboardTest = report.SelectSingleNode("/BitRaser_Report/Components[4]//Component[1]").InnerText;
            string processorTest = report.SelectSingleNode("/BitRaser_Report/Components[4]//Component[2]").InnerText;
            string memoryTest = report.SelectSingleNode("/BitRaser_Report/Components[4]//Component[3]").InnerText;
            string diskTest = report.SelectSingleNode("/BitRaser_Report/Components[6]/Components[@name=\"Disk0\"]//Component[3]").InnerText;

            try
            {
                batteryStatus = report.SelectSingleNode("/BitRaser_Report/Components[4]//Component[12]").InnerText;
            }
            catch
            {
                batteryStatus = "Successful";
            }

            if((motherboardTest != "Successful") || (processorTest != "Successful") || (memoryTest != "Successful") || (diskTest != "PASSED") || (batteryStatus != "Successful"))
            {
                Console.Clear();
                Console.WriteLine("WARNING: A key component of S/N: " + computer.serialNum + " has not passed BitRaser diagnostics");
                Console.WriteLine("Summary: \n Motherboard Test: " + motherboardTest + "\n Processor Test: " + processorTest + "\n Memory Test: " + memoryTest + "\n Hard Drive Test: " + diskTest + "\n Battery Test: " + batteryStatus + "\n");

                Console.WriteLine("The computer has been erased; however, it will need to be repaired and manually entered into CST");
                Console.WriteLine("\n Press ENTER to continue.");
                Console.ReadLine();

                Console.WriteLine("Deleting report file...\n");
                File.Delete(@"C:\Users\" + userName + @"\Downloads\" + 0 + "-BitRaserReport.xml");
                SaveData();

                Console.Clear();

                calledGetBRData = false;
                newComputerDetected = false;
                getBRDataFinished = false;
                calledConvertData = false;
                convertDataFinished = false;
                calledSendToCST = false;

                Program.Main();
            }

            Console.Clear();
            Console.WriteLine("Hit \"ENTER\" to continue to mandatory UI.");
            responseToManual = Console.ReadLine();
            Console.Clear();

            if(responseToManual == "Q")
            {
                Environment.Exit(0);
            }

            if (responseToManual == "M")
            {
                reportIDList.Clear();
                ChromebookInformation chromebook = new ChromebookInformation();
                Console.Clear();

                Console.WriteLine("Initializing Chromebook Input UI...\n");
                Thread.Sleep(1000);
                Console.Clear();

                Console.WriteLine("Enter the Chromebook's S/N below:");
                chromebook.serialNum = Console.ReadLine(); Console.WriteLine("");
                Console.WriteLine("Enter the Chromebook's condition below:");
                chromebook.condition = Console.ReadLine(); Console.WriteLine("");
                chromebook.condition = chromebook.condition.ToUpper();

                while ((chromebook.condition.Length > 1) || ((!chromebook.condition.Contains("A")) && (!chromebook.condition.Contains("B")) && (!chromebook.condition.Contains("C")) && (!chromebook.condition.Contains("D")) && (!chromebook.condition.Contains("F"))))
                {
                    Console.WriteLine("Your input is either invalid or contains too many characters.");
                    Console.WriteLine("Please re-enter condition.\n");
                    chromebook.condition = Console.ReadLine();
                    chromebook.condition = chromebook.condition.ToUpper();
                }

                Console.WriteLine("Enter the Chromebook model below:");
                chromebook.model = Console.ReadLine(); Console.WriteLine("");
                Console.WriteLine("Press \"ENTER\" to confirm submission to CST.");
                Console.ReadLine();

                Console.Clear();

                chromeBInfoList.Add(chromebook);

                while(sentChromebook == false)
                {
                    try
                    {
                        SendChromebookCST();
                    }
                    catch
                    {
                        sentChromebook = false;
                    }
                    Thread.Sleep(500);
                }

                if (chromeBookCSTFinished == true)
                {
                    Console.WriteLine("Deleting report file...\n");
                    File.Delete(@"C:\Users\" + userName + @"\Downloads\" + 0 + "-BitRaserReport.xml");
                    chromeBInfoList.Clear();
                    LoadData();
                    Console.Clear();

                    calledGetBRData = false;
                    newComputerDetected = false;
                    getBRDataFinished = false;
                    calledConvertData = false;
                    convertDataFinished = false;
                    calledSendToCST = false;
                    chromeBookCSTFinished = false;

                    ControllerFunc();
                }
            }

            if ((computer.condition.Length > 1) || ((!computer.condition.Contains("A")) && (!computer.condition.Contains("B")) && (!computer.condition.Contains("C")) && (!computer.condition.Contains("D")) && (!computer.condition.Contains("F"))))
            {
                Console.WriteLine("What is the condition of S/N: " + computer.serialNum + "\n Model: " + computer.model + "\n RAM: " + computer.ramAmnt + " GB" + "\n Storage: " + computer.storageAmnt + " GB\n");
                computer.condition = Console.ReadLine();
                computer.condition = computer.condition.ToUpper();

                while ((computer.condition.Length > 1) || ((!computer.condition.Contains("A")) && (!computer.condition.Contains("B")) && (!computer.condition.Contains("C")) && (!computer.condition.Contains("D")) && (!computer.condition.Contains("F"))))
                {
                    Console.WriteLine("Your input is either invalid or contains too many characters.");
                    Console.WriteLine("Please re-enter condition.\n");
                    computer.condition = Console.ReadLine();
                    computer.condition = computer.condition.ToUpper();
                }
            }

            Console.WriteLine("\nWas it encrypted? (Y/N)\n");
            computer.encrypted = Console.ReadLine();
            computer.encrypted = computer.encrypted.ToUpper();

            while((computer.encrypted.Length > 1) || ((!computer.encrypted.Contains("Y")) && (!computer.encrypted.Contains("N"))))
            {
                Console.WriteLine("Your input is either invalid or contains too many characters.\nWas it encrypted (Y/N)\n");
                computer.encrypted = Console.ReadLine();
                computer.encrypted = computer.encrypted.ToUpper();
            }

            Console.Clear();

            computerInfoList.Add(computer);

            convertDataFinished = true;
        }
        public void SendToCST()
        {
            calledSendToCST = true;
            var computer = computerInfoList[0];

            Console.WriteLine("Sending to CST... \n");

            IWebDriver webDriver = new ChromeDriver();
            string cstLink = "https://w16kcst2.int.hp.com/";

            webDriver.Navigate().GoToUrl(cstLink);

            Thread.Sleep(2000);

            webDriver.FindElement(By.XPath("/html/body/section[2]/div/div[1]/div/div/div[1]/form/div[1]/div/input")).SendKeys(cstEmail);
            webDriver.FindElement(By.XPath("/html/body/section[2]/div/div[1]/div/div/div[1]/form/div[2]/div/input")).SendKeys(cstPassword);
            webDriver.FindElement(By.XPath("/html/body/section[2]/div/div[1]/div/div/div[1]/form/div[4]/button")).Click();

            Thread.Sleep(2000);

            while (webDriver.Url == "https://w16kcst2.int.hp.com/")
            {
                webDriver.Quit();
                Console.Clear();
                Console.WriteLine("ERROR: Login to CST failed.\nPress \"ENTER\" to continue.\n");
                Console.ReadLine();

                Console.WriteLine("Please enter your CST email");
                cstEmail = Console.ReadLine();
                Console.WriteLine("");
                Console.WriteLine("Your CST email is " + cstEmail + "\n");

                Console.WriteLine("Please enter your CST password");
                cstPassword = Console.ReadLine();
                Console.WriteLine("");
                Console.WriteLine("Your CST password is " + cstPassword + "\n");

                Console.WriteLine("Press \"ENTER\" to confirm.");
                Console.ReadLine();

                Console.WriteLine("\nLogging in to CST...\n");
                webDriver = new ChromeDriver(options);

                webDriver.Navigate().GoToUrl(cstLink);

                Thread.Sleep(2000);

                webDriver.FindElement(By.XPath("/html/body/section[2]/div/div[1]/div/div/div[1]/form/div[1]/div/input")).SendKeys(cstEmail);
                webDriver.FindElement(By.XPath("/html/body/section[2]/div/div[1]/div/div/div[1]/form/div[2]/div/input")).SendKeys(cstPassword);
                webDriver.FindElement(By.XPath("/html/body/section[2]/div/div[1]/div/div/div[1]/form/div[4]/button")).Click();

                Thread.Sleep(2000);
            }
            
            Console.WriteLine("Sent Computer with S/N: " + computer.serialNum + " to CST.\n");

            Console.WriteLine("Deleting report file...\n");
            computerInfoList.Clear();
            File.Delete(@"C:\Users\" + userName + @"\Downloads\" + 0 + "-BitRaserReport.xml");
            SaveData();

            Console.Clear();
            Thread.Sleep(500);

            calledGetBRData = false;
            newComputerDetected = false;
            getBRDataFinished = false;
            calledConvertData = false;
            convertDataFinished = false;
            calledSendToCST = false;
        }

        public void SendChromebookCST()
        {
            sentChromebook = true;
            ChromebookInformation chromebook = chromeBInfoList[0];

            Console.WriteLine("Sending Chromebook to CST... \n");

            IWebDriver webDriver = new ChromeDriver();
            string cstLink = "https://w16kcst2.int.hp.com/";

            webDriver.Navigate().GoToUrl(cstLink);

            Thread.Sleep(2000);

            webDriver.FindElement(By.XPath("/html/body/section[2]/div/div[1]/div/div/div[1]/form/div[1]/div/input")).SendKeys(cstEmail);
            webDriver.FindElement(By.XPath("/html/body/section[2]/div/div[1]/div/div/div[1]/form/div[2]/div/input")).SendKeys(cstPassword);
            webDriver.FindElement(By.XPath("/html/body/section[2]/div/div[1]/div/div/div[1]/form/div[4]/button")).Click();

            Thread.Sleep(2000);

            while (webDriver.Url == "https://w16kcst2.int.hp.com/")
            {
                webDriver.Quit();

                Console.Clear();
                Console.WriteLine("ERROR: Login to CST failed.\nPress \"ENTER\" to continue.\n");
                Console.ReadLine();

                Console.WriteLine("Please enter your CST email");
                cstEmail = Console.ReadLine();
                Console.WriteLine("");
                Console.WriteLine("Your CST email is " + cstEmail + "\n");

                Console.WriteLine("Please enter your CST password");
                cstPassword = Console.ReadLine();
                Console.WriteLine("");
                Console.WriteLine("Your CST password is " + cstPassword + "\n");

                Console.WriteLine("Press \"ENTER\" to confirm.");
                Console.ReadLine();

                Console.WriteLine("\nLogging in to CST...\n");
                webDriver = new ChromeDriver(options);

                webDriver.Navigate().GoToUrl(cstLink);

                Thread.Sleep(2000);

                webDriver.FindElement(By.XPath("/html/body/section[2]/div/div[1]/div/div/div[1]/form/div[1]/div/input")).SendKeys(cstEmail);
                webDriver.FindElement(By.XPath("/html/body/section[2]/div/div[1]/div/div/div[1]/form/div[2]/div/input")).SendKeys(cstPassword);
                webDriver.FindElement(By.XPath("/html/body/section[2]/div/div[1]/div/div/div[1]/form/div[4]/button")).Click();

                Thread.Sleep(2000);
            }

            Console.WriteLine("Sent Chromebook with S/N: " + chromebook.serialNum + " to CST.\n");
            Thread.Sleep(1500);
            Console.Clear();
            chromeBookCSTFinished = true;
        }
        public void DetectAndDeleteReports()
        {
            for (int i = 5; i > -1; i--)
            {
                if (File.Exists(@"C:\Users\" + userName + @"\Downloads\" + i + "-BitRaserReport.xml"))
                {
                    File.Delete(@"C:\Users\" + userName + @"\Downloads\" + i + "-BitRaserReport.xml");
                    Console.WriteLine("Deleted: " + i + "-BitRaserReport.xml \n");
                }

                Thread.Sleep(250);
            }
        }
    }
    public class ComputerInformation
    {
        public string serialNum;
        public string model;
        public string ramAmnt;
        public string storageAmnt;
        public string condition;
        public string processor;
        public string encrypted;
    }
    public class ChromebookInformation
    {
        public string serialNum;
        public string model;
        public string condition;
    }
}
