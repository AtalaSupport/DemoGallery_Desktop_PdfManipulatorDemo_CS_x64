# PdfMaipulatorDemo

This demo shows how you can use our PdfDocument class on the back end (along 
with our PdfDecoder and ThumbnailView to provide a visual UI)

You can rearrange pages, delete pages, and combine pages from multiple pdfs with 
a simple thumbnail driven UI

Demonstrates how to manipulate pages in a PDF, and resave without having to recompose
the entire PDF. Includes the ability to reoder pages, remove pages, merge multiple PDF
files, and save the result.

This Windows Forms application demonstrates the use
of features in our PdfDoc class that do NOT require the purchase of DotPdf.

This is the C# version

## Instructions
PDF Manipulator is a tool used for combining pages from different PDF files into a new PDF file.

### Adding Pages:
To get started, click the File menu and select the Add Files… menu item.  Each page of the PDF file will be shown as a thumbnail and its filename is listed in the control on the right side of the window.  Additional files can be added to the existing thumbnails using this same procedure.

### Reordering Pages:
You can reorder the pages by dragging the thumbnails with your mouse.  An indicator is shown between the pages where the page you’re dragging will be inserted once the mouse button is released.

### Removing Pages:
To remove a page, select the page thumbnail with your mouse and press the Delete key on your keyboard.  To remove all pages from a file that was added, select the filename from the list on the right and press the Delete key on your keyboard.

### Saving the Result:
To save these pages as a multipage PDF, click the File menu and select the Save As… menu item.  This will bring up a dialog so you can choose where to save the image and its filename.


## Prerequisites
This demo assumes you have the Atalasoft DotImage SDK installed and 
licensed for DotImage Document Imaging and PdfReader add-on (or you can 
request a 30 day evaluation when installing/activating)

[Download DotImage](https://www.atalasoft.com/BeginDownload/DotImageDownloadPage)

## Cloning
We recommend the following if you wish to donload/clone a copy

Example: git for windows
```bash
git clone https://github.com/AtalaSupport/DemoGallery_Desktop_PdfManipulatorDemo_CS_x64.git PdfManipulatorDemo
```

## Last Update
Last updated 2025-11-13 - TD

