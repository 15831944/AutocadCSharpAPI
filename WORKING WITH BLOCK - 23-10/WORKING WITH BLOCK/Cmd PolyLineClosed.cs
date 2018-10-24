﻿using Autodesk.AutoCAD.Runtime;
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

/// This code requires a reference to AcExportLayoutEx.dll:
//using System.Threading;


using commonFunctions;

namespace myCustomCmds
{
    public class DimPoly
    {
        // Ham polyline

        //public static List<Point3d> getMinMaxPoint(this Polyline myPolyline)
        //{
        //    // lay minpoint cua extent border
        //    //Point3d myMinPoint = myPolyline.GeometricExtents.MinPoint;
        //    //Point3d myMaxPoint = myPolyline.GeometricExtents.MaxPoint;

        //    if (myPolyline.NumberOfVertices < 4 || myPolyline.Area <=0)
        //    {
        //        Application.ShowAlertDialog("Duong poly khong hop le");
        //        return null;
        //    }

        //    List<Point3d> myAllPointOfPoly = new List<Point3d>();
        //    List<Point3d> my4Point = new List<Point3d>();

        //    for (int i = 0; i < myPolyline.NumberOfVertices; i++)
        //    {
        //        myAllPointOfPoly.Add(myPolyline.GetPoint3dAt(i));
        //    }

        //    // Lay minmax X cua poly
        //    myAllPointOfPoly.Sort(sortByX);
        //    Point3d myMinPointX = myAllPointOfPoly[0];
        //    Point3d myMaxPointX = myAllPointOfPoly[myAllPointOfPoly.Count-1];

        //    // Lay minmax Y cua poly
        //    myAllPointOfPoly.Sort(sortByY);

        //    Point3d myMinPoinY = myAllPointOfPoly[0];
        //    Point3d myMaxPointY = myAllPointOfPoly[myAllPointOfPoly.Count - 1];

        //    my4Point.Add(myMinPointX);
        //    my4Point.Add(myMaxPointX);
        //    my4Point.Add(myMinPoinY);
        //    my4Point.Add(myMaxPointY);

        //    return my4Point;
        //}


        //HAM Poly:
        [CommandMethod("PD")]
        public static void autoDimPolyBySide()
        {
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Application.DocumentManager.MdiActiveDocument.Database.Orthomode = false;
            Database acCurDb = acDoc.Database;

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {

                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                                                OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                OpenMode.ForWrite) as BlockTableRecord;

                double scaleCurrentDim = acCurDb.GetDimstyleData().Dimscale;


                ObjectId myObjId = myCustomFunctions.GetObjectIdByType("POLYLINE,LWPOLYLINE");

                if (myObjId.ToString() == "0") return ;
                if (myObjId == new ObjectId()) return ;

                Polyline myPolySelected = myObjId.GetObject(OpenMode.ForWrite) as Polyline;

                if (myPolySelected.NumberOfVertices < 2) return;

                if (myPolySelected.Area == 0) return;

                myPolySelected.Closed = true;

                // Pick side to insert
                // Chọn 1 diem tren man hinh de pick insert
                PromptPointResult pPtRes;
                PromptPointOptions pPtOpts = new PromptPointOptions("");

                // Prompt for the start point
                pPtOpts.Message = "\nPick a point to place Dim: ";
                pPtRes = acDoc.Editor.GetPoint(pPtOpts);

                // Exit if the user presses ESC or cancels the command
                if (pPtRes.Status == PromptStatus.Cancel) return;
                Point3d ptPositionInsert = pPtRes.Value;


                
                Point3d minPoint = new Point3d();
                Point3d maxPoint = new Point3d();

                bool dimDirectX = true;

                if (ptPositionInsert.Y <= myPolySelected.getMinMaxPoint()[2].Y || ptPositionInsert.Y >= myPolySelected.getMinMaxPoint()[3].Y)
                {
                    minPoint = myPolySelected.getMinMaxPoint()[0];
                    maxPoint = myPolySelected.getMinMaxPoint()[1];
                    dimDirectX = true;
                }
                else
                {
                    minPoint = myPolySelected.getMinMaxPoint()[2];
                    maxPoint = myPolySelected.getMinMaxPoint()[3];
                    dimDirectX = false;
                }

                // Lay tap hop diem

                bool sidePick = isLeftOrAbove(minPoint, maxPoint, ptPositionInsert);

                List<Point3d> listPointToDim = new List<Point3d>();
                listPointToDim.Add(minPoint);
                listPointToDim.Add(maxPoint);

                for (int i = 0; i < myPolySelected.NumberOfVertices; i++)
                {
                    Point3d myPointCheck = myPolySelected.GetPoint3dAt(i);     

                        // Neu cung side thi them vao database
                    if (isLeftOrAbove(minPoint, maxPoint, myPointCheck) == sidePick)
                        {
                            listPointToDim.Add(myPointCheck);

                        }
                }

                if (dimDirectX)
                {
                    listPointToDim.Sort(sortByX);
                    CmdDim.autoDimHorizontalNotSelect(listPointToDim, ptPositionInsert);
                }
                else
                {
                    listPointToDim.Sort(sortByY);
                    CmdDim.autoDimVerticalNotSelect(listPointToDim, ptPositionInsert);
                }

                    acTrans.Commit();
            }
        }

        /// <summary>
        /// 
        /// </summary>

        //[CommandMethod("APD")]
        //public static void autoDimPolyByClick()
        //{
        //    // Get the current document and database
        //    Document acDoc = Application.DocumentManager.MdiActiveDocument;
        //    Application.DocumentManager.MdiActiveDocument.Database.Orthomode = false;
        //    Database acCurDb = acDoc.Database;

        //    // Start a transaction
        //    using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
        //    {

        //        // Open the Block table for read
        //        BlockTable acBlkTbl;
        //        acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
        //                                        OpenMode.ForRead) as BlockTable;

        //        // Open the Block table record Model space for write
        //        BlockTableRecord acBlkTblRec;
        //        acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
        //                                        OpenMode.ForWrite) as BlockTableRecord;

        //        double scaleCurrentDim = acCurDb.GetDimstyleData().Dimscale;


        //        ObjectId myObjId = myCustomFunctions.GetObjectIdByType("POLYLINE,LWPOLYLINE");

        //        if (myObjId.ToString() == "0") return;
        //        if (myObjId == new ObjectId()) return;

        //        Polyline myPolySelected = myObjId.GetObject(OpenMode.ForWrite) as Polyline;

        //        if (myPolySelected.NumberOfVertices < 2) return;

        //        if (myPolySelected.Area == 0) return;

        //        myPolySelected.Closed = true;

        //        //// Pick side to insert
        //        //// Chọn 1 diem tren man hinh de pick insert
        //        //PromptPointResult pPtRes;
        //        //PromptPointOptions pPtOpts = new PromptPointOptions("");

        //        //// Prompt for the start point
        //        //pPtOpts.Message = "\nPick a point to place Dim: ";
        //        //pPtRes = acDoc.Editor.GetPoint(pPtOpts);

        //        //// Exit if the user presses ESC or cancels the command
        //        //if (pPtRes.Status == PromptStatus.Cancel) return;
        //        //Point3d ptPositionInsert = pPtRes.Value;
        //        Point3d ptPositionInsert = new Point3d();

        //        List<Point3d> myListPositionDim = new List<Point3d>();

        //        Point3d pDp1 = new Point3d(myPolySelected.getMinMaxPoint()[0].X - scaleCurrentDim*10,
        //            myPolySelected.getMinMaxPoint()[0].Y, 0);

        //        Point3d pDp2 = new Point3d(myPolySelected.getMinMaxPoint()[1].X + scaleCurrentDim * 10,
        //            myPolySelected.getMinMaxPoint()[1].Y, 0);

        //        Point3d pDp3 = new Point3d(myPolySelected.getMinMaxPoint()[2].X,
        //            myPolySelected.getMinMaxPoint()[2].Y - scaleCurrentDim * 10, 0);

        //        Point3d pDp4 = new Point3d(myPolySelected.getMinMaxPoint()[3].X,
        //            myPolySelected.getMinMaxPoint()[3].Y + scaleCurrentDim * 10, 0);

        //        myListPositionDim.Add(pDp1);
        //        myListPositionDim.Add(pDp2);
        //        myListPositionDim.Add(pDp3);
        //        myListPositionDim.Add(pDp4);


        //        Point3d minPoint = new Point3d();
        //        Point3d maxPoint = new Point3d();

        //        bool dimDirectX = true;
        //        foreach (Point3d myDimPositionItem in myListPositionDim)
        //        {
        //            ptPositionInsert = myDimPositionItem;
        //            if (ptPositionInsert.Y < myPolySelected.getMinMaxPoint()[2].Y || ptPositionInsert.Y > myPolySelected.getMinMaxPoint()[3].Y)
        //            {
        //                minPoint = myPolySelected.getMinMaxPoint()[0];
        //                maxPoint = myPolySelected.getMinMaxPoint()[1];
        //                dimDirectX = true;
        //            }
        //            else
        //            {
        //                minPoint = myPolySelected.getMinMaxPoint()[2];
        //                maxPoint = myPolySelected.getMinMaxPoint()[3];
        //                dimDirectX = false;
        //            }

        //            // Lay tap hop diem

        //            bool sidePick = isLeftOrAbove(minPoint, maxPoint, ptPositionInsert);

        //            List<Point3d> listPointToDim = new List<Point3d>();
        //            listPointToDim.Add(minPoint);
        //            listPointToDim.Add(maxPoint);

        //            for (int i = 0; i < myPolySelected.NumberOfVertices; i++)
        //            {
        //                Point3d myPointCheck = myPolySelected.GetPoint3dAt(i);

        //                // Neu cung side thi them vao database
        //                if (isLeftOrAbove(minPoint, maxPoint, myPointCheck) == sidePick)
        //                {
        //                    listPointToDim.Add(myPointCheck);
        //                }
        //            }

        //            if (dimDirectX)
        //            {
        //                listPointToDim.Sort(sortByX);
        //                CmdDim.autoDimHorizontalNotSelect(listPointToDim, ptPositionInsert);
        //            }
        //            else
        //            {
        //                listPointToDim.Sort(sortByY);
        //                CmdDim.autoDimVerticalNotSelect(listPointToDim, ptPositionInsert);
        //            }
        //        }

        //        acTrans.Commit();
        //    }
        //}

        
        // Nhan 1 parameter la 1 polyline sau do dim chung





        public static void DimPolyLineByObject(Polyline myPolySelected)
        {
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Application.DocumentManager.MdiActiveDocument.Database.Orthomode = false;
            Database acCurDb = acDoc.Database;

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {

                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                                                OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                OpenMode.ForWrite) as BlockTableRecord;

                double scaleCurrentDim = acCurDb.GetDimstyleData().Dimscale;


                if (myPolySelected.NumberOfVertices < 3) return;

                if (myPolySelected.Area == 0) return;

                myPolySelected.Closed = true;

                //// Pick side to insert
                //// Chọn 1 diem tren man hinh de pick insert
                //PromptPointResult pPtRes;
                //PromptPointOptions pPtOpts = new PromptPointOptions("");

                //// Prompt for the start point
                //pPtOpts.Message = "\nPick a point to place Dim: ";
                //pPtRes = acDoc.Editor.GetPoint(pPtOpts);

                //// Exit if the user presses ESC or cancels the command
                //if (pPtRes.Status == PromptStatus.Cancel) return;
                //Point3d ptPositionInsert = pPtRes.Value;
                Point3d ptPositionInsert = new Point3d();

                List<Point3d> myListPositionDim = new List<Point3d>();

                Point3d pDp1 = new Point3d(myPolySelected.getMinMaxPoint()[0].X - scaleCurrentDim * 10,
                    myPolySelected.getMinMaxPoint()[0].Y, 0);

                Point3d pDp2 = new Point3d(myPolySelected.getMinMaxPoint()[1].X + scaleCurrentDim * 10,
                    myPolySelected.getMinMaxPoint()[1].Y, 0);

                Point3d pDp3 = new Point3d(myPolySelected.getMinMaxPoint()[2].X,
                    myPolySelected.getMinMaxPoint()[2].Y - scaleCurrentDim * 10, 0);

                Point3d pDp4 = new Point3d(myPolySelected.getMinMaxPoint()[3].X,
                    myPolySelected.getMinMaxPoint()[3].Y + scaleCurrentDim * 10, 0);

                myListPositionDim.Add(pDp1);
                myListPositionDim.Add(pDp2);
                myListPositionDim.Add(pDp3);
                myListPositionDim.Add(pDp4);


                Point3d minPoint = new Point3d();
                Point3d maxPoint = new Point3d();

                bool dimDirectX = true;
                foreach (Point3d myDimPositionItem in myListPositionDim)
                {
                    ptPositionInsert = myDimPositionItem;
                    if (ptPositionInsert.Y < myPolySelected.getMinMaxPoint()[2].Y || ptPositionInsert.Y > myPolySelected.getMinMaxPoint()[3].Y)
                    {
                        minPoint = myPolySelected.getMinMaxPoint()[0];
                        maxPoint = myPolySelected.getMinMaxPoint()[1];
                        dimDirectX = true;
                    }
                    else
                    {
                        minPoint = myPolySelected.getMinMaxPoint()[2];
                        maxPoint = myPolySelected.getMinMaxPoint()[3];
                        dimDirectX = false;
                    }

                    // Lay tap hop diem

                    bool sidePick = isLeftOrAbove(minPoint, maxPoint, ptPositionInsert);

                    List<Point3d> listPointToDim = new List<Point3d>();
                    listPointToDim.Add(minPoint);
                    listPointToDim.Add(maxPoint);

                    for (int i = 0; i < myPolySelected.NumberOfVertices; i++)
                    {
                        Point3d myPointCheck = myPolySelected.GetPoint3dAt(i);

                        // Neu cung side thi them vao database
                        if (isLeftOrAbove(minPoint, maxPoint, myPointCheck) == sidePick)
                        {
                            listPointToDim.Add(myPointCheck);
                        }
                    }

                    if (dimDirectX)
                    {
                        listPointToDim.Sort(sortByX);
                        CmdDim.autoDimHorizontalNotSelect(listPointToDim, ptPositionInsert);
                    }
                    else
                    {
                        listPointToDim.Sort(sortByY);
                        CmdDim.autoDimVerticalNotSelect(listPointToDim, ptPositionInsert);
                    }
                }

                acTrans.Commit();
            }
        }

        [CommandMethod("MPD")]
        public static void autoDimMulPolyline()
        {// Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {

                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                                                OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                OpenMode.ForWrite) as BlockTableRecord;

                // Create a TypedValue array to define the filter criteria
                TypedValue[] acTypValAr = new TypedValue[1];
                acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "POLYLINE,LWPOLYLINE"), 0);
                //acTypValAr.SetValue(new TypedValue((int)DxfCode.LayerName, "0"), 2);

                // Assign the filter criteria to a SelectionFilter object
                SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);
                

                // Request for objects to be selected in the drawing area
                PromptSelectionResult acSSPrompt = acDoc.Editor.GetSelection(acSelFtr);


                // If the prompt status is OK, objects were selected
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    SelectionSet acSSet = acSSPrompt.Value;
                    if (acSSet == null) return;

                    List<Polyline> myListPolyValid = new List<Polyline>();

                    foreach (SelectedObject acSSObj in acSSet)
                    {
                        Polyline myPolylineItem = acSSObj.ObjectId.GetObject(OpenMode.ForWrite) as Polyline;
                        if (myPolylineItem.Area > 0 && myPolylineItem.NumberOfVertices > 2)
                        {
                            myListPolyValid.Add(myPolylineItem);
                        }
                        else
                        {
                            continue;
                        }
                    }

                    foreach (Polyline myPoly in myListPolyValid)
                    {
                        DimPolyLineByObject(myPoly);
                    }
                }
                acTrans.Commit();
                return;
            }
        }




        public static bool isLeftOrAbove(Point3d POL1, Point3d POL2, Point3d PO)
        {
            return ((POL2.X - POL1.X) * (PO.Y - POL1.Y) - (POL2.Y - POL1.Y) * (PO.X - POL1.X)) > 0;
        }


        static int sortByY(Point3d a, Point3d b)
        {
            if (a.Y == b.Y)
                return a.X.CompareTo(b.X);
            return a.Y.CompareTo(b.Y);
        }

        static int sortByX(Point3d a, Point3d b)
        {
            if (a.X == b.X)
                return a.Y.CompareTo(b.Y);
            return a.X.CompareTo(b.X);
        }
    }
}
