const BASE_URL ='http://localhost:5227';

async function request<T> (path: string, options: RequestInit = {}): Promise<T>{
	const res = await fetch(`${BASE_URL}${path}`, {
		...options,
		headers: {
			'Content-Type': 'application/json',
			'X-Admin-Api-Key': '24b00da1-e4a3-496c-b115-dbcd4201e693',
			...options.headers
		}
	});

	if(!res.ok){
		const err = await res.json().catch(() => ({message: res.statusText}));
		throw new Error(err.message ?? 'Request failed');
	}

	return res.json();
}

export const api = {
	get: <T>(path: string) => request<T>(path),
	post: <T>(path: string, body: unknown) => request<T>(path, {method: 'POST', body: JSON.stringify(body)}),
	delete: <T>(path: string, body: unknown) =>  request<T>(path, {method: 'DELETE', body: JSON.stringify(body)})
}