<script lang="ts">
	import { configStore } from '$lib/stores/config.svelte';
	import { page } from '$app/stores';

	let pageTitle = $derived(
		($page.url.pathname as string).includes('/config') ? 'Initial Configuration' :
		($page.url.pathname as string).includes('/schema') ? 'Database Visualizer' :
		'Dashboard'
	);
</script>

<header class="header">
	<div class="header__left">
		<h2 class="page-title">{pageTitle}</h2>
	</div>

	<div class="header__right">
		<div class="status-badge" class:configured={configStore.isDbConfigured}>
			<span class="indicator"></span>
			{configStore.isDbConfigured ? 'DB Connected' : 'DB Unconnected'}
		</div>
	</div>
</header>

<style>
    .header {
        height: 70px;
        background: var(--color-background-primary);
        border-bottom: 1px solid var(--color-border-tertiary);
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: 0 2rem;
        position: sticky;
        top: 0;
        z-index: 10;
    }

    .page-title {
        margin: 0;
        font-size: 1.1rem;
        font-weight: 600;
        color: var(--color-text-primary);
    }

    .status-badge {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        padding: 0.4rem 0.8rem;
        border-radius: 99px;
        font-size: 0.85rem;
        font-weight: 500;
        background: var(--color-background-danger);
        color: var(--color-text-danger);
        border: 1px solid var(--color-border-danger);
    }

    .status-badge.configured {
        background: var(--color-background-success);
        color: var(--color-text-success);
        border: 1px solid var(--color-border-success);
    }

    .indicator {
        width: 8px;
        height: 8px;
        border-radius: 50%;
        background: currentColor;
    }
</style>