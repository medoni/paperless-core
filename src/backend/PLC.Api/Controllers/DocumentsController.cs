using Microsoft.AspNetCore.Mvc;
using PLC.Application.Commands;
using PLC.Application.DTOs;
using PLC.Application.Handlers;
using PLC.Infrastructure.Interfaces;

namespace PLC.Api.Controllers;

[ApiController]
[Route("api/v1/documents")]
public class DocumentsController : ControllerBase
{
    private readonly UploadDocumentHandler _uploadHandler;
    private readonly IDocumentRepository _documentRepository;

    public DocumentsController(
        UploadDocumentHandler uploadHandler,
        IDocumentRepository documentRepository)
    {
        _uploadHandler = uploadHandler;
        _documentRepository = documentRepository;
    }

    /// <summary>
    /// Upload one or more documents
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UploadResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadDocuments(
        [FromForm] IFormFile[] files,
        [FromForm] string? tags,
        [FromForm] string? assignedTo,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new UploadDocumentCommand
            {
                Files = files,
                Tags = ParseCommaSeparated(tags),
                AssignedTo = ParseCommaSeparated(assignedTo)
            };

            var response = await _uploadHandler.HandleAsync(command, cancellationToken);

            return CreatedAtAction(
                nameof(GetDocumentById),
                new { id = response.Documents.First().Id },
                response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = "INVALID_REQUEST",
                Message = ex.Message
            });
        }
        catch (Exception)
        {
            // TODO: Add proper logging (including exception details)
            return StatusCode(500, new ErrorResponse
            {
                Error = "INTERNAL_ERROR",
                Message = "An error occurred while processing the upload"
            });
        }
    }

    /// <summary>
    /// Get document by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocumentById(
        string id,
        CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(id, cancellationToken);

        if (document == null)
        {
            return NotFound(new ErrorResponse
            {
                Error = "DOCUMENT_NOT_FOUND",
                Message = $"Document with ID '{id}' not found"
            });
        }

        // Map to DTO
        var dto = new DocumentDto
        {
            Id = document.Id,
            FileName = document.FileName,
            FileSize = document.FileSize,
            MimeType = document.MimeType,
            UploadedAt = document.UploadedAt,
            UploadedBy = document.UploadedBy,
            Status = document.Status.ToString(),
            Category = document.Category,
            Subcategory = document.Subcategory,
            ClassificationConfidence = document.ClassificationConfidence,
            Slug = document.Slug,
            Tags = document.Tags,
            AssignedTo = document.AssignedTo,
            StorageUri = document.StorageUri,
            OcrText = document.OcrText,
            ExtractedDataJson = document.ExtractedDataJson
        };

        return Ok(dto);
    }

    private List<string>? ParseCommaSeparated(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        return input.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToList();
    }
}

public class ErrorResponse
{
    public string Error { get; set; } = null!;
    public string Message { get; set; } = null!;
    public object? Details { get; set; }
}
