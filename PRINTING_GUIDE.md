# FastReport Thermal Printer Integration

This document describes the FastReport-based printing implementation for POS thermal printers.

## Overview

The system uses **FastReport.OpenSource** to generate and print receipts to thermal printers. The implementation replaces the legacy ESC/POS printing system.

## Components

### 1. **Models/PrintRequest.cs**
Request model for print operations:
```csharp
{
  "orderNumber": "ORD-12345",    // Required
  "printerName": "Thermal Printer" // Optional, uses default if not provided
}
```

### 2. **Controllers/PrintController.cs**
API endpoints for printing:
- `POST /api/print` - Print a receipt
- `GET /api/print/printers` - List available printers

### 3. **Helpers/FastReportPrintHelper.cs**
Core printing logic:
- Report preparation and data binding
- Image export and printing
- Printer management utilities

### 4. **Models/DTO/Bills/PrintResponseDto.cs**
Response DTOs for API responses

### 5. **posbill.frx**
FastReport template for thermal receipts (80mm width)

## API Usage

### Print a Receipt

**Endpoint:** `POST /api/print`

**Request:**
```json
{
  "orderNumber": "ORD-12345",
  "printerName": "XP-80C"  // Optional
}
```

**Response (Success):**
```json
{
  "printed": true,
  "orderNumber": "ORD-12345",
  "status": "Completed",
  "printer": "XP-80C"
}
```

**Response (Error):**
```json
{
  "printed": false,
  "orderNumber": "ORD-12345",
  "status": "Completed",
  "printer": "XP-80C",
  "error": "Failed to send to printer 'XP-80C'"
}
```

### Get Available Printers

**Endpoint:** `GET /api/print/printers`

**Response:**
```json
{
  "printers": [
    "Microsoft Print to PDF",
    "XP-80C",
    "Thermal Printer"
  ],
  "defaultPrinter": "XP-80C"
}
```

## FastReport Template (posbill.frx)

The template uses these parameters:
- `date` - Order date (yyyy-MM-dd)
- `time` - Order time (HH:mm:ss)
- `invoceNo` - Invoice/Order number
- `TotalAmount` - Net amount
- `Discount` - Total discount
- `Cash` - Amount paid
- `Balance` - Balance/change
- `GrosAmount` - Gross amount

The template uses a data source `Items` with:
- `no` - Line number
- `description` - Item name
- `mprice` - Marked price
- `ourprice` - Sale price
- `qty` - Quantity
- `nextAmount` - Line total

## Printing Process

1. **Load Report Template**: Reads `posbill.frx` from application root
2. **Bind Data**: Registers order items and sets parameters
3. **Prepare Report**: FastReport prepares the report for printing
4. **Export to Image**: Converts report to PNG (203 DPI for thermal printers)
5. **Print**: Sends image to printer using System.Drawing.Printing

## Thermal Printer Configuration

The implementation supports standard 80mm thermal printers:
- **Resolution**: 203 DPI (typical for thermal printers)
- **Paper Width**: 80mm (configured in posbill.frx)
- **Image Format**: PNG (for compatibility)

## Error Handling

The system handles:
- Missing report template
- Order not found
- No printers available
- Printing failures

All errors return appropriate HTTP status codes and descriptive messages.

## Dependencies

```xml
<PackageReference Include="FastReport.OpenSource" Version="2026.1.3" />
<PackageReference Include="FastReport.OpenSource.Web" Version="2026.1.3" />
<PackageReference Include="System.Drawing.Common" Version="7.0.6" />
```

## Notes

- Temporary PNG files are created during printing and automatically cleaned up
- The system automatically scales images to fit printer width
- Default printer is used if not specified in the request
- All print operations run asynchronously to avoid blocking API requests
