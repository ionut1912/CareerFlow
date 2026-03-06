import {renderHook, act} from '@testing-library/react-native';
import {handleAcceptLegal, handleRejectLegal} from '@/app/(auth)/utils';
import {useLegalAcceptance} from '@/hooks/useLegalAcceptance';

// Mock the external utility functions
jest.mock('@/app/(auth)/utils', () => ({
  handleAcceptLegal: jest.fn(),
  handleRejectLegal: jest.fn(),
}));

describe('useLegalAcceptance', () => {
  beforeEach(() => {
    // Clear mock histories before each test to prevent test pollution
    jest.clearAllMocks();
  });

  it('initializes with false for all legal documents and incomplete status', () => {
    const {result} = renderHook(() => useLegalAcceptance());

    expect(result.current.legalAccepted).toEqual({
      terms: false,
      privacy: false,
    });
    expect(result.current.isLegalComplete).toBe(false);
  });

  it('handles accepting a single legal document and triggers the external utility', () => {
    const {result} = renderHook(() => useLegalAcceptance());

    act(() => {
      result.current.onAccept('terms');
    });

    // Verify internal state
    expect(result.current.legalAccepted.terms).toBe(true);
    expect(result.current.legalAccepted.privacy).toBe(false);
    expect(result.current.isLegalComplete).toBe(false); // Still incomplete

    // Verify external side effect
    expect(handleAcceptLegal).toHaveBeenCalledTimes(1);
    expect(handleAcceptLegal).toHaveBeenCalledWith('terms');
  });

  it('handles rejecting a legal document and triggers the external utility', () => {
    const {result} = renderHook(() => useLegalAcceptance());

    // First accept it so we can test changing it back to false
    act(() => {
      result.current.onAccept('privacy');
    });

    expect(result.current.legalAccepted.privacy).toBe(true);

    // Now reject it
    act(() => {
      result.current.onReject('privacy');
    });

    // Verify internal state
    expect(result.current.legalAccepted.privacy).toBe(false);
    expect(result.current.isLegalComplete).toBe(false);

    // Verify external side effect
    expect(handleRejectLegal).toHaveBeenCalledTimes(1);
    expect(handleRejectLegal).toHaveBeenCalledWith('privacy');
  });

  it('evaluates isLegalComplete to true only when both terms and privacy are accepted', () => {
    const {result} = renderHook(() => useLegalAcceptance());

    // Accept terms
    act(() => {
      result.current.onAccept('terms');
    });
    expect(result.current.isLegalComplete).toBe(false);

    // Accept privacy
    act(() => {
      result.current.onAccept('privacy');
    });
    expect(result.current.isLegalComplete).toBe(true); // Now complete

    // Reject one to ensure it flips back
    act(() => {
      result.current.onReject('terms');
    });
    expect(result.current.isLegalComplete).toBe(false); // Incomplete again
  });
});
