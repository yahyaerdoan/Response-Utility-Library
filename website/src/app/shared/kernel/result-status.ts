export type StatusClass = 1 | 2 | 3 | 4 | 5;

export type FactoryShape =
  | 'none'
  | 'title'
  | 'location'
  | 'detailAsTitle'
  | 'detail'
  | 'detailWithDefault';

export interface ResultStatusInfo {
  readonly name: string;
  readonly code: number;
  readonly statusClass: StatusClass;
  readonly isSuccessful: boolean;
  readonly factory: FactoryShape;
  readonly hasGenericFactory: boolean;
  readonly defaultTitle: string;
  readonly defaultDetail: string | null;
  readonly locationArgument: string | null;
  readonly problemType: string;
  readonly summary: string;
}

type Row = readonly [name: string, code: number, factory: FactoryShape, title: string, detail: string | null, rfc: string | null, summary: string];

const RFC = 'https://tools.ietf.org/html/';

const ROWS: readonly Row[] = [
  ['Continue', 100, 'title', 'Continue.', null, null, 'Interim response: the client should continue sending the request body.'],
  ['SwitchingProtocols', 101, 'title', 'Switching protocols.', null, null, 'The server agrees to switch protocols (e.g. to WebSocket).'],
  ['Processing', 102, 'title', 'Processing.', null, null, 'WebDAV interim response: the request is still being processed.'],
  ['EarlyHints', 103, 'title', 'Early hints.', null, null, 'Lets the client start preloading resources before the final response.'],

  ['Ok', 200, 'title', 'Operation completed successfully.', null, null, 'Standard success. Built with `Result.Success()` / `Result.Success(data)`.'],
  ['Created', 201, 'title', 'Resource created successfully.', null, null, 'A new resource was created; typical for POST endpoints.'],
  ['Accepted', 202, 'title', 'Request accepted for processing.', null, null, 'Work was queued and will complete asynchronously.'],
  ['NonAuthoritativeInformation', 203, 'title', 'Non-authoritative information.', null, null, 'Payload was modified by a transforming proxy.'],
  ['NoContent', 204, 'none', 'No content.', null, null, 'Success with nothing to return, e.g. updates and deletes.'],
  ['ResetContent', 205, 'none', 'Reset content.', null, null, 'Success; the client should reset the document view (e.g. clear a form).'],
  ['PartialContent', 206, 'title', 'Partial content.', null, null, 'Only part of the resource is returned (range requests).'],
  ['MultiStatus', 207, 'title', 'Multi-status.', null, null, 'WebDAV: the body describes the status of several independent operations.'],
  ['AlreadyReported', 208, 'title', 'Already reported.', null, null, 'WebDAV: members were already enumerated in a previous part of the response.'],
  ['ImUsed', 226, 'title', 'IM used.', null, null, 'Delta encoding: the response is the result of instance manipulations.'],

  ['MultipleChoices', 300, 'detailAsTitle', '{0}', null, null, 'Several representations exist; the text you pass becomes the title.'],
  ['MovedPermanently', 301, 'location', 'Resource moved permanently to: {0}', null, null, 'The resource has a new permanent URI.'],
  ['Found', 302, 'location', 'Resource found at: {0}', null, null, 'The resource temporarily lives at another URI.'],
  ['SeeOther', 303, 'location', 'See other resource at: {0}', null, null, 'Redirect to another URI with GET (classic POST/redirect/GET).'],
  ['NotModified', 304, 'none', 'Not modified.', null, null, 'Conditional GET: the client cache is still valid. Always bodyless.'],
  ['UseProxy', 305, 'location', 'Requested resource must be accessed through the proxy: {0}', null, null, 'Deprecated in HTTP, kept for completeness.'],
  ['TemporaryRedirect', 307, 'location', 'Temporarily redirected to: {0}', null, null, 'Temporary redirect that keeps the HTTP method.'],
  ['PermanentRedirect', 308, 'location', 'Permanently redirected to: {0}', null, null, 'Permanent redirect that keeps the HTTP method.'],

  ['BadRequest', 400, 'detail', 'Bad Request', null, 'rfc9110#section-15.5.1', 'The request is malformed or semantically wrong.'],
  ['Unauthorized', 401, 'detailWithDefault', 'Unauthorized', 'Authentication is required to access this resource.', 'rfc9110#section-15.5.2', 'The caller is not authenticated.'],
  ['PaymentRequired', 402, 'detail', 'Payment Required', null, 'rfc9110#section-15.5.3', 'Payment or quota is needed; handy for billing and plan limits.'],
  ['Forbidden', 403, 'detailWithDefault', 'Forbidden', 'You do not have permission to access this resource.', 'rfc9110#section-15.5.4', 'Authenticated, but not allowed.'],
  ['NotFound', 404, 'detail', 'Not Found', null, 'rfc9110#section-15.5.5', 'The requested resource does not exist.'],
  ['MethodNotAllowed', 405, 'detailWithDefault', 'Method Not Allowed', 'The HTTP method used is not allowed for this endpoint.', 'rfc9110#section-15.5.6', 'The HTTP verb is not supported by the target resource.'],
  ['NotAcceptable', 406, 'detailWithDefault', 'Not Acceptable', 'The requested media type is not supported by this server.', 'rfc9110#section-15.5.7', 'No representation matches the Accept headers.'],
  ['ProxyAuthenticationRequired', 407, 'detailWithDefault', 'Proxy Authentication Required', 'Proxy authentication is required to access this resource.', 'rfc9110#section-15.5.8', 'The client must authenticate with a proxy.'],
  ['RequestTimeout', 408, 'detailWithDefault', 'Request Timeout', 'The request timed out. Please try again.', 'rfc9110#section-15.5.9', 'The server gave up waiting for the request.'],
  ['Conflict', 409, 'detail', 'Conflict', null, 'rfc9110#section-15.5.10', 'State conflict: duplicates, concurrency tokens, invalid transitions.'],
  ['Gone', 410, 'detail', 'Gone', null, 'rfc9110#section-15.5.11', 'The resource existed but was permanently removed.'],
  ['LengthRequired', 411, 'detailWithDefault', 'Length Required', 'A Content-Length header is required for this request.', 'rfc9110#section-15.5.12', 'The request needs a Content-Length header.'],
  ['PreconditionFailed', 412, 'detail', 'Precondition Failed', null, 'rfc9110#section-15.5.13', 'An If-Match / If-Unmodified-Since precondition failed (optimistic concurrency).'],
  ['ContentTooLarge', 413, 'detail', 'Content Too Large', null, 'rfc9110#section-15.5.14', 'The payload exceeds what the server accepts (upload limits).'],
  ['UriTooLong', 414, 'detailWithDefault', 'URI Too Long', 'The request URI exceeds the maximum allowed length.', 'rfc9110#section-15.5.15', 'The request target is longer than the server will interpret.'],
  ['UnsupportedMediaType', 415, 'detailWithDefault', 'Unsupported Media Type', 'The submitted media type is not supported. Please use a supported type.', 'rfc9110#section-15.5.16', 'The Content-Type is not supported.'],
  ['RangeNotSatisfiable', 416, 'detail', 'Range Not Satisfiable', null, 'rfc9110#section-15.5.17', 'The requested byte range cannot be served.'],
  ['ExpectationFailed', 417, 'detailWithDefault', 'Expectation Failed', 'The expectation given in the Expect header could not be met.', 'rfc9110#section-15.5.18', 'The Expect header could not be satisfied.'],
  ['ImATeapot', 418, 'detailWithDefault', "I'm a Teapot", "I'm a teapot.", 'rfc9110#section-15.5.19', 'RFC 2324 easter egg. Yes, it is supported.'],
  ['MisdirectedRequest', 421, 'detailWithDefault', 'Misdirected Request', 'The request was directed to a server unable to produce a response.', 'rfc9110#section-15.5.20', 'The request reached a server that cannot answer for this authority.'],
  ['UnprocessableContent', 422, 'detail', 'Unprocessable Content', null, 'rfc9110#section-15.5.21', 'Well-formed but semantically invalid (validation). See also `Result.Invalid(...)`.'],
  ['Locked', 423, 'detailWithDefault', 'Locked', 'The resource is currently locked and cannot be modified.', 'rfc4918#section-11.3', 'WebDAV: the resource is locked.'],
  ['FailedDependency', 424, 'detail', 'Failed Dependency', null, 'rfc4918#section-11.4', 'The operation depended on another one that failed.'],
  ['TooEarly', 425, 'detailWithDefault', 'Too Early', 'The request was rejected because it may be replayed.', 'rfc8470#section-5.2', 'The server will not risk processing a possibly replayed request.'],
  ['UpgradeRequired', 426, 'detail', 'Upgrade Required', null, 'rfc9110#section-15.5.23', 'The client must switch to a different protocol.'],
  ['PreconditionRequired', 428, 'detail', 'Precondition Required', null, 'rfc6585#section-3', 'The server requires a conditional request (e.g. If-Match).'],
  ['TooManyRequests', 429, 'detailWithDefault', 'Too Many Requests', 'Too many requests. Please slow down and try again later.', 'rfc6585#section-4', 'Rate limiting.'],
  ['RequestHeaderFieldsTooLarge', 431, 'detailWithDefault', 'Request Header Fields Too Large', 'One or more request header fields exceed the maximum allowed size.', 'rfc6585#section-5', 'Headers are too large.'],
  ['UnavailableForLegalReasons', 451, 'detail', 'Unavailable For Legal Reasons', null, 'rfc7725#section-3', 'Blocked for legal reasons (censorship, court orders, GDPR).'],

  ['InternalServerError', 500, 'detailWithDefault', 'Internal Server Error', 'An unexpected error occurred. Please try again later.', 'rfc9110#section-15.6.1', 'Generic unexpected server failure.'],
  ['NotImplemented', 501, 'detailWithDefault', 'Not Implemented', 'This feature is not yet implemented.', 'rfc9110#section-15.6.2', 'The functionality is not supported (yet).'],
  ['BadGateway', 502, 'detailWithDefault', 'Bad Gateway', 'An upstream server returned an invalid response.', 'rfc9110#section-15.6.3', 'An upstream dependency returned garbage.'],
  ['ServiceUnavailable', 503, 'detailWithDefault', 'Service Unavailable', 'The service is temporarily unavailable. Please try again later.', 'rfc9110#section-15.6.4', 'Overloaded or down for maintenance.'],
  ['GatewayTimeout', 504, 'detailWithDefault', 'Gateway Timeout', 'An upstream server did not respond in time.', 'rfc9110#section-15.6.5', 'An upstream dependency timed out.'],
  ['HttpVersionNotSupported', 505, 'detailWithDefault', 'HTTP Version Not Supported', 'The HTTP version used in the request is not supported.', 'rfc9110#section-15.6.6', 'The HTTP protocol version is not supported.'],
  ['VariantAlsoNegotiates', 506, 'detailWithDefault', 'Variant Also Negotiates', 'The server has an internal configuration error.', 'rfc2295#section-8.1', 'Content negotiation misconfiguration.'],
  ['InsufficientStorage', 507, 'detailWithDefault', 'Insufficient Storage', 'Insufficient storage to complete the request.', 'rfc4918#section-11.5', 'WebDAV: not enough storage to complete the request.'],
  ['LoopDetected', 508, 'detailWithDefault', 'Loop Detected', 'The server detected an infinite loop while processing the request.', 'rfc5842#section-7.2', 'WebDAV: infinite loop detected.'],
  ['NotExtended', 510, 'detailWithDefault', 'Not Extended', 'Further extensions to the request are required.', 'rfc2774#section-7', 'Further extensions are required.'],
  ['NetworkAuthenticationRequired', 511, 'detailWithDefault', 'Network Authentication Required', 'Network authentication is required to access this resource.', 'rfc6585#section-6', 'Captive portals: authenticate with the network first.'],
];

const NON_GENERIC_SUCCESS = new Set(['Continue', 'SwitchingProtocols', 'Processing', 'EarlyHints', 'NoContent', 'ResetContent']);

export const RESULT_STATUSES: readonly ResultStatusInfo[] = ROWS.map(([name, code, factory, title, detail, rfc, summary]) => {
  const statusClass = Math.floor(code / 100) as StatusClass;
  return {
    name,
    code,
    statusClass,
    isSuccessful: statusClass < 4,
    factory,
    hasGenericFactory: !NON_GENERIC_SUCCESS.has(name),
    defaultTitle: title,
    defaultDetail: detail,
    locationArgument: factory === 'location' ? (name === 'UseProxy' ? 'proxy' : 'location') : null,
    problemType: rfc ? RFC + rfc : 'about:blank',
    summary,
  };
});

export const STATUS_CLASS_LABELS: Readonly<Record<StatusClass, string>> = {
  1: 'Informational',
  2: 'Success',
  3: 'Redirection',
  4: 'Client error',
  5: 'Server error',
};

export function findStatus(name: string): ResultStatusInfo | undefined {
  return RESULT_STATUSES.find(status => status.name.toLowerCase() === name.toLowerCase());
}

export function factoryName(status: ResultStatusInfo): string {
  return status.name === 'Ok' ? 'Success' : status.name;
}

export function isBodyless(status: ResultStatusInfo): boolean {
  return status.name === 'NoContent' || status.statusClass === 1 || status.statusClass === 3;
}

const PHRASE_OVERRIDES: Readonly<Record<string, string>> = {
  Ok: 'OK',
  ImUsed: 'IM Used',
  ImATeapot: "I'm a Teapot",
  UriTooLong: 'URI Too Long',
  HttpVersionNotSupported: 'HTTP Version Not Supported',
};

export function reasonPhrase(status: ResultStatusInfo): string {
  return PHRASE_OVERRIDES[status.name] ?? status.name.replace(/([a-z])([A-Z])/g, '$1 $2');
}
