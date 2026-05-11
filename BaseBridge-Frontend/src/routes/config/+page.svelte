<script lang="ts">
	import DbConfigForm from '$lib/components/config/DbConfigForm.svelte';
	import AiConfigForm from '$lib/components/config/AiConfigForm.svelte';
	import { configStore } from '$lib/stores/config.svelte';
</script>

<svelte:head>
	<title>Configuration</title>
</svelte:head>

<div class="config-page">
	<header class="config-header">
		<h1>Initial Setup</h1>
		<p>To be able to generate endpoints, you must connect the application to the database.</p>
	</header>

	<div class="config-grid">
		<DbConfigForm />

		<AiConfigForm />
	</div>

	{#if configStore.isDbConfigured}
		<div class="success-actions">
			<button onclick={() => window.location.href = '/'} class="btn-continue" style="border: none; cursor: pointer;">
				Go to the endpoints panel &rarr;
			</button>
		</div>
	{/if}
</div>

<style>
    .config-page {
        max-width: 900px;
        margin: 0 auto;
    }

    .config-header {
        margin-bottom: 2rem;
        text-align: center;
    }

    .config-header h1 { margin-bottom: 0.5rem; color: var(--color-text-primary); }
    .config-header p { color: var(--color-text-secondary); margin-top: 0; }

    .config-grid {
        display: grid;
        grid-template-columns: 1fr;
        gap: 2rem;
    }

    @media (min-width: 768px) {
        .config-grid {
            grid-template-columns: 1fr 1fr;
            align-items: start;
        }
    }

    .success-actions {
        margin-top: 2rem;
        text-align: center;
    }

    .btn-continue {
        display: inline-block;
        background: var(--color-text-success);
        color: white;
        padding: 0.75rem 1.5rem;
        text-decoration: none;
        border-radius: var(--border-radius-md);
        font-weight: 500;
    }
</style>