import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { createLogger, type LogLevel, type LogEntry } from './logger';

describe('createLogger', () => {
  let consoleSpy: {
    log: ReturnType<typeof vi.spyOn>;
    warn: ReturnType<typeof vi.spyOn>;
    error: ReturnType<typeof vi.spyOn>;
    debug: ReturnType<typeof vi.spyOn>;
  };

  beforeEach(() => {
    consoleSpy = {
      log: vi.spyOn(console, 'log').mockImplementation(() => {}),
      warn: vi.spyOn(console, 'warn').mockImplementation(() => {}),
      error: vi.spyOn(console, 'error').mockImplementation(() => {}),
      debug: vi.spyOn(console, 'debug').mockImplementation(() => {}),
    };
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('should log info messages', () => {
    const logger = createLogger({ level: 'info' });
    logger.info('test message');

    expect(consoleSpy.log).toHaveBeenCalledTimes(1);
    expect(consoleSpy.log).toHaveBeenCalledWith(
      expect.stringContaining('INFO: test message')
    );
  });

  it('should log with context', () => {
    const logger = createLogger({ level: 'info' });
    logger.info('test message', { key: 'value' });

    expect(consoleSpy.log).toHaveBeenCalledWith(
      expect.stringContaining('{"key":"value"}')
    );
  });

  it('should respect log level', () => {
    const logger = createLogger({ level: 'warn' });

    logger.debug('debug message');
    logger.info('info message');
    logger.warn('warn message');
    logger.error('error message');

    expect(consoleSpy.debug).not.toHaveBeenCalled();
    expect(consoleSpy.log).not.toHaveBeenCalled();
    expect(consoleSpy.warn).toHaveBeenCalledTimes(1);
    expect(consoleSpy.error).toHaveBeenCalledTimes(1);
  });

  it('should use correct console methods', () => {
    const logger = createLogger({ level: 'debug' });

    logger.debug('debug');
    logger.info('info');
    logger.warn('warn');
    logger.error('error');

    expect(consoleSpy.debug).toHaveBeenCalledTimes(1);
    expect(consoleSpy.log).toHaveBeenCalledTimes(1);
    expect(consoleSpy.warn).toHaveBeenCalledTimes(1);
    expect(consoleSpy.error).toHaveBeenCalledTimes(1);
  });

  it('should create child logger with inherited context', () => {
    const logger = createLogger({ level: 'info', context: { service: 'test' } });
    const child = logger.child({ requestId: '123' });

    child.info('child message');

    expect(consoleSpy.log).toHaveBeenCalledWith(
      expect.stringContaining('"service":"test"')
    );
    expect(consoleSpy.log).toHaveBeenCalledWith(
      expect.stringContaining('"requestId":"123"')
    );
  });

  it('should use custom output function', () => {
    const customOutput = vi.fn();
    const logger = createLogger({ level: 'info', output: customOutput });

    logger.info('test', { key: 'value' });

    expect(customOutput).toHaveBeenCalledTimes(1);
    expect(customOutput).toHaveBeenCalledWith(
      expect.objectContaining({
        level: 'info',
        message: 'test',
        context: { key: 'value' },
      })
    );
    expect(consoleSpy.log).not.toHaveBeenCalled();
  });
});
