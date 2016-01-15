Imports Microsoft.SPOT
Imports Microsoft.SPOT.Hardware
Imports SecretLabs.NETMF.Hardware
Imports SecretLabs.NETMF.Hardware.Netduino
Imports Toolbox.NETMF.Hardware

'  Copyright 2012-2014 Stefan Thoolen (http://www.netmftoolbox.com/)
'
'  Licensed under the Apache License, Version 2.0 (the "License");
'  you may not use this file except in compliance with the License.
'  You may obtain a copy of the License at
'
'      http://www.apache.org/licenses/LICENSE-2.0
'
'  Unless required by applicable law or agreed to in writing, software
'  distributed under the License is distributed on an "AS IS" BASIS,
'  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
'  See the License for the specific language governing permissions and
'  limitations under the License.
Module Module1

    Dim WithEvents Printer As ThermalPrinter

    Sub Main()
        Printer = New ThermalPrinter()

        ' Use this to make the prints of better quality
        ' It will slow down the printer, and perhaps shorten the lifespan of the printer too
        Printer.HeatingTime = 255
        Printer.HeatingInterval = 255

        ' Font demo
        Printer.SmallText = True : Printer.PrintLine("Small text") : Printer.SmallText = False
        Printer.Inverted = True : Printer.PrintLine("Inverted text") : Printer.Inverted = False
        Printer.UpsideDown = True : Printer.PrintLine("Upside down") : Printer.UpsideDown = False
        Printer.DoubleHeight = True : Printer.PrintLine("Doubled text height") : Printer.DoubleHeight = False
        Printer.DoubleWidth = True : Printer.PrintLine("Doubled text width") : Printer.DoubleWidth = False
        Printer.StrikeThrough = True : Printer.PrintLine("Striked through text") : Printer.StrikeThrough = False
        Printer.PrintLine("Normal text")

        ' Alignment demo
        Printer.SetAlignment(ThermalPrinter.Alignment.AlignRight) : Printer.PrintLine("Right aligned")
        Printer.SetAlignment(ThermalPrinter.Alignment.AlignCenter) : Printer.PrintLine("Centered")
        Printer.SetAlignment(ThermalPrinter.Alignment.AlignLeft) : Printer.PrintLine("Left aligned")

        ' Spacing demo
        Printer.SetLeftSpacing(8) : Printer.PrintLine("8 spaces at the left")
        Printer.SetLeftSpacing(4) : Printer.PrintLine("4 spaces at the left")
        Printer.SetLeftSpacing(0) : Printer.PrintLine("0 spaces at the left")
        Printer.SetLineSpacing(50) : Printer.PrintLine("Line spacing: 50")
        Printer.SetLineSpacing(40) : Printer.PrintLine("Line spacing: 40")
        Printer.SetLineSpacing(30) : Printer.PrintLine("Line spacing: 30")

        ' Barcode demo
        Printer.PrintLine("UPC_A:") : Printer.PrintBarcode("858115002002", ThermalPrinter.BarCodeSystem.UPC_A)
        Printer.PrintLine("EAN13:") : Printer.PrintBarcode("9781449302450", ThermalPrinter.BarCodeSystem.EAN13)
        Printer.PrintLine("CODE128:") : Printer.PrintBarcode("netduino.com", ThermalPrinter.BarCodeSystem.CODE128, 2)

        ' QR Code
        Printer.PrintLine("QR Code:")
        Printer.PrintBitmap(Bitmaps.http___www_netduino_com__qrcode.Width, Bitmaps.http___www_netduino_com__qrcode.Height, Bitmaps.http___www_netduino_com__qrcode.Data)

        ' Bitmap demo
        Printer.PrintLine("Bitmap demo:")
        Printer.PrintBitmap(Bitmaps.netduino_bmp.Width, Bitmaps.netduino_bmp.Height, Bitmaps.netduino_bmp.Data)

        ' Feeds so all demos are visible
        Printer.LineFeed(2)
    End Sub

    Private Sub Printer_OnStatusChange(ByVal Unused As UInteger, ByVal Status As UInteger, ByVal Time As Date) Handles Printer.OnStatusChange
        Debug.Print("New status: " + Status.ToString())
    End Sub
End Module
