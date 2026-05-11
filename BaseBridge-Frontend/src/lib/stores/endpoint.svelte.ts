import type { EndpointData, EndpointParameter, GenerateEndpointRequest, SaveEndpointRequest } from '$lib/api/types';
import { adminApi } from '$lib/api/endpoint';

function createEndpointsStore(){
	let endpoints = $state<EndpointData[]>([]);
	let loading = $state(false);
	let generating = $state(false);
	let error = $state<string | null>(null);
	let pendingParameters = $state<EndpointParameter[] | null>(null);

	let pendingQuery = $state<string | null>(null);
	let pendingRequest = $state<GenerateEndpointRequest | null>(null);

	const count = $derived(endpoints.length);
	const hasPendingQuery = $derived(pendingQuery !== null);

	async function load(){
		loading = true;
		try{
			endpoints = await adminApi.getAllEndpoint();
		} catch (e){
			error = e instanceof Error ? e.message : 'Failed load';
		} finally {
			loading = false;
		}
	}

	async function generate(request: GenerateEndpointRequest){
		generating = true;
		error = null;
		try{
			const res = await adminApi.generateEndpoint(request);
			pendingQuery = res.generatedQuery;
			pendingParameters = res.parameters && res.parameters.length > 0 ? res.parameters : null;
			pendingRequest = request;
			return res.generatedQuery;
		} catch (e){
			error = e instanceof Error ? e.message : 'Failed to generate query';
		} finally {
			generating = false;
		}
	}

	async function confirmAndSave(request: SaveEndpointRequest){
		loading = true;
		error = null;
		try{
			await adminApi.saveEndpoint(request);
			endpoints = [...endpoints, {
				name: request.name,
				path: `api/data/${request.path}`,
				query: request.validatedQuery,
				description: request.endpointDescription,
				parameters: request.parameters
			}];
			clearPending();
		} catch (e){
			error = e instanceof Error ? e.message : 'Failed to save endpoint';
		} finally {
			loading = false;
		}
	}

	async function remove(name: string){
		try {
			await adminApi.deleteEndpoint(name);
			endpoints = endpoints.filter(ep => ep.name !== name);
		} catch (e){
			error = e instanceof Error ? e.message: 'Failed to delete the endpoint';
			throw e;
		}
	}

	function clearPending(){
		pendingQuery = null;
		pendingRequest = null;
		pendingParameters = null;
	}

	function clearError(){
		error = null;
	}

	return {
		get endpoints() {return endpoints;},
		get loading() {return loading;},
		get generating() {return generating;},
		get error() {return error;},
		get pendingQuery(){return pendingQuery;},
		get pendingRequest(){return pendingRequest;},
		get count(){return count;},
		get hasPendingQuery(){return hasPendingQuery;},
		get pendingParameters(){return pendingParameters},
		load,
		generate,
		confirmAndSave,
		remove,
		clearPending,
		clearError
	};
}

export const endpointsStore = createEndpointsStore();