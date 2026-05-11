<script lang="ts">
	import { configStore } from '$lib/stores/config.svelte';
	import { ui } from '$lib/stores/ui.svelte';
	import Spinner from '$lib/components/ui/Spinner.svelte';

	// Schimbăm starea implicită pe groq
	let provider = $state('groq');
	let apiKey = $state('');
	let modelName = $state('');

	async function handleSubmit(e: Event) {
		e.preventDefault();
		try {
			// Adăugăm 'groq' în lista care nu trimite API key dacă folosește cheia de server
			const finalApiKey = ['groq', 'deepseek', 'gemini'].includes(provider) ? '' : apiKey;
			await configStore.saveAi({ provider, apiKey: finalApiKey, modelName });
			ui.success('AI configuration has been updated!');
		} catch {
			ui.error(configStore.error || 'An error occurred while saving the AI configuration.');
		}
	}
</script>

<div class="card">
	<h2 class="card-title">AI Model Configuration</h2>
	<p class="card-desc">Leave the default to use Groq (built-in), or add your own provider.</p>

	<form onsubmit={handleSubmit} class="form">
		<div class="form-group">
			<label for="provider">Provider AI</label>
			<select id="provider" bind:value={provider}>
				<!-- Am adăugat Groq ca default pe prima poziție -->
				<option value="groq">Groq (Default Server Key)</option>
				<option value="groq-custom">Groq (Custom Key)</option>
				<option value="deepseek">DeepSeek (Server Key)</option>
				<option value="deepseek-custom">DeepSeek (Custom Key)</option>
				<option value="gemini">Google Gemini (Server Key)</option>
				<option value="gemini-custom">Google Gemini (Custom Key)</option>
				<option value="openai">OpenAI (ChatGPT)</option>
				<option value="anthropic">Anthropic (Claude)</option>
			</select>
		</div>

		<!-- Câmpul API Key apare doar dacă provider-ul NU folosește o cheie hardcodată pe server -->
		{#if !['groq', 'deepseek', 'gemini'].includes(provider)}
			<div class="form-group">
				<label for="apiKey">API Key</label>
				<input type="password" id="apiKey" bind:value={apiKey} required placeholder="sk-..." />
			</div>
		{/if}

		<div class="form-group">
			<label for="modelName">Model Name</label>
			<!-- Ternar actualizat pentru a oferi placeholder-ul corect pentru Groq -->
			<input
				type="text"
				id="modelName"
				bind:value={modelName}
				placeholder={provider.includes('groq') ? "ex: llama-3.3-70b-versatile" : provider.includes('deepseek') ? "ex: deepseek-chat" : "ex: gpt-4-turbo"}
			/>
		</div>

		<button type="submit" class="btn-primary" disabled={configStore.loadingAi}>
			{#if configStore.loadingAi}
				<Spinner size="sm" />
			{:else}
				Save AI Configuration
			{/if}
		</button>
	</form>
</div>

<style>
    .card { background: var(--color-background-primary); border: 1px solid var(--color-border-tertiary); border-radius: var(--border-radius-lg); padding: 1.5rem; }
    .card-title { margin-top: 0; font-size: 1.25rem; }
    .card-desc { color: var(--color-text-secondary); margin-bottom: 1.5rem; font-size: 0.9rem; }
    .form { display: flex; flex-direction: column; gap: 1rem; }
    .form-group { display: flex; flex-direction: column; gap: 0.25rem; }
    .form-group label { font-size: 0.85rem; font-weight: 500; color: var(--color-text-primary); }
    input, select { padding: 0.5rem; border: 1px solid var(--color-border-secondary); border-radius: var(--border-radius-md); background: var(--color-background-primary); color: var(--color-text-primary); }
    .btn-primary { background: var(--color-text-info); color: white; border: none; padding: 0.6rem 1rem; border-radius: var(--border-radius-md); cursor: pointer; font-weight: 500; display: flex; justify-content: center; gap: 0.5rem; }
</style>