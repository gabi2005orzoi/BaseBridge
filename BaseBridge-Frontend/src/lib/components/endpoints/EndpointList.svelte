<script lang="ts">
	import { endpointsStore } from '$lib/stores/endpoint.svelte';
	import { configStore } from '$lib/stores/config.svelte';
	import EndpointCard from './EndpointCard.svelte';
	import Spinner from '$lib/components/ui/Spinner.svelte';
	import { onMount } from 'svelte';

	onMount(() => {
		if (configStore.isDbConfigured) {
			endpointsStore.load();
		}
	});
</script>

<div class="list-container">
	{#if !configStore.isDbConfigured}
		<div class="state-container empty">
			<svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
				<ellipse cx="12" cy="5" rx="9" ry="3"/>
				<path d="M21 12c0 1.66-4 3-9 3s-9-1.34-9-3"/>
				<path d="M3 5v14c0 1.66 4 3 9 3s9-1.34 9-3V5"/>
			</svg>
			<h3>Database not connected</h3>
			<p>Please configure the database connection to view your endpoints.</p>
			<button class="btn-retry" onclick={() => window.location.href = '/config'}>
				Go to Configuration
			</button>
		</div>
	{:else if endpointsStore.loading && endpointsStore.endpoints.length === 0}
		<div class="state-container">
			<Spinner size="lg" />
			<p>Loading endpoints...</p>
		</div>
	{:else if endpointsStore.error}
		<div class="state-container error">
			<p>{endpointsStore.error}</p>
			<button class="btn-retry" onclick={() => endpointsStore.load()}>Try again</button>
		</div>
	{:else if endpointsStore.count === 0}
		<div class="state-container empty">
			<svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
				<path d="M20 7h-3m3 4h-3m3 4h-3M4 5h8v14H4z" stroke-linecap="round" stroke-linejoin="round"/>
			</svg>
			<h3>No endpoint generated</h3>
			<p>Use the button above to generate your first AI endpoint.</p>
		</div>
	{:else}
		<div class="grid">
			{#each endpointsStore.endpoints as endpoint (endpoint.name)}
				<EndpointCard {endpoint} />
			{/each}
		</div>
	{/if}
</div>

<style>
    .state-container { display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 4rem 2rem; text-align: center; color: var(--color-text-secondary); background: var(--color-background-primary); border-radius: var(--border-radius-lg); border: 1px dashed var(--color-border-tertiary); }
    .state-container h3 { margin: 1rem 0 0.5rem; color: var(--color-text-primary); }
    .state-container.error { color: var(--color-text-danger); border-color: var(--color-border-danger); }

    .btn-retry { margin-top: 1rem; background: var(--color-background-secondary); border: 1px solid var(--color-border-secondary); padding: 0.5rem 1rem; border-radius: var(--border-radius-md); cursor: pointer; color: var(--color-text-primary); }

    .grid { display: grid; grid-template-columns: 1fr; gap: 1.5rem; }
    @media (min-width: 768px) {
        .grid { grid-template-columns: repeat(auto-fill, minmax(350px, 1fr)); }
    }
</style>