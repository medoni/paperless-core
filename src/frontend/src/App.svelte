<script lang="ts">
  import { onMount } from 'svelte';
  import DocumentUpload from './lib/components/DocumentUpload.svelte';
  import DocumentDetail from './lib/components/DocumentDetail.svelte';
  import { api, type UploadResponse } from './lib/api';

  let apiStatus = 'checking...';
  let currentView: 'upload' | 'detail' = 'upload';
  let selectedDocumentId: string | null = null;
  let uploadedDocuments: UploadResponse | null = null;

  async function checkApiHealth() {
    try {
      const data = await api.getHealth();
      apiStatus = `connected (${data.status})`;
    } catch (error) {
      apiStatus = 'disconnected';
    }
  }

  function handleUploaded(event: CustomEvent<UploadResponse>) {
    uploadedDocuments = event.detail;
    if (event.detail.documents.length > 0) {
      // Automatically show first uploaded document
      selectedDocumentId = event.detail.documents[0].id;
      currentView = 'detail';
    }
  }

  function showUpload() {
    currentView = 'upload';
    selectedDocumentId = null;
  }

  onMount(() => {
    checkApiHealth();
  });
</script>

<main>
  <div class="container">
    <header>
      <h1>PaperlessCore 📄</h1>
      <p class="subtitle">Document Management System</p>
    </header>

    <div class="status-bar">
      <span class="status-label">API Status:</span>
      <span
        class="status-value"
        class:success={apiStatus.includes('connected')}
        class:error={apiStatus.includes('disconnected')}
      >
        {apiStatus}
      </span>
    </div>

    <nav class="tabs">
      <button
        class="tab"
        class:active={currentView === 'upload'}
        on:click={showUpload}
      >
        📤 Upload
      </button>
      <button
        class="tab"
        class:active={currentView === 'detail'}
        disabled={!selectedDocumentId}
        on:click={() => currentView = 'detail'}
      >
        📋 Document Details
      </button>
    </nav>

    <div class="content">
      {#if currentView === 'upload'}
        <DocumentUpload on:uploaded={handleUploaded} />

        {#if uploadedDocuments}
          <div class="upload-success">
            <h3>✅ Upload Successful!</h3>
            <p>
              {uploadedDocuments.documents.length} document{uploadedDocuments.documents.length !== 1 ? 's' : ''} uploaded successfully
            </p>
            <ul class="uploaded-list">
              {#each uploadedDocuments.documents as doc}
                <li>
                  <button
                    class="doc-link"
                    on:click={() => {
                      selectedDocumentId = doc.id;
                      currentView = 'detail';
                    }}
                  >
                    {doc.fileName}
                  </button>
                </li>
              {/each}
            </ul>
          </div>
        {/if}
      {:else if currentView === 'detail' && selectedDocumentId}
        <DocumentDetail documentId={selectedDocumentId} />
      {/if}
    </div>
  </div>
</main>

<style>
  :global(body) {
    margin: 0;
    padding: 0;
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto', 'Oxygen',
      'Ubuntu', 'Cantarell', 'Fira Sans', 'Droid Sans', 'Helvetica Neue', sans-serif;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    min-height: 100vh;
    color: #333;
  }

  main {
    min-height: 100vh;
    padding: 2rem;
  }

  .container {
    max-width: 1200px;
    margin: 0 auto;
  }

  header {
    text-align: center;
    margin-bottom: 2rem;
  }

  h1 {
    margin: 0 0 0.5rem 0;
    font-size: 3rem;
    color: white;
    text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.2);
  }

  .subtitle {
    margin: 0;
    color: rgba(255, 255, 255, 0.9);
    font-size: 1.2rem;
  }

  .status-bar {
    background: rgba(255, 255, 255, 0.95);
    padding: 0.75rem 1rem;
    border-radius: 0.5rem;
    margin-bottom: 1.5rem;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 0.5rem;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  }

  .status-label {
    font-weight: 600;
    color: #555;
  }

  .status-value {
    color: #666;
  }

  .success {
    color: #10b981;
    font-weight: 600;
  }

  .error {
    color: #ef4444;
    font-weight: 600;
  }

  .tabs {
    display: flex;
    gap: 0.5rem;
    margin-bottom: 1.5rem;
  }

  .tab {
    flex: 1;
    background: rgba(255, 255, 255, 0.9);
    border: none;
    padding: 1rem;
    border-radius: 0.5rem 0.5rem 0 0;
    font-size: 1rem;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.2s;
    color: #666;
  }

  .tab:hover:not(:disabled) {
    background: white;
  }

  .tab.active {
    background: white;
    color: #667eea;
    box-shadow: 0 -2px 4px rgba(0, 0, 0, 0.1);
  }

  .tab:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  .content {
    background: white;
    border-radius: 0 0 0.5rem 0.5rem;
    padding: 0;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    overflow: hidden;
  }

  .upload-success {
    margin-top: 1.5rem;
    padding: 1.5rem;
    background: #f0fdf4;
    border: 2px solid #86efac;
    border-radius: 0.5rem;
  }

  .upload-success h3 {
    margin: 0 0 0.5rem 0;
    color: #166534;
  }

  .upload-success p {
    margin: 0 0 1rem 0;
    color: #166534;
  }

  .uploaded-list {
    list-style: none;
    padding: 0;
    margin: 0;
  }

  .uploaded-list li {
    padding: 0.5rem 0;
  }

  .doc-link {
    background: none;
    border: none;
    color: #667eea;
    text-decoration: underline;
    cursor: pointer;
    font-size: 0.95rem;
    padding: 0;
  }

  .doc-link:hover {
    color: #5568d3;
  }
</style>
