/**
 * PaperlessCore API Client
 * Generated from OpenAPI spec
 */

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:3000';

// Types based on OpenAPI spec
export interface Document {
  id: string;
  fileName: string;
  fileSize: number;
  mimeType: string;
  uploadedAt: string;
  uploadedBy: string;
  status: 'uploaded' | 'classifying' | 'classified' | 'ocr-processing' | 'ocr-completed' | 'extracting' | 'processed' | 'failed' | 'manual-review';
  processingStatus: {
    classification: 'pending' | 'processing' | 'completed' | 'failed' | 'skipped';
    ocr: 'pending' | 'processing' | 'completed' | 'failed' | 'skipped';
    extraction: 'pending' | 'processing' | 'completed' | 'failed' | 'skipped';
  };
  classification?: {
    category: string;
    subcategory: string;
    confidence: number;
  };
  tags: string[];
  assignedTo: string[];
  extractedData?: Record<string, any>;
  storageUri?: string;
  slug?: string;
  ocrText?: string;
}

export interface UploadedDocument {
  id: string;
  fileName: string;
  fileSize: number;
  mimeType: string;
  status: string;
  uploadedAt: string;
}

export interface UploadResponse {
  uploadId: string;
  documents: UploadedDocument[];
}

export interface ErrorResponse {
  error: string;
  message: string;
  details?: Record<string, any>;
}

export interface HealthResponse {
  status: 'healthy' | 'degraded' | 'unhealthy';
  timestamp: string;
  version?: string;
}

export interface VersionResponse {
  version: string;
  buildDate: string;
  gitCommit?: string;
  environment?: 'development' | 'staging' | 'production';
}

/**
 * API Client
 */
export class ApiClient {
  private baseUrl: string;

  constructor(baseUrl: string = API_BASE_URL) {
    this.baseUrl = baseUrl;
  }

  /**
   * Health check
   */
  async getHealth(): Promise<HealthResponse> {
    const response = await fetch(`${this.baseUrl}/api/v1/health`);
    if (!response.ok) {
      throw new Error('Health check failed');
    }
    return response.json();
  }

  /**
   * Version information
   */
  async getVersion(): Promise<VersionResponse> {
    const response = await fetch(`${this.baseUrl}/api/v1/version`);
    if (!response.ok) {
      throw new Error('Version check failed');
    }
    return response.json();
  }

  /**
   * Upload documents
   */
  async uploadDocuments(files: File[], metadata?: { tags?: string[]; assignedTo?: string[] }): Promise<UploadResponse> {
    const formData = new FormData();

    files.forEach(file => {
      formData.append('files', file);
    });

    if (metadata) {
      formData.append('metadata', JSON.stringify(metadata));
    }

    const response = await fetch(`${this.baseUrl}/api/v1/documents`, {
      method: 'POST',
      body: formData,
    });

    if (!response.ok) {
      const error: ErrorResponse = await response.json();
      throw new Error(error.message || 'Upload failed');
    }

    return response.json();
  }

  /**
   * Get document by ID
   */
  async getDocument(id: string): Promise<Document> {
    const response = await fetch(`${this.baseUrl}/api/v1/documents/${id}`);

    if (!response.ok) {
      if (response.status === 404) {
        throw new Error('Document not found');
      }
      const error: ErrorResponse = await response.json();
      throw new Error(error.message || 'Failed to fetch document');
    }

    return response.json();
  }

  /**
   * Delete document
   */
  async deleteDocument(id: string): Promise<void> {
    const response = await fetch(`${this.baseUrl}/api/v1/documents/${id}`, {
      method: 'DELETE',
    });

    if (!response.ok && response.status !== 204) {
      const error: ErrorResponse = await response.json();
      throw new Error(error.message || 'Failed to delete document');
    }
  }

  /**
   * Get download URL for document
   */
  getDownloadUrl(id: string): string {
    return `${this.baseUrl}/api/v1/documents/${id}/download`;
  }
}

// Export singleton instance
export const api = new ApiClient();
