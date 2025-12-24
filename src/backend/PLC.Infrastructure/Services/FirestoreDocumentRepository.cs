using Google.Cloud.Firestore;
using Microsoft.Extensions.Options;
using PLC.Domain;
using PLC.Infrastructure.Configuration;
using PLC.Infrastructure.Interfaces;

namespace PLC.Infrastructure.Services;

/// <summary>
/// Firestore implementation for Document repository
/// Maps Document entity to Firestore documents
/// </summary>
public class FirestoreDocumentRepository : IDocumentRepository
{
    private readonly FirestoreDb _firestoreDb;
    private readonly GoogleCloudOptions _options;

    public FirestoreDocumentRepository(
        FirestoreDb firestoreDb,
        IOptions<GoogleCloudOptions> options)
    {
        _firestoreDb = firestoreDb;
        _options = options.Value;
    }

    private CollectionReference Collection =>
        _firestoreDb.Collection(_options.FirestoreCollection);

    public async Task<Document> CreateAsync(
        Document document,
        CancellationToken cancellationToken = default)
    {
        var docRef = Collection.Document(document.Id);
        var data = MapToFirestore(document);

        await docRef.SetAsync(data, cancellationToken: cancellationToken);

        return document;
    }

    public async Task<Document?> GetByIdAsync(
        string documentId,
        CancellationToken cancellationToken = default)
    {
        var docRef = Collection.Document(documentId);
        var snapshot = await docRef.GetSnapshotAsync(cancellationToken);

        if (!snapshot.Exists)
            return null;

        return MapFromFirestore(snapshot);
    }

    public async Task UpdateAsync(
        Document document,
        CancellationToken cancellationToken = default)
    {
        var docRef = Collection.Document(document.Id);
        var data = MapToFirestore(document);

        // Use SetAsync with merge for idempotency
        await docRef.SetAsync(data, SetOptions.MergeAll, cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        string documentId,
        CancellationToken cancellationToken = default)
    {
        var docRef = Collection.Document(documentId);
        var snapshot = await docRef.GetSnapshotAsync(cancellationToken);

        return snapshot.Exists;
    }

    /// <summary>
    /// Map Document entity to Firestore dictionary
    /// </summary>
    private Dictionary<string, object?> MapToFirestore(Document document)
    {
        // TODO: needs refactoring
        return new Dictionary<string, object?>
        {
            ["id"] = document.Id,
            ["fileName"] = document.FileName,
            ["fileSize"] = document.FileSize,
            ["mimeType"] = document.MimeType,
            ["storageUri"] = document.StorageUri,
            ["uploadedAt"] = Timestamp.FromDateTime(document.UploadedAt),
            ["uploadedBy"] = document.UploadedBy,
            ["status"] = document.Status.ToString(),
            ["category"] = document.Category,
            ["subcategory"] = document.Subcategory,
            ["classificationConfidence"] = document.ClassificationConfidence,
            ["slug"] = document.Slug,
            ["ocrText"] = document.OcrText,
            ["extractedDataJson"] = document.ExtractedDataJson,
            ["tags"] = document.Tags,
            ["assignedTo"] = document.AssignedTo,
            ["classifiedAt"] = document.ClassifiedAt.HasValue
                ? Timestamp.FromDateTime(document.ClassifiedAt.Value)
                : null,
            ["ocrCompletedAt"] = document.OcrCompletedAt.HasValue
                ? Timestamp.FromDateTime(document.OcrCompletedAt.Value)
                : null,
            ["extractionCompletedAt"] = document.ExtractionCompletedAt.HasValue
                ? Timestamp.FromDateTime(document.ExtractionCompletedAt.Value)
                : null,
            ["finishedAt"] = document.FinishedAt.HasValue
                ? Timestamp.FromDateTime(document.FinishedAt.Value)
                : null,
            ["deletedAt"] = document.DeletedAt.HasValue
                ? Timestamp.FromDateTime(document.DeletedAt.Value)
                : null
        };
    }

    /// <summary>
    /// Map Firestore document to Document entity
    /// </summary>
    private Document MapFromFirestore(DocumentSnapshot snapshot)
    {
        // TODO: needs refactoring
        var data = snapshot.ToDictionary();

        // Use reflection to set private properties
        var document = (Document)Activator.CreateInstance(
            typeof(Document),
            nonPublic: true)!;

        SetProperty(document, "Id", data["id"].ToString()!);
        SetProperty(document, "FileName", data["fileName"].ToString()!);
        SetProperty(document, "FileSize", Convert.ToInt64(data["fileSize"]));
        SetProperty(document, "MimeType", data["mimeType"].ToString()!);
        SetProperty(document, "StorageUri", data["storageUri"].ToString()!);
        SetProperty(document, "UploadedAt",
            ((Timestamp)data["uploadedAt"]).ToDateTime());
        SetProperty(document, "UploadedBy", data["uploadedBy"].ToString()!);
        SetProperty(document, "Status",
            Enum.Parse<DocumentStatus>(data["status"].ToString()!));

        // Optional fields
        SetPropertyIfExists(document, data, "Category");
        SetPropertyIfExists(document, data, "Subcategory");
        SetPropertyIfExists(document, data, "ClassificationConfidence", v => Convert.ToDouble(v));
        SetPropertyIfExists(document, data, "Slug");
        SetPropertyIfExists(document, data, "OcrText");
        SetPropertyIfExists(document, data, "ExtractedDataJson");

        // Lists
        if (data.ContainsKey("tags") && data["tags"] is List<object> tags)
            SetProperty(document, "Tags", tags.Select(t => t.ToString()!).ToList());
        else
            SetProperty(document, "Tags", new List<string>());

        if (data.ContainsKey("assignedTo") && data["assignedTo"] is List<object> assignedTo)
            SetProperty(document, "AssignedTo", assignedTo.Select(a => a.ToString()!).ToList());
        else
            SetProperty(document, "AssignedTo", new List<string>());

        // Timestamps
        SetPropertyIfExists(document, data, "ClassifiedAt", v => ((Timestamp)v).ToDateTime());
        SetPropertyIfExists(document, data, "OcrCompletedAt", v => ((Timestamp)v).ToDateTime());
        SetPropertyIfExists(document, data, "ExtractionCompletedAt", v => ((Timestamp)v).ToDateTime());
        SetPropertyIfExists(document, data, "FinishedAt", v => ((Timestamp)v).ToDateTime());
        SetPropertyIfExists(document, data, "DeletedAt", v => ((Timestamp)v).ToDateTime());

        return document;
    }

    private void SetProperty(object obj, string propertyName, object value)
    {
        var prop = obj.GetType().GetProperty(propertyName,
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        prop?.SetValue(obj, value);
    }

    private void SetPropertyIfExists(
        object obj,
        Dictionary<string, object> data,
        string propertyName,
        Func<object, object>? converter = null)
    {
        if (data.ContainsKey(LowerFirstChar(propertyName)) &&
            data[LowerFirstChar(propertyName)] != null)
        {
            var value = data[LowerFirstChar(propertyName)];
            if (converter != null)
                value = converter(value);

            SetProperty(obj, propertyName, value);
        }
    }

    private string LowerFirstChar(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return char.ToLowerInvariant(input[0]) + input.Substring(1);
    }
}
