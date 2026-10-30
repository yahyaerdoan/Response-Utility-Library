export interface WizardOutcome {
  readonly kind: 'outcome';
  readonly code: number;
  readonly status: string;
  readonly snippet: string;
  readonly why: string;
  readonly note?: string;
}

export interface WizardQuestion {
  readonly kind: 'question';
  readonly id: string;
  readonly text: string;
  readonly help?: string;
  readonly yes: WizardNode;
  readonly no: WizardNode;
}

export type WizardNode = WizardQuestion | WizardOutcome;

const outcome = (code: number, status: string, snippet: string, why: string, note?: string): WizardOutcome => ({ kind: 'outcome', code, status, snippet, why, note });

const question = (id: string, text: string, yes: WizardNode, no: WizardNode, help?: string): WizardQuestion => ({ kind: 'question', id, text, help, yes, no });

const serverSide = question(
  'upstream',
  'Did an external provider fail (payment gateway, email service, another API)?',
  question(
    'upstream-timeout',
    'Did it time out rather than return an error?',
    outcome(504, 'GatewayTimeout', 'return Result.GatewayTimeout<OrderDto>("The payment provider did not respond in time.");', 'The dependency never answered; the caller may retry later.'),
    outcome(502, 'BadGateway', 'return Result.BadGateway<OrderDto>("Could not start payment for your order.");', 'Your server is fine, but the provider it depends on returned an error.'),
  ),
  outcome(
    500,
    'InternalServerError',
    'return Result.InternalServerError<OrderDto>(); // default, client-safe detail',
    'A step on your own server failed and the caller cannot fix it by changing the request.',
    'If this means a bug rather than an expected condition, throwing is fine: the exception middleware turns it into the same 500 Problem Details.',
  ),
);

const failure = question(
  'auth-flow',
  'Is this a sign-in, token refresh or password reset that failed (bad credentials, invalid token)?',
  outcome(
    400,
    'BadRequest',
    'return Result.BadRequest<AuthTokensResponse>("Invalid username or password.");',
    'OAuth 2 specifies 400 (invalid_grant) here.',
    'Avoid 401 on the sign-in endpoint itself: many clients treat any 401 as an expired session and log the user out.',
  ),
  question(
    'authenticated',
    'Is the caller signed in?',
    question(
      'allowed',
      'Is the caller allowed to do this?',
      question(
        'exists',
        'Does the resource the request targets exist?',
        question(
          'content-valid',
          'Is the request content valid on its own (formats, ranges, required fields, known keys)?',
          question(
            'state-allows',
            'Does the resource\'s current state allow this (not already cancelled, slot still free, name not taken)?',
            question(
              'rate',
              'Is the caller sending too many requests?',
              outcome(429, 'TooManyRequests', 'return Result.TooManyRequests<OrderDto>();', 'The request is fine; the caller has to slow down.'),
              serverSide,
            ),
            outcome(409, 'Conflict', 'return Result.Conflict<OrderDto>("This appointment slot is no longer available.");', 'The request is valid, but it conflicts with the resource\'s current state. Retrying the same request will not help until the state changes.'),
          ),
          outcome(
            422,
            'UnprocessableContent',
            'return Result.Invalid<OrderDto>("Quantity must be at least 1.");\n\n// or per field:\nreturn OperationDataResult<OrderDto>.Failure(new Dictionary<string, IReadOnlyList<string>>\n{\n    ["Quantity"] = ["Quantity must be at least 1."],\n});',
            'The request is well-formed but its content is invalid regardless of any state. The same request will always fail.',
            'Per-field errors let a form highlight each input. See field errors in the docs.',
          ),
        ),
        question(
          'gone',
          'Did it exist once but expire or get permanently removed (invite, one-time link)?',
          outcome(410, 'Gone', 'return Result.Gone("This invite has expired. Ask for a new one.");', 'The resource existed and will not come back; clients should stop asking for it.'),
          outcome(404, 'NotFound', 'return Result.NotFound<OrderDto>($"Order {id} does not exist.");', 'The target resource does not exist. A detail is required, so say what was not found.'),
        ),
      ),
      outcome(403, 'Forbidden', 'return Result.Forbidden<OrderDto>();', 'The caller is authenticated but lacks the permission. Signing in again will not help.'),
    ),
    outcome(401, 'Unauthorized', 'return Result.Unauthorized<OrderDto>();', 'The caller is not authenticated. Clients typically redirect to sign-in.'),
  ),
);

const success = question(
  'returns-data',
  'Does the response carry data?',
  question(
    'created',
    'Did the request create a new resource?',
    outcome(201, 'Created', 'return Result.Created(dto);', 'A new resource exists now; the body is the created resource.'),
    question(
      'accepted',
      'Was the work only queued, to finish later?',
      outcome(202, 'Accepted', 'return Result.Accepted(new JobDto(jobId));', 'The request was accepted for processing, which has not completed yet.'),
      outcome(200, 'Ok', 'return Result.Success(dto);', 'The standard success with a body.'),
    ),
  ),
  outcome(204, 'NoContent', 'return Result.NoContent();', 'Success with nothing to return, e.g. an update or a delete.'),
);

export const WIZARD_ROOT: WizardQuestion = question('succeeded', 'Did the operation succeed?', success, failure);
