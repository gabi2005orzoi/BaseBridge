<script lang="ts">
	import '../app.css';
	import Toast from '$lib/components/ui/Toast.svelte';
	import Sidebar from '$lib/components/layout/Sidebar.svelte';
	import Header from '$lib/components/layout/Header.svelte';
	import CreateEndpointModal from '$lib/components/endpoints/CreateEndpointModal.svelte';
	import QueryReviewModal from '$lib/components/endpoints/QueryReviewModal.svelte';
	import { ui } from '$lib/stores/ui.svelte';
	import { onMount } from 'svelte';
	import { configStore } from '$lib/stores/config.svelte';

	let { children } = $props();

	onMount(async () => {
		await configStore.loadStatus();
	});
</script>

<Toast />

{#if ui.modal.type === 'create-endpoint'}
	<CreateEndpointModal />
{:else if ui.modal.type === 'review-query'}
	<QueryReviewModal />
{/if}

<div class="app-layout">
	<Sidebar />

	<div class="main-wrapper">
		<Header />

		<main class="main-content">
			{@render children()}
		</main>
	</div>
</div>

<style>
    .app-layout {
        display: flex;
        min-height: 100vh;
        background-color: var(--color-background-secondary);
    }

    .main-wrapper {
        flex: 1;
        display: flex;
        flex-direction: column;
        min-width: 0;
    }

    .main-content {
        flex: 1;
        padding: 2rem;
        max-width: 1400px;
        width: 100%;
        margin: 0 auto;
    }
</style>