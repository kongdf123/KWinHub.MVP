describe('scientific metric code naming convention', () => {
  const VALID_CODE = /^[A-Z][A-Z0-9]*(?:_[A-Z0-9]+)*$/;

  const VALID_EXAMPLES = [
    'CMJ_JUMP_HEIGHT_FLIGHT_TIME',
    'CMJ_JUMP_HEIGHT_IMPULSE_MOMENTUM',
    'SPRINT_REACTION_TIME_BLOCK',
    'SPRINT_PEAK_VELOCITY',
    'RSI_MOD',
    'PEAK_FORCE',
  ];

  const INVALID_EXAMPLES = [
    'cmj_jump_height',
    'CMJ-JUMP-HEIGHT',
    'JumpHeight',
    '_LEADING_UNDERSCORE',
    'LOWER_case_mixed',
  ];

  test.each(VALID_EXAMPLES)('accepts %s', (code) => {
    expect(VALID_CODE.test(code)).toBe(true);
  });

  test.each(INVALID_EXAMPLES)('rejects %s', (code) => {
    expect(VALID_CODE.test(code)).toBe(false);
  });
});