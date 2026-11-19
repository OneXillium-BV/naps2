# OneXillium branch `feature/station-pdf-options`

This branch adds three PDF options for the OXhub ScanStation and removes the vulnerable ImageSharp package. The station installer in OneXillium-BV/OXhub-Scanstation (`station-installer/`) ships a build of this branch, and the station relies on its console options.

**Status:** this branch is not merged into `master` yet. A pull request couldn't be opened, because neither of our accounts may create pull requests on this fork. Whoever has that right: please merge `feature/station-pdf-options` into `master`, using the text below as the description.

## PDF options (be684295f)

- **JPEG quality:** `PdfCompression.JpegQuality` / `--pdfjpegquality N`.
  - Colour and greyscale pages go into the PDF as JPEG at quality N, also when the source is lossless (imported PNG, MaxQuality scans).
  - Black-and-white pages keep JBIG2 or CCITT, transparent images stay lossless, and unmodified JPEG files are still embedded as they are.
  - Without the option, nothing changes.
- **PDF/A-2u:** `PdfCompat.PdfA2U` / `--pdfcompat A2-u` writes PDF/A-2u natively (part 2, conformance U, PDF 1.7). It's also in the PDF settings dropdown. The value is appended to the enum, so existing values keep their numbers.
- **JBIG2 switch:** taken from `IvarSnaaijer/naps2` `fix/issue-952-jbig2-toggle`, without its ImageSharp bump. It's merged with the flatten-before-OCR option, and `--disablejbig2` adds it to the console.
- **Behaviour change for the console:** it now sets PDF compression only from its own options, the way it already does for metadata and encryption. Console output no longer depends on what was set in the GUI.

## ImageSharp removed (09eb05193)

- **Problem:** `SixLabors.ImageSharp` 3.1.12 has a high-severity advisory, [GHSA-v76p-62qx-wwq2](https://github.com/advisories/GHSA-v76p-62qx-wwq2), a heap out-of-bounds write when reading tiled fax TIFFs.
- **Why not upgrade:** every fixed version (3.2.0, 4.1.1 or later) refuses to build without a paid Six Labors licence key.
- **What this does:** upstream NAPS2 deleted `NAPS2.Images.ImageSharp` for the same reason (cyanfish/naps2 8ad6016d4). This branch cherry-picks that deletion. Upstream's replacement, `NAPS2.Images.Skia`, depends on upstream commits this branch doesn't have, so it isn't included.
- **Effect on Windows:** none. The Windows app never shipped ImageSharp; it uses GDI.

## Building for the station

Publish both the app and the console; publishing the app alone does not rebuild `NAPS2.Console.exe`:

```
dotnet publish NAPS2.App.WinForms -c Release -r win-x64 --self-contained -o _build\station
dotnet publish NAPS2.App.Console  -c Release -r win-x64 --self-contained -o _build\station
```

`NAPS2.Console.exe --help` shows the commit on its first line and lists `--pdfjpegquality` and `--disablejbig2`.

## Tests

- `NAPS2.Sdk.Tests`: 948 passed, 30 skipped.
- `NAPS2.Lib.Tests`: 424 passed, 8 skipped.
- Run them on `net10.0-windows`, for example `dotnet test NAPS2.Sdk.Tests\NAPS2.Sdk.Tests.csproj -c Release`.
- New tests:
  - PDF/A-2u validation (plain, with OCR, imported PDF);
  - export tests for the JPEG quality option;
  - console tests for the new flags.

## Not included

This branch is 161 commits behind cyanfish/naps2. Syncing with upstream (including the Skia backend) is a separate task.
