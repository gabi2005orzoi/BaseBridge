<script lang="ts">
	import { ui } from '$lib/stores/ui.svelte';
	import { api } from '$lib/api/client';
	import type { EndpointData } from '$lib/api/types';
	import Spinner from '$lib/components/ui/Spinner.svelte';

	const endpoint = ui.modal.data as unknown as EndpointData;

	let params = $state<Record<string, string>>({});
	let response = $state<any>(null);
	let loading = $state(false);
	let error = $state<string | null>(null);

	$effect(() => {
		if (endpoint.parameters) {
			endpoint.parameters.forEach(p => {
				if (!(p.name in params)) params[p.name] = '';
			});
		}
	});

	async function runTest() {
		loading = true;
		error = null;
		response = null;

		try {
			const searchParams = new URLSearchParams(params);
			const queryString = searchParams.toString();
			const fullPath = `${endpoint.path}${queryString ? '?' + queryString : ''}`;

			response = await api.get(fullPath);
		} catch (e) {
			error = e instanceof Error ? e.message : 'An error appeared while calling the API';
		} finally {
			loading = false;
		}
	}
</script>

<div class="modal-backdrop" onclick={ui.closeModal}>
	<div class="modal-content" onclick={e => e.stopPropagation()}>
		<div class="modal-header">
			<h2>Test Endpoint: {endpoint.name}</h2>
			<button class="btn-close" onclick={ui.closeModal}>&times;</button>
		</div>

		<div class="modal-body">
			<p class="endpoint-path"><code>GET {endpoint.path}</code></p>

			{#if endpoint.parameters && endpoint.parameters.length > 0}
				<div class="params-section">
					<h3>Parameters</h3>
					<div class="params-grid">
						{#each endpoint.parameters as param}
							<div class="param-input">
								<label for={param.name}>{param.name}</label>
								<input
									id={param.name}
									type="text"
									bind:value={params[param.name]}
									placeholder="value..."
								/>
							</div>
						{/each}
					</div>
				</div>
			{/if}

			<button class="btn-run" onclick={runTest} disabled={loading}>
				{#if loading}<Spinner size="sm" />{:else}Execute Request{/if}
			</button>

			<div class="response-section">
				<h3>Server Response</h3>
				<div class="response-container {error ? 'error' : ''}">
					{#if loading}
						<p>Loading...</p>
					{:else if error}
						<p class="error-text">{error}</p>
					{:else if response}
						<pre><code>{JSON.stringify(response, null, 2)}</code></pre>
					{:else}
						<p class="placeholder">Press "Execute Request" for the data.</p>
					{/if}
				</div>
			</div>
		</div>
	</div>
</div>

<style>
    .modal-backdrop { position: fixed; inset: 0; background: rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center; z-index: 1000; padding: 1rem; }
    .modal-content { background: var(--color-background-primary); border-radius: 12px; width: 100%; max-width: 700px; max-height: 90vh; overflow-y: auto; box-shadow: 0 20px 25px -5px rgba(0,0,0,0.1); }
    .modal-header { padding: 1.5rem; border-bottom: 1px solid var(--color-border-tertiary); display: flex; justify-content: space-between; align-items: center; }
    .modal-body { padding: 1.5rem; display: flex; flex-direction: column; gap: 1.5rem; }

    .endpoint-path { background: var(--color-background-secondary); padding: 0.75rem; border-radius: 6px; margin: 0; }
    .params-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
    .param-input { display: flex; flex-direction: column; gap: 0.25rem; }
    .param-input label { font-size: 0.85rem; font-weight: 600; color: var(--color-text-secondary); }
    .param-input input { padding: 0.5rem; border: 1px solid var(--color-border-tertiary); border-radius: 4px; background: var(--color-background-primary); color: var(--color-text-primary); }

    .btn-run { background: var(--color-text-success); color: white; border: none; padding: 0.75rem; border-radius: 6px; font-weight: 600; cursor: pointer; }
    .btn-run:disabled { opacity: 0.6; cursor: not-allowed; }

    .response-container { background: #1e1e1e; color: #d4d4d4; padding: 1rem; border-radius: 6px; min-height: 100px; font-family: monospace; font-size: 0.9rem; overflow-x: auto; }
    .response-container.error { border: 1px solid var(--color-border-danger); background: rgba(239, 68, 68, 0.05); }
    .error-text { color: #f87171; }
    .placeholder { color: #6b7280; font-style: italic; }
    .btn-close { background: none; border: none; font-size: 1.5rem; cursor: pointer; color: var(--color-text-secondary); }
</style>
