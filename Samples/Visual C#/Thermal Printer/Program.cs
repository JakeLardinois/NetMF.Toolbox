using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using Toolbox.NETMF.Hardware;

/*
 * Copyright 2012-2014 Stefan Thoolen (http://www.netmftoolbox.com/)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Thermal_Printer
{
    public class Program
    {
        public static void Main()
        {
            ThermalPrinter Printer = new ThermalPrinter();
            Printer.OnStatusChange += new NativeEventHandler(Printer_OnStatusChange);

            // Use this to make the prints of better quality
            // It will slow down the printer, and perhaps shorten the lifespan of the printer too
            Printer.HeatingTime = 255;
            Printer.HeatingInterval = 255;

            // Font demo
            Printer.SmallText = true; Printer.PrintLine("Small text"); Printer.SmallText = false;
            Printer.Inverted = true; Printer.PrintLine("Inverted text"); Printer.Inverted = false;
            Printer.UpsideDown = true; Printer.PrintLine("Upside down"); Printer.UpsideDown = false;
            Printer.DoubleHeight = true; Printer.PrintLine("Doubled text height"); Printer.DoubleHeight = false;
            Printer.DoubleWidth = true; Printer.PrintLine("Doubled text width"); Printer.DoubleWidth = false;
            Printer.StrikeThrough = true; Printer.PrintLine("Striked through text"); Printer.StrikeThrough = false;
            Printer.PrintLine("Normal text");

            // Alignment demo
            Printer.SetAlignment(ThermalPrinter.Alignment.AlignRight); Printer.PrintLine("Right aligned");
            Printer.SetAlignment(ThermalPrinter.Alignment.AlignCenter); Printer.PrintLine("Centered");
            Printer.SetAlignment(ThermalPrinter.Alignment.AlignLeft); Printer.PrintLine("Left aligned");

            // Spacing demo
            Printer.SetLeftSpacing(8); Printer.PrintLine("8 spaces at the left");
            Printer.SetLeftSpacing(4); Printer.PrintLine("4 spaces at the left");
            Printer.SetLeftSpacing(0); Printer.PrintLine("0 spaces at the left");
            Printer.SetLineSpacing(50); Printer.PrintLine("Line spacing: 50");
            Printer.SetLineSpacing(40); Printer.PrintLine("Line spacing: 40");
            Printer.SetLineSpacing(30); Printer.PrintLine("Line spacing: 30");

            // Barcode demo
            Printer.PrintLine("UPC_A:"); Printer.PrintBarcode("858115002002", ThermalPrinter.BarCodeSystem.UPC_A);
            Printer.PrintLine("EAN13:"); Printer.PrintBarcode("9781449302450", ThermalPrinter.BarCodeSystem.EAN13);
            Printer.PrintLine("CODE128:"); Printer.PrintBarcode("netduino.com", ThermalPrinter.BarCodeSystem.CODE128, 2);

            // QR Code
            Printer.PrintLine("QR Code:");
            Printer.PrintBitmap(Bitmaps.http___www_netduino_com__qrcode.Width, Bitmaps.http___www_netduino_com__qrcode.Height, Bitmaps.http___www_netduino_com__qrcode.Data);

            // Bitmap demo
            Printer.PrintLine("Bitmap demo:");
            Printer.PrintBitmap(Bitmaps.netduino_bmp.Width, Bitmaps.netduino_bmp.Height, Bitmaps.netduino_bmp.Data);

            // Feeds so all demos are visible
            Printer.LineFeed(2);
        }

        static void Printer_OnStatusChange(uint Unused, uint Status, DateTime Time)
        {
            Debug.Print("New status: " + Status.ToString());
        }
    }
}
