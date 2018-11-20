﻿
using System.Net;
using System.IO;
using Microsoft.Win32;
//using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using System.Collections.Generic;
using System;

using System.Runtime.InteropServices;
using Autodesk.AutoCAD.Colors;
//using AcAp = Autodesk.AutoCAD.ApplicationServices.Application;

using Autodesk.AutoCAD.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace myCustomCmds
{
    public class CheckLicense : Autodesk.AutoCAD.Runtime.IExtensionApplication
    {
        public static bool licensed = false;

        public void Initialize()
        {
            getLicense();
        }

        public void getLicense()
        {
            try
            {
                if (CheckForInternetConnection())
                {
                    using (var client = new WebClient())
                    {


                        var url = "https://textuploader.com/dyx30/raw";
                        string textFromFile = (new WebClient()).DownloadString(url);

                        string myProductId = "5531";
                        string myText2Write = "@%^&(!";
                        if (textFromFile.Contains("xyz"))
                        {
                            licensed = true;       
                        }
                        else
                        {
                            licensed = false;
                            Application.ShowAlertDialog("Một số command không được tiếp tục hỗ trợ.");
                        }

                    }
                }
                else
                {
                    licensed = false;
                    Application.ShowAlertDialog("Có thể đã có lỗi xảy ra.");
                }
            }

            catch
            {
                Application.ShowAlertDialog("Có thể đã có lỗi xảy ra.");          
            }
        }

        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://clients3.google.com/generate_204"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }


        public void Terminate()
        {
            Console.WriteLine("Cleaning up...");
        }
        //[CommandMethod("TST")]
        //public void Test()
        //{
        //    Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
        //    ed.WriteMessage("This is the TST command.");
        //}

    }
}