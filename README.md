# Node RED BARRED
The Node RED Barcode Processing Platform

## Say what!?
Node RED BARRED, is a Barcode Processing toolkit, with no bias towards any specific target.  
This toolkit includes 2 main components

- The set of Node RED Nodes 
  - `Incoming Barcode`
  - `Send Result`
  - `Incoming Item`
  - `Scanner Prompt`

- A native Mobile Client, that does the scanning, the mobile application is developed in .NET MAUI.  
  The barcode decoding is `on-device`, so the performance is only limited by the mobile platform. 

Currently, the supported barcode symbologies are:

 - iOS
   - **1D**: Codabar, Code 39, Code 93, Code 128, EAN-8, EAN-13, GS1 DataBar, ITF, UPC-A, UPC-E;
   - **2D**: Aztec, Data Matrix, MicroPDF417, MicroQR, PDF417, QR Code

- Android
   - **1D**: Codabar, Code 39, Code 93, Code 128, EAN-8, EAN-13, ITF, UPC-A, UPC-E;
   - **2D**: Aztec, Data Matrix, PDF417, QR Code
     

## Native App Build Environment

 - Android DK 15 (35)
 - Java DK 25
 - DOTNET 9.0 (With Maui payloads)
 - xCode 16.4
 - Rider 2025.2.3
 - MacOS 26

## The Usage Flow

![Image](./Node%20RED/Images/flow.png)

## Scanner Prompt Node

The `Scanner Prompt` Node allows you to send a message or object - without the need for a scanner to first present a scan, this for example, can allow you to notify scanners at any point, it can send a message to 1 or many scanners.

## Scanner Enrollment / Branding

The scanner UI, can be controlled by the Configuration Node.

 - Department Name
 - Color Theme
 - Scanner name (per Scanner)
 - Scan Rates & timeouts

 You enrol scanners via a QR Code - which is generated via the Configuration Node.  

## Acknowledgements

[Dynamic Dave - Node RED Community member](https://discourse.nodered.org/u/dynamicdave/summary) - For helping me test  
[Afriscic](https://github.com/afriscic) - For the Native Barcode Decoding lib



## License
MIT License

Copyright (c) 2025 Marcus Davies

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
