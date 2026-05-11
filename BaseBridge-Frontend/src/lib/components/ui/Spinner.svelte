<script lang="ts">
  interface Props {
    size?: 'sm' | 'md' | 'lg';
    label?: string;
  }

  const { size = 'md', label = 'Se încarcă...' }: Props = $props();

  const sizes = {
    sm: 'w-4 h-4 border-2',
    md: 'w-6 h-6 border-2',
    lg: 'w-10 h-10 border-[3px]',
  };
</script>

<span class="spinner-wrap" role="status" aria-label={label}>
  <span class={`spinner ${sizes[size]}`}></span>
</span>

<style>
  .spinner-wrap {
    display: inline-flex;
    align-items: center;
    justify-content: center;
  }

  .spinner {
    display: inline-block;
    border-radius: 50%;
    border-color: var(--color-border-secondary);
    border-top-color: var(--color-text-primary);
    animation: spin 0.7s linear infinite;
  }

  /* Tailwind classes pentru w/h/border sunt definite în props,
     dar dacă nu folosești Tailwind, folosește asta: */
  .w-4 { width: 1rem; }
  .h-4 { height: 1rem; }
  .w-6 { width: 1.5rem; }
  .h-6 { height: 1.5rem; }
  .w-10 { width: 2.5rem; }
  .h-10 { height: 2.5rem; }
  .border-2 { border-width: 2px; border-style: solid; }
  .border-\[3px\] { border-width: 3px; border-style: solid; }

  @keyframes spin {
    to { transform: rotate(360deg); }
  }
</style>