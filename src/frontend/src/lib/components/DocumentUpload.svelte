<script lang="ts">
  import { createEventDispatcher } from 'svelte';
  import { api, type UploadResponse } from '../api';

  const dispatch = createEventDispatcher<{ uploaded: UploadResponse }>();

  let files: FileList | null = null;
  let uploading = false;
  let error: string | null = null;
  let dragOver = false;

  async function handleUpload() {
    if (!files || files.length === 0) {
      error = 'Please select at least one file';
      return;
    }

    uploading = true;
    error = null;

    try {
      const fileArray = Array.from(files);
      const result = await api.uploadDocuments(fileArray);
      dispatch('uploaded', result);
      files = null;
      // Reset file input
      const fileInput = document.getElementById('file-input') as HTMLInputElement;
      if (fileInput) fileInput.value = '';
    } catch (err) {
      error = err instanceof Error ? err.message : 'Upload failed';
    } finally {
      uploading = false;
    }
  }

  function handleDrop(event: DragEvent) {
    event.preventDefault();
    dragOver = false;

    if (event.dataTransfer?.files) {
      files = event.dataTransfer.files;
    }
  }

  function handleDragOver(event: DragEvent) {
    event.preventDefault();
    dragOver = true;
  }

  function handleDragLeave() {
    dragOver = false;
  }

  $: fileCount = files?.length || 0;
</script>

<div class="upload-container">
  <h2>Upload Documents</h2>

  <div
    class="drop-zone"
    class:drag-over={dragOver}
    on:drop={handleDrop}
    on:dragover={handleDragOver}
    on:dragleave={handleDragLeave}
    role="button"
    tabindex="0"
  >
    <div class="icon">📄</div>
    <p>Drag & drop files here or click to browse</p>
    <input
      id="file-input"
      type="file"
      bind:files
      multiple
      accept=".pdf,.jpg,.jpeg,.png"
    />
    <p class="hint">Supported: PDF, JPG, PNG (max 50 MB per file)</p>
  </div>

  {#if fileCount > 0}
    <div class="file-list">
      <h3>Selected Files ({fileCount})</h3>
      <ul>
        {#each Array.from(files || []) as file}
          <li>
            <span class="file-name">{file.name}</span>
            <span class="file-size">{(file.size / 1024 / 1024).toFixed(2)} MB</span>
          </li>
        {/each}
      </ul>
    </div>
  {/if}

  {#if error}
    <div class="error">
      ❌ {error}
    </div>
  {/if}

  <button
    on:click={handleUpload}
    disabled={uploading || fileCount === 0}
    class="upload-btn"
  >
    {uploading ? 'Uploading...' : `Upload ${fileCount} file${fileCount !== 1 ? 's' : ''}`}
  </button>
</div>

<style>
  .upload-container {
    background: white;
    border-radius: 0.5rem;
    padding: 1.5rem;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  }

  h2 {
    margin: 0 0 1rem 0;
    font-size: 1.5rem;
    color: #333;
  }

  .drop-zone {
    border: 2px dashed #ccc;
    border-radius: 0.5rem;
    padding: 2rem;
    text-align: center;
    cursor: pointer;
    transition: all 0.2s;
    position: relative;
  }

  .drop-zone:hover,
  .drop-zone.drag-over {
    border-color: #667eea;
    background: #f0f4ff;
  }

  .icon {
    font-size: 3rem;
    margin-bottom: 1rem;
  }

  .drop-zone p {
    margin: 0.5rem 0;
    color: #666;
  }

  .hint {
    font-size: 0.85rem;
    color: #999;
  }

  input[type="file"] {
    position: absolute;
    width: 100%;
    height: 100%;
    top: 0;
    left: 0;
    opacity: 0;
    cursor: pointer;
  }

  .file-list {
    margin: 1.5rem 0;
    padding: 1rem;
    background: #f7f7f7;
    border-radius: 0.5rem;
  }

  .file-list h3 {
    margin: 0 0 0.75rem 0;
    font-size: 1rem;
    color: #555;
  }

  .file-list ul {
    list-style: none;
    padding: 0;
    margin: 0;
  }

  .file-list li {
    display: flex;
    justify-content: space-between;
    padding: 0.5rem;
    border-bottom: 1px solid #e0e0e0;
  }

  .file-list li:last-child {
    border-bottom: none;
  }

  .file-name {
    color: #333;
    font-weight: 500;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    flex: 1;
    margin-right: 1rem;
  }

  .file-size {
    color: #999;
    font-size: 0.9rem;
  }

  .error {
    margin: 1rem 0;
    padding: 0.75rem;
    background: #fee;
    border: 1px solid #fcc;
    border-radius: 0.25rem;
    color: #c33;
  }

  .upload-btn {
    width: 100%;
    background: #667eea;
    color: white;
    border: none;
    padding: 0.75rem;
    border-radius: 0.5rem;
    font-size: 1rem;
    font-weight: 600;
    cursor: pointer;
    transition: background 0.2s;
  }

  .upload-btn:hover:not(:disabled) {
    background: #5568d3;
  }

  .upload-btn:disabled {
    background: #ccc;
    cursor: not-allowed;
  }

  .upload-btn:active:not(:disabled) {
    transform: scale(0.98);
  }
</style>
