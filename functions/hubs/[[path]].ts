// Cloudflare Pages Function: reverse-proxies /hubs/* from the HTTPS SPA origin
// to the backend (medconnect.runasp.net).
const API_ORIGIN = 'http://medconnect.runasp.net';

interface EventContext {
  request: Request;
}

export async function onRequest(context: EventContext): Promise<Response> {
  const { request } = context;
  const url = new URL(request.url);

  // Handle CORS preflight
  if (request.method === 'OPTIONS') {
    const origin = request.headers.get('Origin') || '*';
    return new Response(null, {
      status: 204,
      headers: {
        'Access-Control-Allow-Origin': origin,
        'Access-Control-Allow-Methods': 'GET, POST, OPTIONS',
        'Access-Control-Allow-Headers': request.headers.get('Access-Control-Request-Headers') || '*',
        'Access-Control-Allow-Credentials': 'true',
        'Access-Control-Max-Age': '86400',
      },
    });
  }

  // If the client requests a WebSocket upgrade, forward it
  if (String(request.headers.get('upgrade') || '').toLowerCase() === 'websocket') {
    const wsTarget = new URL(API_ORIGIN + url.pathname + url.search);
    try {
      return await fetch(wsTarget.toString(), request);
    } catch (err) {
      return new Response('WebSocket upgrade failed: ' + String(err), { status: 502 });
    }
  }

  const target = new URL(API_ORIGIN + url.pathname + url.search);

  const headers = new Headers();
  request.headers.forEach((value: string, key: string) => {
    const lower = key.toLowerCase();
    if (lower !== 'host' && lower !== 'cf-connecting-ip' && lower !== 'cf-ray') {
      headers.set(key, value);
    }
  });

  const init: RequestInit = { method: request.method, headers };
  if (request.method !== 'GET' && request.method !== 'HEAD') {
    const body = await request.arrayBuffer();
    if (body.byteLength > 0) {
      init.body = body;
      headers.set('content-length', body.byteLength.toString());
    } else {
      headers.set('content-length', '0');
    }
  }

  const upstream = await fetch(target.toString(), init);

  const responseHeaders = new Headers();
  upstream.headers.forEach((value: string, key: string) => {
    const lower = key.toLowerCase();
    if (lower !== 'content-encoding' && lower !== 'transfer-encoding') {
      responseHeaders.set(key, value);
    }
  });

  const origin = request.headers.get('Origin');
  if (origin) {
    responseHeaders.set('Access-Control-Allow-Origin', origin);
    responseHeaders.set('Access-Control-Allow-Credentials', 'true');
  }

  return new Response(upstream.body, { status: upstream.status, headers: responseHeaders });
}
