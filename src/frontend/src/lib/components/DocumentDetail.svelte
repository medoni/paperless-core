<script lang="ts">
  import { onMount } from 'svelte';
  import { api, type Document } from '../api';

  export let documentId: string;

  let document: Document | null = null;
  let loading = true;
  let error: string | null = null;

  async function loadDocument() {
    loading = true;
    error = null;

    try {
      document = await api.getDocument(documentId);
    } catch (err) {
      error = err instanceof Error ? err.message : 'Failed to load document';
    } finally {
      loading = false;
    }
  }

  function getStatusColor(status: string): string {
    switch (status) {
      case 'processed': return '#10b981';
      case 'failed': return '#ef4444';
      case 'manual-review': return '#f59e0b';
      default: return '#6b7280';
    }
  }

  function getProcessingStatusIcon(status: string): string {
    switch (status) {
      case 'completed': return '✅';
      case 'failed': return '❌';
      case 'processing': return '⏳';
      case 'pending': return '⏸️';
      case 'skipped': return '⊘';
      default: return '❓';
    }
  }

  onMount(() => {
    loadDocument();
  });
</script>

<div class="document-detail">
  {#if loading}
    <div class="loading">
      <div class="spinner"></div>
      <p>Loading document...</p>
    </div>
  {:else if error}
    <div class="error">
      <p>❌ {error}</p>
      <button on:click={loadDocument}>Retry</button>
    </div>
  {:else if document}
    <div class="header">
      <h2>{document.fileName}</h2>
      <span
        class="status-badge"
        style="background-color: {getStatusColor(document.status)}"
      >
        {document.status}
      </span>
    </div>

    <div class="metadata">
      <div class="meta-group">
        <h3>File Information</h3>
        <dl>
          <dt>File Size</dt>
          <dd>{(document.fileSize / 1024 / 1024).toFixed(2)} MB</dd>

          <dt>MIME Type</dt>
          <dd>{document.mimeType}</dd>

          <dt>Uploaded At</dt>
          <dd>{new Date(document.uploadedAt).toLocaleString()}</dd>

          <dt>Uploaded By</dt>
          <dd>{document.uploadedBy}</dd>

          {#if document.slug}
            <dt>Slug</dt>
            <dd>{document.slug}</dd>
          {/if}
        </dl>
      </div>

      <div class="meta-group">
        <h3>Processing Status</h3>
        <dl>
          <dt>Classification</dt>
          <dd>
            {getProcessingStatusIcon(document.processingStatus.classification)}
            {document.processingStatus.classification}
          </dd>

          <dt>OCR</dt>
          <dd>
            {getProcessingStatusIcon(document.processingStatus.ocr)}
            {document.processingStatus.ocr}
          </dd>

          <dt>Extraction</dt>
          <dd>
            {getProcessingStatusIcon(document.processingStatus.extraction)}
            {document.processingStatus.extraction}
          </dd>
        </dl>
      </div>

      {#if document.classification}
        <div class="meta-group">
          <h3>Classification</h3>
          <dl>
            <dt>Category</dt>
            <dd>{document.classification.category}</dd>

            <dt>Subcategory</dt>
            <dd>{document.classification.subcategory}</dd>

            <dt>Confidence</dt>
            <dd>{(document.classification.confidence * 100).toFixed(1)}%</dd>
          </dl>
        </div>
      {/if}

      {#if document.tags.length > 0}
        <div class="meta-group">
          <h3>Tags</h3>
          <div class="tags">
            {#each document.tags as tag}
              <span class="tag">{tag}</span>
            {/each}
          </div>
        </div>
      {/if}

      {#if document.assignedTo.length > 0}
        <div class="meta-group">
          <h3>Assigned To</h3>
          <div class="tags">
            {#each document.assignedTo as person}
              <span class="tag person">{person}</span>
            {/each}
          </div>
        </div>
      {/if}

      {#if document.extractedData}
        <div class="meta-group">
          <h3>Extracted Data</h3>
          <pre>{JSON.stringify(document.extractedData, null, 2)}</pre>
        </div>
      {/if}
    </div>

    <div class="actions">
      <a
        href={api.getDownloadUrl(documentId)}
        target="_blank"
        class="btn btn-primary"
      >
        📥 Download
      </a>
      <button on:click={loadDocument} class="btn btn-secondary">
        🔄 Refresh
      </button>
    </div>
  {/if}
</div>

<style>
  .document-detail {
    background: white;
    border-radius: 0.5rem;
    padding: 1.5rem;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  }

  .loading,
  .error {
    text-align: center;
    padding: 3rem 1rem;
  }

  .spinner {
    border: 3px solid #f3f3f3;
    border-top: 3px solid #667eea;
    border-radius: 50%;
    width: 40px;
    height: 40px;
    animation: spin 1s linear infinite;
    margin: 0 auto 1rem;
  }

  @keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
  }

  .header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1.5rem;
    padding-bottom: 1rem;
    border-bottom: 2px solid #f0f0f0;
  }

  h2 {
    margin: 0;
    font-size: 1.5rem;
    color: #333;
    word-break: break-all;
  }

  .status-badge {
    padding: 0.25rem 0.75rem;
    border-radius: 1rem;
    color: white;
    font-size: 0.85rem;
    font-weight: 600;
    text-transform: uppercase;
    white-space: nowrap;
  }

  .metadata {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
    gap: 1.5rem;
  }

  .meta-group {
    background: #f9f9f9;
    padding: 1rem;
    border-radius: 0.5rem;
  }

  h3 {
    margin: 0 0 0.75rem 0;
    font-size: 1rem;
    color: #555;
    font-weight: 600;
  }

  dl {
    margin: 0;
  }

  dt {
    font-weight: 600;
    color: #666;
    margin-top: 0.5rem;
    font-size: 0.85rem;
  }

  dt:first-child {
    margin-top: 0;
  }

  dd {
    margin: 0.25rem 0 0 0;
    color: #333;
    font-family: 'Courier New', monospace;
    font-size: 0.9rem;
  }

  .tags {
    display: flex;
    flex-wrap: wrap;
    gap: 0.5rem;
  }

  .tag {
    display: inline-block;
    padding: 0.25rem 0.75rem;
    background: #e0e7ff;
    color: #4338ca;
    border-radius: 0.25rem;
    font-size: 0.85rem;
    font-weight: 500;
  }

  .tag.person {
    background: #dcfce7;
    color: #166534;
  }

  pre {
    background: #f0f0f0;
    padding: 1rem;
    border-radius: 0.25rem;
    overflow-x: auto;
    font-size: 0.85rem;
    margin: 0.5rem 0 0 0;
  }

  .actions {
    margin-top: 1.5rem;
    padding-top: 1.5rem;
    border-top: 2px solid #f0f0f0;
    display: flex;
    gap: 1rem;
  }

  .btn {
    padding: 0.75rem 1.5rem;
    border-radius: 0.5rem;
    font-size: 1rem;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.2s;
    border: none;
    text-decoration: none;
    display: inline-block;
  }

  .btn-primary {
    background: #667eea;
    color: white;
  }

  .btn-primary:hover {
    background: #5568d3;
  }

  .btn-secondary {
    background: #e5e7eb;
    color: #374151;
  }

  .btn-secondary:hover {
    background: #d1d5db;
  }

  .btn:active {
    transform: scale(0.98);
  }

  .error button {
    margin-top: 1rem;
  }
</style>
