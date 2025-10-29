# Node RED BARRED
The Node RED Barcode Processing Platform!

Node RED BARRED, is complete unbiased Barcode Processing Toolkit, allowing full control of the processes following a scan.

Ths Toolkit is in 2 parts.

 - A Native Mobile Client (iOS, Android)
 - The set of Node RED Nodes.

The mobile application uses `on-device` barcode detection, so performance is much greater than web based barcode scanners. 

Currently, the supported barcode symbologies are:

 - iOS
   - **1D**: Codabar, Code 39, Code 93, Code 128, EAN-8, EAN-13, GS1 DataBar, ITF, UPC-A, UPC-E;
   - **2D**: Aztec, Data Matrix, MicroPDF417, MicroQR, PDF417, QR Code

- Android
   - **1D**: Codabar, Code 39, Code 93, Code 128, EAN-8, EAN-13, ITF, UPC-A, UPC-E;
   - **2D**: Aztec, Data Matrix, PDF417, QR Code

The set of Nodes for Node RED, open up various processing requreiments, and used together - offers massive felxibility in interoperability with other systems/processes - furthermore, the Module allows for a menu system, adding full customsiation of the system.

| Node | Description |
|------|-----------------|
| `Incoming Barcode` | Recieves scanned barcodes  |
| `Send Result` | Respons to the scanner, that sent the barcode  |
| `Incoming Item` | Recieves information requests  |
| `Send Item` | Send information to the connected scanners  |
| `Incoming Action` | Recieves menu requests |

In affect - this Module (along with the Native Mobile applcation - Which is Free & Open Source) brings you a Handheld Barcode Scanning Terminal

There is a complete flow example included with this Node RED module, but detaling how it all fits toiegther in text, is difficult, so I provide a video walk through of this platform below.

# Native App Build Environment

 - Android DK 15 (35)
 - Java DK 25
 - DOTNET 9.0 (With Maui payloads)
 - xCode 16.4
 - Rider 2025.2.3
 - MacOS 26

## To do

 - Allow rich content in responses / items.
 - Add dropdown (select) to allowed list of input types for info  request
 - Add SSL Support
 - Allow for deeper Object formatting.  
   Currently nested objects on the scanner, are not formatted  

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
