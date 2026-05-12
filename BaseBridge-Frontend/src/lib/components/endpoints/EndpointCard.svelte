<script lang="ts">
	import type { EndpointData } from '$lib/api/types';
	import { endpointsStore } from '$lib/stores/endpoint.svelte';
	import { ui } from '$lib/stores/ui.svelte';

	interface Props {
		endpoint: EndpointData;
	}

	let { endpoint }: Props = $props();
	let isDeleting = $state(false);

	async function handleDelete() {
		if (!confirm(`Are you sure you want to delete the endpoint "${endpoint.name}"?`)) return;

		isDeleting = true;
		try {
			await endpointsStore.remove(endpoint.name);
			ui.success('Endpoint deleted successfully!');
		} catch {
			ui.error(endpointsStore.error || 'Error deleting endpoint');
		} finally {
			isDeleting = false;
		}
	}

	function handleTest(){
		ui.openModal('test-api', endpoint as unknown as Record<string, unknown>);
	}

</script>

<div class="card endpoint-card">
	<div class="card-header">
		<h3 class="title">{endpoint.name}</h3>
		<div class="actions">
			<button class="btn-test" onclick={handleTest}>Test API</button>
			<button class="btn-delete" onclick={handleDelete} disabled={isDeleting} aria-label="Delete endpoint">
				{isDeleting ? '...' : 'Delete'}
			</button>
		</div>
	</div>

	<div class="info-row">
		<span class="label">Path:</span>
		<code class="path-code">{endpoint.path}</code>
	</div>

	{#if endpoint.parameters && endpoint.parameters.length > 0}
		<div class="info-row">
			<span class="label">Required parameters</span>
			<div class="tags-container">
				{#each endpoint.parameters as param (param.name)}
					<span class="param-tag">?{param.name}=value</span>
				{/each}
			</div>
		</div>
	{/if}

	<p class="desc">{endpoint.description}</p>

	<details class="query-details">
		<summary>See the generated query</summary>
		<pre class="query-code"><code>{endpoint.query}</code></pre>
	</details>
</div>

<style>
    .endpoint-card {
        background: var(--color-background-primary);
        border: 1px solid var(--color-border-tertiary);
        border-radius: var(--border-radius-lg);
        padding: 1.5rem;
        display: flex;
        flex-direction: column;
        gap: 1rem;
    }
    .card-header { display: flex; justify-content: space-between; align-items: flex-start; }
    .title { margin: 0; font-size: 1.1rem; color: var(--color-text-primary); }
    .info-row { display: flex; align-items: center; gap: 0.5rem; }
    .label { font-size: 0.85rem; font-weight: 500; color: var(--color-text-secondary); }
    .path-code { background: var(--color-background-secondary); padding: 0.2rem 0.5rem; border-radius: 4px; font-family: var(--font-mono); font-size: 0.85rem; color: var(--color-text-info); }
    .desc { margin: 0; font-size: 0.95rem; color: var(--color-text-secondary); line-height: 1.4; }

    .query-details summary { cursor: pointer; font-size: 0.9rem; font-weight: 500; color: var(--color-text-info); margin-bottom: 0.5rem; }
    .query-code { background: var(--color-background-secondary); padding: 1rem; border-radius: var(--border-radius-md); font-family: var(--font-mono); font-size: 0.85rem; overflow-x: auto; margin: 0; color: var(--color-text-primary); border: 1px solid var(--color-border-tertiary); }

    .btn-delete { background: var(--color-background-danger); color: var(--color-text-danger); border: 1px solid var(--color-border-danger); padding: 0.4rem 0.8rem; border-radius: var(--border-radius-md); cursor: pointer; font-size: 0.85rem; transition: all 0.2s; }
    .btn-delete:hover:not(:disabled) { background: var(--color-text-danger); color: white; }
    .btn-delete:disabled { opacity: 0.5; cursor: not-allowed; }

    .tags-container {
        display: flex; gap: 0.5rem; flex-wrap: wrap;
    }
    .param-tag {
        background: rgba(59, 130, 246, 0.1);
        color: var(--color-text-info);
        padding: 0.2rem 0.6rem;
        border-radius: 9999px;
        font-size: 0.8rem;
        font-family: var(--font-mono);
        border: 1px solid rgba(59, 130, 246, 0.2);
    }

    .actions { display: flex; gap: 0.5rem; }
    .btn-test {
        background: var(--color-background-info);
        color: var(--color-text-info);
        border: 1px solid var(--color-border-info);
        padding: 0.4rem 0.8rem;
        border-radius: var(--border-radius-md);
        cursor: pointer;
        font-size: 0.85rem;
    }
    .btn-test:hover { background: var(--color-text-info); color: white; }
</style>