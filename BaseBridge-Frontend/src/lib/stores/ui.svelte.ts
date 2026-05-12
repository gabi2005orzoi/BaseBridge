type ToastType = 'success' | 'error' | 'info';

interface Toast {
	id: string;
	message: string;
	type: ToastType;
}

type ModalType = 'create-endpoint' | 'review-query' | 'db-config' | 'ai-config' | 'test-api' | null;

interface ModalState{
	type: ModalType;
	data?: Record<string, unknown>;
}

function createUiStore(){
	let toasts = $state<Toast[]>([]);
	let modal = $state<ModalState>({type: null});
	let globalLoading = $state(false);

	function addToast(message: string, type: Toast['type'] = 'success', duration = 4000){
		const id = crypto.randomUUID();
		toasts = [...toasts, {id, message, type}];
		setTimeout(() => toasts = toasts.filter(t => t.id !== id), duration);
	}

	function openModal(type: ModalType, data?: Record<string, unknown>){
		modal = {type, data}
	}

	function closeModal(){
		modal = {type: null};
	}

	function setGlobalLoading(value: boolean){
		globalLoading = value;
	}

	return {
		get toasts() {return toasts;},
		get modal(){return modal},
		get isModalOpen() {return modal.type !== null},
		get globalLoading(){return globalLoading},
		success: (msg: string )=> addToast(msg, 'success'),
		error: (msg: string)=> addToast(msg, 'error'),
		info: (msg: string)=> addToast(msg, 'info'),
		openModal,
		closeModal,
		setGlobalLoading
	};
}

export const ui = createUiStore();