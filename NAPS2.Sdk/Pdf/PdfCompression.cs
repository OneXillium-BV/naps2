namespace NAPS2.Pdf;

/// <summary>
/// Configuration for how images are compressed in exported PDFs.
/// </summary>
public record PdfCompression
{
    /// <summary>
    /// Whether black and white images can be compressed with JBIG2 (lossless). If true, JBIG2 is used when it produces
    /// a smaller result than the alternatives (e.g. CCITT G4).
    /// </summary>
    public bool AllowJbig2 { get; init; } = true;

    /// <summary>
    /// If set (1-100), color and grayscale images are stored in the PDF as JPEG at this quality, including images that
    /// would otherwise be stored losslessly (e.g. imported PNG files or scans with MaxQuality). It also sets the quality
    /// of any other image that is re-encoded as JPEG. Black and white images are unaffected (JBIG2/CCITT), as are images
    /// with transparency and unmodified JPEG files, which are embedded as-is. If null, the image's own quality applies.
    /// </summary>
    public int? JpegQuality { get; init; }
}
