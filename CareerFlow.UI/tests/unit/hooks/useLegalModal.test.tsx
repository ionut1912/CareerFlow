import {renderHook, act} from '@testing-library/react-native';
import {getLegal} from '@/services/legalService';
import {useLegalModal} from '@/hooks/useLegalModal';

// Mock the external API service
jest.mock('@/services/legalService', () => ({
  getLegal: jest.fn(),
}));

const mockedGetLegal = getLegal as jest.Mock;

describe('useLegalModal', () => {
  const mockOnAccept = jest.fn();
  const mockOnReject = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('initializes with the correct default hidden state', () => {
    const {result} = renderHook(() =>
      useLegalModal(mockOnAccept, mockOnReject),
    );

    expect(result.current.modal).toEqual({
      visible: false,
      loading: false,
      title: '',
      content: '',
      type: '',
    });
  });

  it('opens the modal, sets loading state, and successfully fetches legal content', async () => {
    // Setup the mock to return a successful response
    mockedGetLegal.mockResolvedValueOnce({
      data: {content: 'Acesta este textul pentru termeni și condiții.'},
    });

    const {result} = renderHook(() =>
      useLegalModal(mockOnAccept, mockOnReject),
    );

    // Execute the async open function
    await act(async () => {
      await result.current.open('terms');
    });

    // Verify the API was called with the correct argument
    expect(mockedGetLegal).toHaveBeenCalledWith('terms');

    // Verify the final state after the promise resolves
    expect(result.current.modal).toEqual({
      visible: true,
      loading: false,
      title: 'Termeni și Condiții', // Mapped from LEGAL_TITLES
      content: 'Acesta este textul pentru termeni și condiții.',
      type: 'terms',
    });
  });

  it('handles API errors gracefully by showing a fallback error message', async () => {
    // Setup the mock to simulate a network failure or 500 error
    mockedGetLegal.mockRejectedValueOnce(new Error('Network Error'));

    const {result} = renderHook(() =>
      useLegalModal(mockOnAccept, mockOnReject),
    );

    await act(async () => {
      await result.current.open('privacy');
    });

    // Verify the fallback state is applied
    expect(result.current.modal).toEqual({
      visible: true,
      loading: false,
      title: 'Eroare',
      content: 'Eroare la încărcarea datelor.',
      type: 'privacy',
    });
  });

  it('closes the modal by setting visible to false without destroying other state', async () => {
    const {result} = renderHook(() =>
      useLegalModal(mockOnAccept, mockOnReject),
    );

    // First, force the modal into an open state manually
    act(() => {
      result.current.open('terms');
    });

    // Now close it
    act(() => {
      result.current.close();
    });

    expect(result.current.modal.visible).toBe(false);
  });

  it('triggers onAccept callback with the current type and closes the modal', () => {
    const {result} = renderHook(() =>
      useLegalModal(mockOnAccept, mockOnReject),
    );

    // Hydrate the state with a type so we can test the callback argument
    act(() => {
      // Bypassing the async fetch for a pure state interaction test
      result.current.open('privacy');
    });

    act(() => {
      result.current.handleAccept();
    });

    expect(mockOnAccept).toHaveBeenCalledTimes(1);
    expect(mockOnAccept).toHaveBeenCalledWith('privacy');
    expect(result.current.modal.visible).toBe(false); // Validates close() was called
  });

  it('triggers onReject callback with the current type and closes the modal', () => {
    const {result} = renderHook(() =>
      useLegalModal(mockOnAccept, mockOnReject),
    );

    act(() => {
      result.current.open('terms');
    });

    act(() => {
      result.current.handleReject();
    });

    expect(mockOnReject).toHaveBeenCalledTimes(1);
    expect(mockOnReject).toHaveBeenCalledWith('terms');
    expect(result.current.modal.visible).toBe(false);
  });

  it('does not crash if callbacks are not provided (optional chaining test)', async () => {
    // Instantiate without the optional callbacks
    const {result} = renderHook(() => useLegalModal());

    act(() => {
      result.current.open('privacy');
    });

    // This should execute smoothly without throwing "onAccept is not a function"
    expect(() => {
      act(() => {
        result.current.handleAccept();
        result.current.handleReject();
      });
    }).not.toThrow();
  });
});
