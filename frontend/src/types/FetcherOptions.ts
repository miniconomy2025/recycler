type JsonBody = Record<string, unknown>;

export type FetcherOptions = Omit<RequestInit, 'body'> & {
  body?: JsonBody | BodyInit | null;
};