<script lang="ts">
	import { ui } from '$lib/stores/ui.svelte';
</script>

<div class="toast-container" aria-live="polite" aria-label="Notificări">
	{#each ui.toasts as toast (toast.id)}
		<div class="toast toast--{toast.type}" role="alert">
			<span class="toast__icon" aria-hidden="true">
				{#if toast.type === 'success'}
					<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
						<path d="M20 6L9 17l-5-5"/>
					</svg>
				{:else if toast.type === 'error'}
					<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
						<circle cx="12" cy="12" r="10"/>
						<path d="M15 9l-6 6M9 9l6 6"/>
					</svg>
				{:else}
					<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
						<circle cx="12" cy="12" r="10"/>
						<path d="M12 16v-4M12 8h.01"/>
					</svg>
				{/if}
			</span>
			<span class="toast__message">{toast.message}</span>
		</div>
	{/each}
</div>

<style>
	.toast-container {
		position: fixed;
		bottom: 1.5rem;
		right: 1.5rem;
		z-index: 9999;
		display: flex;
		flex-direction: column;
		gap: 0.6rem;
		pointer-events: none;
		max-width: 360px;
		width: calc(100vw - 3rem);
	}

	.toast {
		display: flex;
		align-items: center;
		gap: 0.75rem;
		padding: 0.75rem 1rem;
		border-radius: 10px;
		border: 1px solid transparent;
		font-size: 0.875rem;
		font-weight: 500;
		line-height: 1.4;
		pointer-events: all;
		backdrop-filter: blur(12px);
		box-shadow:
			0 4px 6px -1px rgba(0, 0, 0, 0.3),
			0 10px 25px -5px rgba(0, 0, 0, 0.2);
		animation: slide-in 0.25s cubic-bezier(0.16, 1, 0.3, 1) both;
	}

	@keyframes slide-in {
		from {
			opacity: 0;
			transform: translateX(calc(100% + 1.5rem));
		}
		to {
			opacity: 1;
			transform: translateX(0);
		}
	}

	/* Success */
	.toast--success {
		background: rgba(16, 185, 129, 0.15);
		border-color: rgba(16, 185, 129, 0.35);
		color: #34d399;
	}
	.toast--success .toast__icon {
		color: #10b981;
		flex-shrink: 0;
	}

	/* Error */
	.toast--error {
		background: rgba(239, 68, 68, 0.15);
		border-color: rgba(239, 68, 68, 0.35);
		color: #fca5a5;
	}
	.toast--error .toast__icon {
		color: #ef4444;
		flex-shrink: 0;
	}

	/* Info */
	.toast--info {
		background: rgba(59, 130, 246, 0.15);
		border-color: rgba(59, 130, 246, 0.35);
		color: #93c5fd;
	}
	.toast--info .toast__icon {
		color: #3b82f6;
		flex-shrink: 0;
	}

	.toast__message {
		flex: 1;
		color: var(--color-text-primary);
	}
</style>