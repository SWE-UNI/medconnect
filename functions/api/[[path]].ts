// Cloudflare Pages Function: reverse-proxies /api/* from the HTTPS SPA origin
// to the HTTP-only backend (medconnect.runasp.net). The browser never talks to
// the HTTP origin directly (mixed content is blocked), so this runs server-side
// on the Cloudflare edge where HTTP fetch is allowed.
const API_ORIGIN = 'http://medconnect.runasp.net';

interface EventContext {
  request: Request;
}

export async function onRequest(context: EventContext): Promise<Response> {
  const { request } = context;
  const url = new URL(request.url);

  if (String(request.headers.get('upgrade') || '').toLowerCase() === 'websocket') {
    return new Response(
      JSON.stringify({ error: 'WebSocket (chat) is not supported through the API proxy yet.' }),
      { status: 502, headers: { 'content-type': 'application/json' } }
    );
  }

  const target = new URL(API_ORIGIN + url.pathname + url.search);

  const headers = new Headers();
  request.headers.forEach((value: string, key: string) => {
    if (key.toLowerCase() !== 'host') headers.set(key, value);
  });

  const init: RequestInit = { method: request.method, headers };
  if (request.method !== 'GET' && request.method !== 'HEAD') {
    init.body = await request.arrayBuffer();
  }

  const upstream = await fetch(target.toString(), init);

  const responseHeaders = new Headers();
  upstream.headers.forEach((value: string, key: string) => {
    const lower = key.toLowerCase();
    if (lower !== 'content-encoding' && lower !== 'transfer-encoding') {
      responseHeaders.set(key, value);
    }
  });

  return new Response(upstream.body, { status: upstream.status, headers: responseHeaders });
}