import { FetcherOptions } from "../types/FetcherOptions";

const BASE_URL = 'https://recycler-api.projects.bbdgrad.com/internal';

export const fetcher = (path: string, options: FetcherOptions = {}) => {
  const { body, headers = {}, ...rest } = options;

  const finalBody =
    body && typeof body === 'object' && !(body instanceof FormData)
      ? JSON.stringify(body)
      : body;

  return fetch(`${BASE_URL}${path}`, {
    credentials: 'include',
    headers: {
      'Content-Type': 'application/json',
      ...headers,
    },
    body: finalBody,
    ...rest,
  }).then(async (res) => {
    if (!res.ok) {
      const error = await res.json().catch(() => ({}));
      return Promise.reject({ status: res.status, ...error });
    }
    return await res.json();
  });
};
