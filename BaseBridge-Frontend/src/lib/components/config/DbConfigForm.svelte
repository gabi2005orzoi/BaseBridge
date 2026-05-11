<script lang="ts">
	import { configStore } from '$lib/stores/config.svelte';
	import { ui } from '$lib/stores/ui.svelte';
	import Spinner from '$lib/components/ui/Spinner.svelte';

	let dbType = $state('sqlserver');
	let host = $state('');
	let dbName = $state('');
	let user = $state('');
	let password = $state('');

	async function handleSubmit(e: Event) {
		e.preventDefault();
		try {
			await configStore.saveDb({ dbType, host, dbName, user, password });
			ui.success('Database configuration saved successfully!');
		} catch{
			ui.error(configStore.error || `An error occurred while saving the DB configuration.`);
		}
	}
</script>

<div class="card">
	<h2 class="card-title">Database Configuration</h2>
	<p class="card-desc">Enter credentials to allow endpoint generation.</p>

	<form onsubmit={handleSubmit} class="form">
		<div class="form-group">
			<label for="dbType">Database Type</label>
			<select id="dbType" bind:value={dbType} required>
				<option value="sqlserver">SQL Server</option>
				<option value="postgresql">PostgreSQL</option>
				<option value="mysql">MySQL</option>
			</select>
		</div>

		<div class="form-group">
			<label for="host">Host / URL</label>
			<input type="text" id="host" bind:value={host} placeholder="ex: localhost, 127.0.0.1" required />
		</div>

		<div class="form-group">
			<label for="dbName">Database name</label>
			<input type="text" id="dbName" bind:value={dbName} required />
		</div>

		<div class="form-group">
			<label for="user">Username</label>
			<input type="text" id="user" bind:value={user} required />
		</div>

		<div class="form-group">
			<label for="password">Password</label>
			<input type="password" id="password" bind:value={password} required />
		</div>

		<button type="submit" class="btn-primary" disabled={configStore.loadingDb}>
			{#if configStore.loadingDb}
				<Spinner size="sm" />
			{:else}
				Save Connection
			{/if}
		</button>
	</form>
</div>

<style>
    .card {
        background: var(--color-background-primary);
        border: 1px solid var(--color-border-tertiary);
        border-radius: var(--border-radius-lg);
        padding: 1.5rem;
        box-shadow: 0 1px 3px rgba(0,0,0,0.05);
    }
    .card-title { margin-top: 0; font-size: 1.25rem; }
    .card-desc { color: var(--color-text-secondary); margin-bottom: 1.5rem; font-size: 0.9rem; }
    .form { display: flex; flex-direction: column; gap: 1rem; }
    .form-group { display: flex; flex-direction: column; gap: 0.25rem; }
    .form-group label { font-size: 0.85rem; font-weight: 500; color: var(--color-text-primary); }
    input, select {
        padding: 0.5rem;
        border: 1px solid var(--color-border-secondary);
        border-radius: var(--border-radius-md);
        background: var(--color-background-primary);
        color: var(--color-text-primary);
    }
    .btn-primary {
        background: var(--color-text-info);
        color: white;
        border: none;
        padding: 0.6rem 1rem;
        border-radius: var(--border-radius-md);
        cursor: pointer;
        font-weight: 500;
        display: flex;
        justify-content: center;
        align-items: center;
        gap: 0.5rem;
    }
    .btn-primary:disabled { opacity: 0.7; cursor: not-allowed; }
</style>