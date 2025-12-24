namespace PLC.Domain;

/// <summary>
/// Overall document lifecycle status
/// </summary>
public enum DocumentStatus
{
    /// <summary>
    /// Upload to storage in progress
    /// </summary>
    Uploading,

    /// <summary>
    /// Document is being processed (classification, OCR, extraction)
    /// Workers check properties to determine what work needs to be done
    /// </summary>
    Processing,

    /// <summary>
    /// All processing completed successfully
    /// </summary>
    Finished,

    /// <summary>
    /// Document has been soft-deleted
    /// </summary>
    Deleted
}
