<script lang="ts">
  import { onMount } from 'svelte';

  let apiStatus = 'checking...';
  let apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:8080';

  async function checkApiHealth() {
    try {
      const response = await fetch(`${apiUrl}/health`);
      if (response.ok) {
        const data = await response.json();
        apiStatus = `connected (${data.status})`;
      } else {
        apiStatus = 'unavailable';
      }
    } catch (error) {
      apiStatus = 'disconnected';
    }
  }

  onMount(() => {
    checkApiHealth();
  });
</script>

<main>
  <div class="container">
    <h1>Hello Universe 🌌</h1>
    <p class="subtitle">PaperlessCore Development Environment</p>

    <div class="status">
      <h2>System Status</h2>
      <div class="status-item">
        <span class="label">Frontend:</span>
        <span class="value success">Running</span>
      </div>
      <div class="status-item">
        <span class="label">API:</span>
        <span class="value" class:success={apiStatus.includes('connected')} class:error={apiStatus.includes('disconnected')}>
          {apiStatus}
        </span>
      </div>
    </div>

    <div class="info">
      <h2>Environment</h2>
      <dl>
        <dt>Version</dt>
        <dd>0.1.0-dev</dd>
        <dt>API Endpoint</dt>
        <dd>{apiUrl}</dd>
        <dt>Build Time</dt>
        <dd>{new Date().toISOString()}</dd>
      </dl>
    </div>

    <div class="actions">
      <button on:click={checkApiHealth}>Refresh Status</button>
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
    display: flex;
    justify-content: center;
    align-items: center;
    min-height: 100vh;
    padding: 2rem;
  }

  .container {
    background: white;
    border-radius: 1rem;
    padding: 2rem;
    max-width: 600px;
    width: 100%;
    box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
  }

  h1 {
    margin: 0 0 0.5rem 0;
    font-size: 2.5rem;
    color: #667eea;
  }

  .subtitle {
    margin: 0 0 2rem 0;
    color: #666;
    font-size: 1.1rem;
  }

  .status, .info {
    margin: 2rem 0;
    padding: 1.5rem;
    background: #f7f7f7;
    border-radius: 0.5rem;
  }

  h2 {
    margin: 0 0 1rem 0;
    font-size: 1.2rem;
    color: #333;
  }

  .status-item {
    display: flex;
    justify-content: space-between;
    margin: 0.5rem 0;
    padding: 0.5rem 0;
    border-bottom: 1px solid #e0e0e0;
  }

  .status-item:last-child {
    border-bottom: none;
  }

  .label {
    font-weight: 600;
    color: #555;
  }

  .value {
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

  dl {
    margin: 0;
  }

  dt {
    font-weight: 600;
    color: #555;
    margin-top: 0.75rem;
  }

  dt:first-child {
    margin-top: 0;
  }

  dd {
    margin: 0.25rem 0 0 0;
    color: #666;
    font-family: 'Courier New', monospace;
    font-size: 0.9rem;
  }

  .actions {
    margin-top: 2rem;
    text-align: center;
  }

  button {
    background: #667eea;
    color: white;
    border: none;
    padding: 0.75rem 2rem;
    border-radius: 0.5rem;
    font-size: 1rem;
    cursor: pointer;
    transition: background 0.2s;
  }

  button:hover {
    background: #5568d3;
  }

  button:active {
    transform: scale(0.98);
  }
</style>
