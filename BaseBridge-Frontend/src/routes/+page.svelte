<script lang="ts">
	import { ui } from '$lib/stores/ui.svelte';
	import {configStore} from '$lib/stores/config.svelte';
	import EndpointList from '$lib/components/endpoints/EndpointList.svelte';

	function handleCreateClick() {
		ui.openModal('create-endpoint');
	}
</script>

<svelte:head>
	<title>Dashboard - BaseBridge</title>
</svelte:head>

<div class="dashboard">
	<header class="dashboard-header">
		<div>
			<h1 class="title">Endpoints</h1>
			<p class="subtitle">Manages AI-generated data extraction scripts.</p>
		</div>

		<button
			class="btn-create"
			onclick={handleCreateClick}
			disabled={!configStore.isDbConfigured}
			title={!configStore.isDbConfigured ? "Please connect to a database first" : ""}>
			<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
				<path d="M12 5v14M5 12h14" stroke-linecap="round" stroke-linejoin="round"/>
			</svg>
			Add Endpoint
		</button>
	</header>

	<main>
		<EndpointList />
	</main>
</div>

<style>
    .dashboard { max-width: 1200px; margin: 0 auto; }

    .dashboard-header { display: flex; flex-direction: column; gap: 1.5rem; margin-bottom: 2rem; }
    @media (min-width: 640px) {
        .dashboard-header { flex-direction: row; justify-content: space-between; align-items: center; }
    }

    .title { margin: 0 0 0.5rem 0; color: var(--color-text-primary); font-size: 1.75rem; }
    .subtitle { margin: 0; color: var(--color-text-secondary); }

    .btn-create { display: inline-flex; align-items: center; gap: 0.5rem; background: var(--color-text-success); color: white; border: none; padding: 0.75rem 1.25rem; border-radius: var(--border-radius-md); font-weight: 600; cursor: pointer; transition: filter 0.2s; box-shadow: 0 2px 4px rgba(22, 163, 74, 0.2); }
    .btn-create:hover { filter: brightness(1.1); }

    .btn-create:disabled {
        opacity: 0.6;
        cursor: not-allowed;
        filter: grayscale(0.5);
        box-shadow: none;
    }

    .btn-create:disabled:hover {
        filter: grayscale(0.5);
    }
</style>